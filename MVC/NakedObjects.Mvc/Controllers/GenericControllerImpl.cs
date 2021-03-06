// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Common.Logging;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core;
using NakedObjects.Core.Util;
using NakedObjects.Resources;
using NakedObjects.Surface;
using NakedObjects.Surface.Utility;
using NakedObjects.Web.Mvc.Models;

namespace NakedObjects.Web.Mvc.Controllers {
    public abstract class GenericControllerImpl : NakedObjectsController {
        private static readonly ILog Log = LogManager.GetLogger<GenericControllerImpl>();

        #region actions

        protected GenericControllerImpl(INakedObjectsSurface surface,  IIdHelper idHelper, IMessageBrokerSurface messageBroker) : base(surface, idHelper, messageBroker) {}

        [HttpGet]
        public virtual ActionResult Details(ObjectAndControlData controlData) {
            Assert.AssertTrue(controlData.SubAction == ObjectAndControlData.SubActionType.Details ||
                              controlData.SubAction == ObjectAndControlData.SubActionType.None);

            var nakedObject = controlData.GetNakedObject(Surface);
            nakedObject = FilterCollection(nakedObject, controlData);
            SetNewCollectionFormats(controlData);
            return AppropriateView(controlData, nakedObject);
        }

        [HttpGet]
        public virtual ActionResult EditObject(ObjectAndControlData controlData) {
            Assert.AssertTrue(controlData.SubAction == ObjectAndControlData.SubActionType.None);
            return View("ObjectEdit", controlData.GetNakedObject(Surface).Object);
        }

        // temp kludge 
        private void SetNotQueryable(INakedObjectSurface no, bool isNotQueryable) {
            INakedObjectAdapter noa = ((dynamic) no).WrappedNakedObject;
            noa.SetNotQueryable(isNotQueryable);
        }

        [HttpPost]
        public virtual ActionResult Details(ObjectAndControlData controlData, FormCollection form) {
            Decrypt(form);
            controlData.Form = form;
            Assert.AssertTrue(controlData.SubAction == ObjectAndControlData.SubActionType.Redisplay ||
                              controlData.SubAction == ObjectAndControlData.SubActionType.Details ||
                              controlData.SubAction == ObjectAndControlData.SubActionType.Cancel ||
                              controlData.SubAction == ObjectAndControlData.SubActionType.None);
            var nakedObject = FilterCollection(controlData.GetNakedObject(Surface), controlData);
            SetExistingCollectionFormats(form);
            SetNewCollectionFormats(controlData);

            // TODO temp hack 
            SetNotQueryable(nakedObject, true);

            if (controlData.SubAction == ObjectAndControlData.SubActionType.Cancel && nakedObject.IsTransient() && nakedObject.IsUserPersistable()) {
                // remove from cache and return to last object 
                Session.RemoveFromCache(Surface, nakedObject, ObjectCache.ObjectFlag.BreadCrumb);
                return AppropriateView(controlData, (INakedObjectSurface) null);
            }
            string property = DisplaySingleProperty(controlData, controlData.DataDict);
            return AppropriateView(controlData, nakedObject, null, property);
        }

        [HttpPost]
        public virtual ActionResult EditObject(ObjectAndControlData controlData, FormCollection form) {
            Decrypt(form);
            controlData.Form = form;
            var nakedObject = controlData.GetNakedObject(Surface);
            SetExistingCollectionFormats(form);

            if (nakedObject.IsNotPersistent()) {
                RefreshTransient(nakedObject, form);
            }

            switch (controlData.SubAction) {
                case (ObjectAndControlData.SubActionType.Action):
                    SetNewCollectionFormats(controlData);
                    return ActionOnNotPersistentObject(controlData);
                case (ObjectAndControlData.SubActionType.None):
                    AddAttemptedValuesNew(nakedObject, controlData);
                    return View("ObjectEdit", nakedObject.Object);
                case (ObjectAndControlData.SubActionType.Pager):
                    SetNewCollectionFormats(controlData);
                    return AppropriateView(controlData, nakedObject);
                case (ObjectAndControlData.SubActionType.Redisplay):
                    return Redisplay(controlData);
            }
            Log.ErrorFormat("SubAction handling not implemented in EditObject for {0}", controlData.SubAction.ToString());
            throw new NotImplementedException(controlData.SubAction.ToString());
        }

        [HttpPost]
        public virtual ActionResult Edit(ObjectAndControlData controlData, FormCollection form) {
            Decrypt(form);
            controlData.Form = form;
            AddFilesToControlData(controlData);

            var nakedObject = controlData.GetNakedObject(Surface);
            RefreshTransient(nakedObject, form);
            SetExistingCollectionFormats(form);
            AddAttemptedValuesNew(nakedObject, controlData);

            switch (controlData.SubAction) {
                case (ObjectAndControlData.SubActionType.Find):
                    return Find(controlData);
                case (ObjectAndControlData.SubActionType.Select):
                    return Select(controlData);
                case (ObjectAndControlData.SubActionType.ActionAsFind):
                    return ActionAsFind(controlData);
                case (ObjectAndControlData.SubActionType.InvokeActionAsFind):
                    return InvokeActionAsFind(controlData);
                case (ObjectAndControlData.SubActionType.InvokeActionAsSave):
                    return InvokeActionAsSave(controlData);
                case (ObjectAndControlData.SubActionType.Redisplay):
                    return Redisplay(controlData);
                case (ObjectAndControlData.SubActionType.None):
                    return ApplyEdit(controlData);
                case (ObjectAndControlData.SubActionType.SaveAndClose):
                    return ApplyEditAndClose(controlData);
                case (ObjectAndControlData.SubActionType.Action):
                    return ApplyEditAction(controlData);
            }
            Log.ErrorFormat("SubAction handling not implemented in Edit for {0}", controlData.SubAction.ToString());
            throw new NotImplementedException(controlData.SubAction.ToString());
        }

        [HttpGet]
        public virtual ActionResult Action(ObjectAndControlData controlData) {
            var no = controlData.GetNakedObject(Surface);
            var action = controlData.GetAction(Surface);

            return View("ActionDialog", new FindViewModelNew {
                ContextObject = no.Object,
                ContextAction = action // todo fix hack 
            });
        }

        [HttpPost]
        public virtual ActionResult Action(ObjectAndControlData controlData, FormCollection form) {
            Decrypt(form);
            controlData.Form = form;
            AddFilesToControlData(controlData);
            AddAttemptedValues(controlData);

            switch (controlData.SubAction) {
                case (ObjectAndControlData.SubActionType.Find):
                    return Find(controlData);
                case (ObjectAndControlData.SubActionType.Select):
                    return SelectOnAction(controlData);
                case (ObjectAndControlData.SubActionType.ActionAsFind):
                    return ActionAsFind(controlData);
                case (ObjectAndControlData.SubActionType.InvokeActionAsFind):
                    return InvokeActionAsFind(controlData);
                case (ObjectAndControlData.SubActionType.InvokeActionAsSave):
                    return InvokeActionAsSave(controlData);
                case (ObjectAndControlData.SubActionType.Action):
                    return InitialAction(controlData);
                case (ObjectAndControlData.SubActionType.Details):
                    return Details(controlData);
                case (ObjectAndControlData.SubActionType.None):
                    SetNewCollectionFormats(controlData);
                    return ApplyAction(controlData);
            }

            Log.ErrorFormat("SubAction handling not implemented in Action for {0}", controlData.SubAction.ToString());
            throw new NotImplementedException(controlData.SubAction.ToString());
        }

        public virtual FileContentResult GetFile(string Id, string PropertyId) {
            //INakedObjectAdapter target = NakedObjectsContext.GetNakedObjectFromId(Id);
            //IAssociationSpec assoc = target.GetObjectSpec().Properties.Single(a => a.Id == PropertyId);
            //var domainObject = assoc.GetNakedObject(target).GetDomainObject();

            var oid = Surface.OidStrategy.GetOid(Id, "");
            var tgt = Surface.GetObject(oid).Target;

            var p = Surface.GetProperty(oid, PropertyId);
            var domainObject = p.Property.GetNakedObject(tgt).Object;

            return AsFile(domainObject);
        }

        #endregion

        #region private

        private ActionResult ActionOnNotPersistentObject(ObjectAndControlData controlData) {
            string targetActionId = controlData.DataDict["targetActionId"];
            string targetObjectId = controlData.DataDict["targetObjectId"];

            var targetNakedObject = GetNakedObjectFromId(targetObjectId);
            if (targetNakedObject.Specification.IsCollection()) {
                var filteredNakedObject = FilterCollection(targetNakedObject, controlData);
                //var metamodel = NakedObjectsContext.MetamodelManager.Metamodel;
                //IObjectSpecImmutable elementSpecImmut =
                //    filteredNakedObject.Spec.GetFacet<ITypeOfFacet>().GetValueSpec(filteredNakedObject, metamodel);

                var elementSpec = targetNakedObject.ElementSpecification;
                Trace.Assert(elementSpec != null);
                var targetAction = elementSpec.GetCollectionContributedActions().Single(a => a.Id == targetActionId);

                if (!filteredNakedObject.ToEnumerable().Any()) {
                    MessageBroker.AddWarning("No objects selected");
                    return AppropriateView(controlData, targetNakedObject, targetAction);
                }

                // force any result to not be queryable
                //filteredNakedObject.SetNotQueryable(true);
                // TODO temp hack 
                SetNotQueryable(filteredNakedObject, true);

                return ExecuteAction(controlData, filteredNakedObject, targetAction);
            }
            else {
                var oid = Surface.OidStrategy.GetOid(targetNakedObject);
                var targetAction = Surface.GetObjectAction(oid, targetActionId).Action;

                //var targetAction = NakedObjectsContext.GetActions(targetNakedObject).Single(a => a.Id == targetActionId);
                return ExecuteAction(controlData, targetNakedObject, targetAction);
            }
        }

        private INakedObjectAdapter Execute(IActionSpec action, INakedObjectAdapter target, INakedObjectAdapter[] parameterSet) {
            return action.Execute(target, parameterSet);
        }

        private INakedObjectSurface GetResult(ActionResultContextSurface context) {
            if (context.HasResult) {
                return context.Result.Target;
            }
            return null;
        }

        private ActionResult ExecuteAction(ObjectAndControlData controlData, INakedObjectSurface nakedObject, INakedObjectActionSurface action) {
            if (ActionExecutingAsContributed(action, nakedObject) && action.ParameterCount == 1) {
                // contributed action being invoked with a single parm that is the current target
                //// no dialog - go straight through 
                //var newForm = new FormCollection { { IdHelper.GetParameterInputId(ScaffoldAction.Wrap(action), ScaffoldParm.Wrap(action.Parameters.First())), NakedObjectsContext.GetObjectId(nakedObject) } };

                //// horrid kludge 
                //var oldForm = controlData.Form;
                //controlData.Form = newForm;

                //if (ValidateParameters(nakedObject, action, controlData)) {
                var ac = new ArgumentsContext() {Values = new Dictionary<string, object>(), ValidateOnly = false};
                var oid = Surface.OidStrategy.GetOid(nakedObject);
                var result = Surface.ExecuteObjectAction(oid, action.Id, ac);
                return AppropriateView(controlData, GetResult(result), action);
                //}

                //controlData.Form = oldForm;
                //AddAttemptedValues(controlData);
            }

            if (!action.Parameters.Any()) {
                var ac = new ArgumentsContext() {Values = new Dictionary<string, object>(), ValidateOnly = false};
                var oid = Surface.OidStrategy.GetOid(nakedObject);
                var result = Surface.ExecuteObjectAction(oid, action.Id, ac);

                return AppropriateView(controlData, GetResult(result), action);
            }

            SetDefaults(nakedObject, action);
            // do after any parameters set by contributed action so this takes priority
            SetSelectedParameters(action);
            SetPagingValues(controlData, nakedObject);
            var property = DisplaySingleProperty(controlData, controlData.DataDict);

            return View(property == null ? "ActionDialog" : "PropertyEdit", new FindViewModelNew {ContextObject = nakedObject.Object, ContextAction = action, PropertyName = property});
        }

        private ActionResult InitialAction(ObjectAndControlData controlData) {
            var nakedObject = controlData.GetNakedObject(Surface);
            var nakedObjectAction = controlData.GetAction(Surface);
            CheckConcurrency(nakedObject, null, controlData, (z, x, y) => IdHelper.GetConcurrencyActionInputId(x, nakedObjectAction, y));
            return ExecuteAction(controlData, nakedObject, nakedObjectAction);
        }

        private bool HasError(ActionResultContextSurface ar) {
            return !string.IsNullOrEmpty(ar.ActionContext.Reason) || ar.ActionContext.VisibleParameters.Any(p => !string.IsNullOrEmpty(p.Reason));
        }

        private ActionResult ApplyAction(ObjectAndControlData controlData) {
            var targetNakedObject = FilterCollection(controlData.GetNakedObject(Surface), controlData);
            var targetAction = controlData.GetAction(Surface);

            CheckConcurrency(targetNakedObject, null, controlData, (z, x, y) => IdHelper.GetConcurrencyActionInputId(x, targetAction, y));

            if (targetNakedObject.IsNotPersistent()) {
                RefreshTransient(targetNakedObject, controlData.Form);
            }

            // do after any parameters set by contributed action so this takes priority
            SetSelectedParameters(targetAction);

            var ac = GetParameterValues(targetAction, controlData);
            var oid = Surface.OidStrategy.GetOid(targetNakedObject);
            var ar = Surface.ExecuteObjectAction(oid, targetAction.Id, ac);

            if (!HasError(ar)) {
                SetNotQueryable(targetNakedObject, targetAction.IsContributed()); // kludge

                return AppropriateView(controlData, GetResult(ar), targetAction);
            }

            foreach (var parm in ar.ActionContext.VisibleParameters) {
                if (!string.IsNullOrEmpty(parm.Reason)) {
                    ModelState.AddModelError(IdHelper.GetParameterInputId(targetAction, parm.Parameter), parm.Reason);
                }
            }

            if (!(string.IsNullOrEmpty(ar.ActionContext.Reason))) {
                ModelState.AddModelError("", ar.ActionContext.Reason);
            }

            var property = DisplaySingleProperty(controlData, controlData.DataDict);
            return View(property == null ? "ActionDialog" : "PropertyEdit", new FindViewModelNew {ContextObject = targetNakedObject.Object, ContextAction = targetAction, PropertyName = property});
        }

        private ActionResult Find(ObjectAndControlData controlData) {
            string spec = controlData.DataDict["spec"];
            string contextObjectId = controlData.DataDict["contextObjectId"];
            string propertyName = controlData.DataDict["propertyName"];
            string contextActionId = controlData.DataDict["contextActionId"];

            var objectSet = Session.CachedObjectsOfType(Surface, Surface.GetDomainType(spec)).ToList();

            if (!objectSet.Any()) {
                Log.InfoFormat("No Cached objects of type {0} found", spec);
                MessageBroker.AddWarning("No objects of appropriate type viewed recently");
            }
            var contextNakedObject = FilterCollection(GetNakedObjectFromId(contextObjectId), controlData);
            var contextAction = string.IsNullOrEmpty(contextActionId) ? null : contextNakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == contextActionId);

            if (objectSet.Count == 1) {
                var selectedItem = new Dictionary<string, string> {{propertyName, GetObjectId(objectSet.Single())}};
                return SelectSingleItem(contextNakedObject, contextAction, controlData, selectedItem);
            }

            return View(Request.IsAjaxRequest() ? "PropertyEdit" : "FormWithSelections", new FindViewModelNew {ActionResult = objectSet, ContextObject = contextNakedObject.Object, ContextAction = contextAction, PropertyName = propertyName});
        }

        private ActionResult SelectSingleItem(INakedObjectSurface nakedObject, INakedObjectActionSurface action, ObjectAndControlData controlData, IDictionary<string, string> selectedItem) {
            var property = DisplaySingleProperty(controlData, selectedItem);

            if (action == null) {
                SetSelectedReferences(nakedObject, selectedItem);
                return property == null ? View("ObjectEdit", nakedObject.Object) : View("PropertyEdit", new PropertyViewModel(nakedObject.Object, property));
            }
            SetSelectedParameters(nakedObject, action, selectedItem);

            return View(property == null ? "ActionDialog" : "PropertyEdit", new FindViewModelNew {ContextObject = nakedObject.Object, ContextAction = action, PropertyName = property});
        }

        private bool HasError(ObjectContextSurface ar) {
            return !string.IsNullOrEmpty(ar.Reason) || ar.VisibleProperties.Any(p => !string.IsNullOrEmpty(p.Reason));
        }

        private bool ApplyEdit(INakedObjectSurface nakedObject, ObjectAndControlData controlData) {
            //string viewName = "ObjectEdit";

            var oid = Surface.OidStrategy.GetOid(nakedObject);

            var usableAndVisibleFields = nakedObject.Specification.Properties.Where(p => p.IsVisible(nakedObject) && p.IsUsable(nakedObject).IsAllowed);
            var fieldsAndMatchingValues = GetFieldsAndMatchingValues(nakedObject, null, usableAndVisibleFields, controlData, GetFieldInputId).ToList();

            CheckConcurrency(nakedObject, null, controlData, GetConcurrencyFieldInputId);

            fieldsAndMatchingValues.ForEach(pair => AddAttemptedValue(GetFieldInputId(null, nakedObject, pair.Item1), pair.Item2));

            var ac = new ArgumentsContext {
                Values = fieldsAndMatchingValues.ToDictionary(f => f.Item1.Id, f => f.Item2),
                ValidateOnly = false
            };

            // check mandatory fields first to emulate WPF UI behaviour where no validation takes place until 
            // all mandatory fields are set. 
            foreach (var pair in fieldsAndMatchingValues) {
                var result = pair.Item2;
                var stringResult = result as string;

                if (pair.Item1.IsMandatory() && (result == null || (result is string && string.IsNullOrEmpty(stringResult)))) {
                    AddErrorAndAttemptedValue(nakedObject, stringResult, pair.Item1, MvcUi.Mandatory);
                }
            }

            if (!ModelState.IsValid) {
                return false;
            }

            var res = Surface.PutObject(oid, ac);

            if (HasError(res)) {
                foreach (var parm in res.VisibleProperties) {
                    if (!string.IsNullOrEmpty(parm.Reason)) {
                        ModelState.AddModelError(IdHelper.GetFieldInputId(nakedObject, parm.Property), parm.Reason);
                    }
                }

                if (!(string.IsNullOrEmpty(res.Reason))) {
                    ModelState.AddModelError("", res.Reason);
                }

                return false;
            }

            return true;
        }

        private ActionResult ApplyEdit(ObjectAndControlData controlData) {
            //string viewName = "ObjectEdit";
            var nakedObject = controlData.GetNakedObject(Surface);

            var viewName = ApplyEdit(nakedObject, controlData) ? "ObjectView" : "ObjectEdit";

            return View(viewName, nakedObject.Object);
        }

        private ActionResult ApplyEditAndClose(ObjectAndControlData controlData) {
            var nakedObject = controlData.GetNakedObject(Surface);
            if (ApplyEdit(nakedObject, controlData)) {
                // last object or home
                object lastObject = Session.LastObject(Surface, ObjectCache.ObjectFlag.BreadCrumb);
                if (lastObject == null) {
                    return RedirectHome();
                }

                var oid = Surface.OidStrategy.GetOid(lastObject);
                nakedObject = Surface.GetObject(oid).Target;
                return AppropriateView(controlData, nakedObject);
            }
            return View("ObjectEdit", nakedObject.Object);
        }

        private ActionResult ApplyEditAction(ObjectAndControlData controlData) {
            var nakedObject = controlData.GetNakedObject(Surface);
            var ok = ApplyEdit(nakedObject, controlData);
            if (ok) {
                string targetActionId = controlData.DataDict["targetActionId"];
                var oid = Surface.OidStrategy.GetOid(nakedObject);
                var targetAction = Surface.GetObjectAction(oid, targetActionId).Action;
                return ExecuteAction(controlData, nakedObject, targetAction);
            }
            return View("ViewModel", nakedObject.Object);
        }

        private ActionResult Redisplay(ObjectAndControlData controlData) {
            SetNewCollectionFormats(controlData);
            var property = DisplaySingleProperty(controlData, controlData.DataDict);
            var isEdit = bool.Parse(controlData.DataDict["editMode"]);
            var nakedObject = controlData.GetNakedObject(Surface);
            return property == null ? View(isEdit ? "ObjectEdit" : "ObjectView", nakedObject.Object) :
                View(isEdit ? "PropertyEdit" : "PropertyView", new PropertyViewModel(nakedObject.Object, property));
        }

        private ActionResult Select(ObjectAndControlData controlData) {
            return SelectSingleItem(controlData.GetNakedObject(Surface), null, controlData, controlData.DataDict);
        }

        private ActionResult SelectOnAction(ObjectAndControlData controlData) {
            var nakedObjectAction = controlData.GetAction(Surface);
            var contextNakedObject = FilterCollection(controlData.GetNakedObject(Surface), controlData);

            return SelectSingleItem(contextNakedObject, nakedObjectAction, controlData, controlData.DataDict);
        }


        //internal bool ValidateChanges(INakedObjectAdapter nakedObject, ObjectAndControlData controlData, IAssociationSpec parent = null) {
        //    List<IAssociationSpec> usableAndVisibleFields;
        //    List<Tuple<IAssociationSpec, object>> fieldsAndMatchingValues;
        //    GetUsableAndVisibleFields(nakedObject, controlData, parent, out usableAndVisibleFields, out fieldsAndMatchingValues);

        //    CheckConcurrency(nakedObject, parent, controlData, GetConcurrencyFieldInputId);

        //    fieldsAndMatchingValues.ForEach(pair => AddAttemptedValue(GetFieldInputId(parent, nakedObject, pair.Item1), pair.Item2));

        //    // check mandatory fields first to emulate WPF UI behaviour where no validation takes place until 
        //    // all mandatory fields are set. 
        //    foreach (var pair in fieldsAndMatchingValues) {
        //        var result = pair.Item2;
        //        var stringResult = result as string;

        //        if (pair.Item1.IsMandatory && (result == null || (result is string && string.IsNullOrEmpty(stringResult)))) {
        //            AddErrorAndAttemptedValue(nakedObject, stringResult, pair.Item1, MvcUi.Mandatory, parent);
        //        }
        //    }

        //    if (ModelState.IsValid) {
        //        ValidateOrApplyInlineChanges(nakedObject, controlData, usableAndVisibleFields, ValidateChanges);
        //    }

        //    if (ModelState.IsValid) {
        //        foreach (var pair in fieldsAndMatchingValues) {
        //            var spec = pair.Item1 as IOneToOneAssociationSpec;
        //            if (spec != null) {
        //                ValidateAssociation(nakedObject, spec, pair.Item2, parent);
        //            }
        //        }
        //    }

        //    if (ModelState.IsValid) {
        //        var validateFacet = nakedObject.Spec.GetFacet<IValidateObjectFacet>();

        //        if (validateFacet != null) {
        //            var parms = fieldsAndMatchingValues.Select(t => new Tuple<string, INakedObjectAdapter>(t.Item1.Id.ToLower(), GetNakedObjectValue(t.Item1, nakedObject, t.Item2))).ToArray();
        //            var result = validateFacet.ValidateParms(nakedObject, parms);

        //            if (!string.IsNullOrEmpty(result)) {
        //                ModelState.AddModelError(string.Empty, result);
        //            }
        //        }
        //    }

        //    if (ModelState.IsValid) {
        //        if (nakedObject.Spec.ContainsFacet<IValidateProgrammaticUpdatesFacet>()) {
        //            string state = nakedObject.ValidToPersist();
        //            if (state != null) {
        //                ModelState.AddModelError(string.Empty, state);
        //            }
        //        }
        //    }

        //    return ModelState.IsValid;
        //}

        private ActionResult InvokeActionAsSave(ObjectAndControlData controlData) {
            var form = controlData.Form;
            string targetActionId = controlData.DataDict["targetActionId"];
            string targetObjectId = controlData.DataDict["targetObjectId"];
            string contextObjectId = controlData.DataDict["contextObjectId"];
            string propertyName = controlData.DataDict["propertyName"];
            string contextActionId = controlData.DataDict["contextActionId"];
            string subEditObjectId = controlData.DataDict["subEditObjectId"];

            var targetNakedObject = GetNakedObjectFromId(targetObjectId);
            var contextNakedObject = FilterCollection(GetNakedObjectFromId(contextObjectId), controlData);
            var targetAction = targetNakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == targetActionId);
            var contextAction = string.IsNullOrEmpty(contextActionId) ? null : contextNakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == contextActionId);
            var subEditObject = GetNakedObjectFromId(subEditObjectId);

            //if (ValidateChanges(subEditObject, controlData)) {
            //    ApplyChanges(subEditObject, controlData);
            //}

            var oid = Surface.OidStrategy.GetOid(subEditObject);
            var ac = ConvertForSave(subEditObject, controlData);

            var result = Surface.PutObject(oid, ac);

            foreach (var p in result.VisibleProperties) {        
                if (!string.IsNullOrEmpty(p.Reason)) {
                    string key = GetFieldInputId(null, subEditObject, p.Property);
                    ModelState.AddModelError(key, p.Reason);
                    AddAttemptedValue(key, p.Property.Specification.IsParseable() ? p.ProposedValue : p.ProposedNakedObject);
                }
            }

            if (!string.IsNullOrEmpty(result.Reason)) {
                ModelState.AddModelError(string.Empty, result.Reason);
            }

            // tempting to try to associate the new object at once - however it is still transient until the end of the 
            // transaction and so association may not work (possible persistent to transient). By doing this we split into two transactions 
            // and so all OK. 

            IEnumerable resultAsEnumerable = new List<object> {subEditObject.Object};
            return View(Request.IsAjaxRequest() ? "PropertyEdit" : "FormWithSelections", new FindViewModelNew {
                ActionResult = resultAsEnumerable,
                TargetObject = targetNakedObject.Object,
                ContextObject = contextNakedObject.Object,
                TargetAction = targetAction,
                ContextAction = contextAction,
                PropertyName = propertyName
            });
        }

        private ActionResult InvokeActionAsFind(ObjectAndControlData controlData) {
            string targetActionId = controlData.DataDict["targetActionId"];
            string targetObjectId = controlData.DataDict["targetObjectId"];
            string contextObjectId = controlData.DataDict["contextObjectId"];
            string propertyName = controlData.DataDict["propertyName"];
            string contextActionId = controlData.DataDict["contextActionId"];

            var targetNakedObject = GetNakedObjectFromId(targetObjectId);
            var contextNakedObject = FilterCollection(GetNakedObjectFromId(contextObjectId), controlData);
            var targetAction = targetNakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == targetActionId);
            var contextAction = string.IsNullOrEmpty(contextActionId) ? null : contextNakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == contextActionId);

            SetContextObjectAsParameterValue(targetAction, contextNakedObject);

            var oid = Surface.OidStrategy.GetOid(targetNakedObject);
            var parms = GetParameterValues(targetAction, controlData);
            var context = Surface.ExecuteObjectAction(oid, targetActionId, parms);

            var result = GetResult(context);

            if (result != null) {
                IEnumerable resultAsEnumerable = !result.Specification.IsCollection() ? new List<object> {result.Object} : (IEnumerable) result.Object;

                if (resultAsEnumerable.Cast<object>().Count() == 1) {
                    var selectedItem = new Dictionary<string, string> {{propertyName, GetObjectId(resultAsEnumerable.Cast<object>().Single())}};
                    return SelectSingleItem(contextNakedObject, contextAction, controlData, selectedItem);
                }
                string view = Request.IsAjaxRequest() ? "PropertyEdit" : "FormWithSelections";

                return View(view, new FindViewModelNew {
                    ActionResult = resultAsEnumerable,
                    TargetObject = targetNakedObject.Object,
                    ContextObject = contextNakedObject.Object,
                    TargetAction = (targetAction),
                    ContextAction = (contextAction),
                    PropertyName = propertyName
                });
            }

            return View(Request.IsAjaxRequest() ? "PropertyEdit" : "FormWithFinderDialog", new FindViewModelNew {
                TargetObject = targetNakedObject.Object,
                ContextObject = contextNakedObject.Object,
                TargetAction = (targetAction),
                ContextAction = (contextAction),
                PropertyName = propertyName
            });
        }

        private static bool ContextParameterIsCollection(INakedObjectActionSurface contextAction, string propertyName) {
            if (contextAction != null) {
                var parameter = contextAction.Parameters.Single(p => p.Id == propertyName);
                return parameter.Specification.IsCollection();
            }
            return false;
        }

        private static bool ContextParameterIsCollection(IActionSpec contextAction, string propertyName) {
            if (contextAction != null) {
                IActionParameterSpec parameter = contextAction.Parameters.Single(p => p.Id == propertyName);
                return parameter.Spec.IsCollection;
            }
            return false;
        }

        private string GetObjectId(object domainObject) {
            return Surface.OidStrategy.GetOid(domainObject).ToString();
        }

        private ActionResult ActionAsFind(ObjectAndControlData controlData) {
            string targetActionId = controlData.DataDict["targetActionId"];
            string targetObjectId = controlData.DataDict["targetObjectId"];
            string contextObjectId = controlData.DataDict["contextObjectId"];
            string propertyName = controlData.DataDict["propertyName"];
            string contextActionId = controlData.DataDict["contextActionId"];

            var targetNakedObject = GetNakedObjectFromId(targetObjectId);
            var contextNakedObject = FilterCollection(GetNakedObjectFromId(contextObjectId), controlData);
            var targetAction = targetNakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == targetActionId);
            var contextAction = string.IsNullOrEmpty(contextActionId) ? null : contextNakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == contextActionId);

            SetContextObjectAsParameterValue(targetAction, contextNakedObject);
            if (targetAction.ParameterCount == 0) {
                //var result = Execute(targetAction, targetNakedObject, new INakedObjectAdapter[] {});

                var oid = Surface.OidStrategy.GetOid(targetNakedObject);

                var context = Surface.ExecuteObjectAction(oid, targetAction.Id, new ArgumentsContext() {
                    Values = new Dictionary<string, object>(),
                    ValidateOnly = false
                });

                var result = GetResult(context);

                IEnumerable resultAsEnumerable = GetResultAsEnumerable(result, contextAction, propertyName);

                if (resultAsEnumerable.Cast<object>().Count() == 1 && !result.IsTransient()) {
                    var selectedItem = new Dictionary<string, string> {{propertyName, GetObjectId(resultAsEnumerable.Cast<object>().Single())}};
                    return SelectSingleItem(contextNakedObject, contextAction, controlData, selectedItem);
                }

                string view = Request.IsAjaxRequest() ? "PropertyEdit" : "FormWithSelections";
                return View(view, new FindViewModelNew {
                    ActionResult = resultAsEnumerable,
                    TargetObject = targetNakedObject.Object,
                    ContextObject = contextNakedObject.Object,
                    TargetAction = (targetAction),
                    ContextAction = (contextAction),
                    PropertyName = propertyName
                });
            }

            SetDefaults(targetNakedObject, targetAction);
            return View(Request.IsAjaxRequest() ? "PropertyEdit" : "FormWithFinderDialog", new FindViewModelNew {
                TargetObject = targetNakedObject.Object,
                ContextObject = contextNakedObject.Object,
                TargetAction = (targetAction),
                ContextAction = (contextAction),
                PropertyName = propertyName
            });
        }

        private static IEnumerable GetResultAsEnumerable(INakedObjectSurface result, INakedObjectActionSurface contextAction, string propertyName) {
            if (result != null) {
                if (result.Specification.IsCollection() && !ContextParameterIsCollection(contextAction, propertyName)) {
                    return (IEnumerable) result.Object;
                }
                return new List<object> {result.Object};
            }
            return new List<object>();
        }

        private static IEnumerable GetResultAsEnumerable(INakedObjectAdapter result, IActionSpec contextAction, string propertyName) {
            if (result != null) {
                if (result.Spec.IsCollection && !ContextParameterIsCollection(contextAction, propertyName)) {
                    return (IEnumerable) result.Object;
                }
                return new List<object> {result.Object};
            }
            return new List<object>();
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext) {
            base.OnActionExecuted(filterContext); // end the transaction 
            UpdateViewAndController(filterContext);
        }

        protected override void OnException(ExceptionContext filterContext) {
            if (filterContext.Exception != null) {
                Exception e = filterContext.Exception;
                Log.ErrorFormat("GenericControllerImpl:OnException handling Type: {0} Message : {1} Trace : {2}", e.GetType(), e.Message, e.StackTrace);
            }
            else {
                // assume this will never happen. 
                Log.Error("GenericControllerImpl:OnException handling exception but exception is null");
            }

            if (filterContext.Exception is DataUpdateException) {
                filterContext.Result = View("DataUpdateError", filterContext.Exception);
                filterContext.ExceptionHandled = true;
            }
            else if (filterContext.Exception is ConcurrencyException) {
                filterContext.Result = View("ConcurrencyError", filterContext.Exception);
                filterContext.ExceptionHandled = true;
            }
            else if (filterContext.Exception is ObjectNotFoundException) {
                filterContext.Result = View("DestroyedError");
                filterContext.ExceptionHandled = true;
            }
            else if (filterContext.Exception is NakedObjectDomainException) {
                filterContext.Result = View("DomainError", filterContext.Exception);
                filterContext.ExceptionHandled = true;
            }

            base.OnException(filterContext);
        }

        #endregion
    }
}