﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Core.Util;
using NakedObjects.Surface;
using NakedObjects.Surface.Utility;
using NakedObjects.Web.Mvc.Models;

namespace NakedObjects.Web.Mvc.Controllers {
    public abstract class CustomController : NakedObjectsController {
        protected CustomController(INakedObjectsSurface surface, IIdHelper idHelper, IMessageBrokerSurface messageBroker) : base(surface, idHelper, messageBroker) { }
        public IDomainObjectContainer Container { set; protected get; }

        #region Actions

        #region Invoke Action - return result

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Return result and update valid bool to indicate if
        ///     parameters valid.
        /// </summary>
        protected T InvokeAction<T>(object domainObject, string actionName, FormCollection parameters, out bool valid) {
            var nakedObject = GetNakedObject(domainObject);
            var action = nakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == actionName);
            return InvokeAction<T>(nakedObject, action, parameters, out valid);
        }

        #region Invoke Action - func lambda 

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Return result and update valid bool to indicate if
        ///     parameters valid.
        /// </summary>
        protected TResult InvokeAction<TTarget, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TResult>>> expression, FormCollection parameters, out bool valid) {
            return InvokeAction<TResult>(GetNakedObject(domainObject), expression, parameters, out valid);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Return result and update valid bool to indicate if
        ///     parameters valid.
        /// </summary>
        protected TResult InvokeAction<TTarget, TParm, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm, TResult>>> expression, FormCollection parameters, out bool valid) {
            return InvokeAction<TResult>(GetNakedObject(domainObject), expression, parameters, out valid);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Return result and update valid bool to indicate if
        ///     parameters valid.
        /// </summary>
        protected TResult InvokeAction<TTarget, TParm1, TParm2, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm1, TParm2, TResult>>> expression, FormCollection parameters, out bool valid) {
            return InvokeAction<TResult>(GetNakedObject(domainObject), expression, parameters, out valid);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Return result and update valid bool to indicate if
        ///     parameters valid.
        /// </summary>
        protected TResult InvokeAction<TTarget, TParm1, TParm2, TParm3, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm1, TParm2, TParm3, TResult>>> expression, FormCollection parameters, out bool valid) {
            return InvokeAction<TResult>(GetNakedObject(domainObject), expression, parameters, out valid);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Return result and update valid bool to indicate if
        ///     parameters valid.
        /// </summary>
        protected TResult InvokeAction<TTarget, TParm1, TParm2, TParm3, TParm4, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm1, TParm2, TParm3, TParm4, TResult>>> expression, FormCollection parameters, out bool valid) {
            return InvokeAction<TResult>(GetNakedObject(domainObject), expression, parameters, out valid);
        }

        #endregion

        #endregion

        #region Invoke Action - return View 

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction(object domainObject, string actionName, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            var nakedObject = GetNakedObject(domainObject);
            return InvokeAction(nakedObject, actionName, parameters, viewNameForFailure, viewNameForSuccess);
        }

       

        #region Invoke Action - action lambda 

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget>(TTarget domainObject, Expression<Func<TTarget, Action>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm>(TTarget domainObject, Expression<Func<TTarget, Action<TParm>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm1, TParm2>(TTarget domainObject, Expression<Func<TTarget, Action<TParm1, TParm2>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm1, TParm2, TParm3>(TTarget domainObject, Expression<Func<TTarget, Action<TParm1, TParm2, TParm3>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm1, TParm2, TParm3, TParm4>(TTarget domainObject, Expression<Func<TTarget, Action<TParm1, TParm2, TParm3, TParm4>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        #endregion

        #region Invoke Action - func lambda

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm1, TParm2, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm1, TParm2, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm1, TParm2, TParm3, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm1, TParm2, TParm3, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<TTarget, TParm1, TParm2, TParm3, TParm4, TResult>(TTarget domainObject, Expression<Func<TTarget, Func<TParm1, TParm2, TParm3, TParm4, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(domainObject, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        #endregion

        #endregion

        #region Invoke Action with Object Id - return view

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction(string objectId, string actionName, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            var nakedObject = GetNakedObjectFromId(objectId);
            return InvokeAction(nakedObject, actionName, parameters, viewNameForFailure, viewNameForSuccess);
        }

        #region Invoke Action - action lambda 

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T>(string objectId, Expression<Func<T, Action>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm>(string objectId, Expression<Func<T, Action<TParm>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm1, TParm2>(string objectId, Expression<Func<T, Action<TParm1, TParm2>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm1, TParm2, TParm3>(string objectId, Expression<Func<T, Action<TParm1, TParm2, TParm3>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm1, TParm2, TParm3, TParm4>(string objectId, Expression<Func<T, Action<TParm1, TParm2, TParm3, TParm4>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        #endregion

        #region Invoke Action - func lambda

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TResult>(string objectId, Expression<Func<T, Func<TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm, TResult>(string objectId, Expression<Func<T, Func<TParm, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm1, TParm2, TResult>(string objectId, Expression<Func<T, Func<TParm1, TParm2, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm1, TParm2, TParm3, TResult>(string objectId, Expression<Func<T, Func<TParm1, TParm2, TParm3, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        /// <summary>
        ///     Invoke the action on the domain object with the parameters in the form. Got to appropriate view based on result
        /// </summary>
        protected ViewResult InvokeAction<T, TParm1, TParm2, TParm3, TParm4, TResult>(string objectId, Expression<Func<T, Func<TParm1, TParm2, TParm3, TParm4, TResult>>> expression, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            return InvokeAction(objectId, (LambdaExpression) expression, parameters, viewNameForFailure, viewNameForSuccess);
        }

        #endregion

        #endregion

        /// <summary>
        ///     Ensures that parameters in an action dialog contain default values specified in the Model.
        ///     Should be called in a Controller method that sets up a view that will contain an action dialog.
        /// </summary>
        /// <example>
        ///     SetUpDefaultParameters(customer, "CreateNewAddress")
        /// </example>
        protected void SetUpDefaultParameters(object domainObject, string actionName) {
            var nakedObject = GetNakedObject(domainObject);
            var findOrder = nakedObject.Specification.GetActionLeafNodes().Single(x => x.Id == actionName);
            SetDefaults(nakedObject, findOrder);
        }

        #endregion

        #region Saving objects

        /// <summary>
        ///     Apply changes from form and attempt to save. Go to indicated View based on result of save
        /// </summary>
        protected ActionResult SaveObject(FormCollection form, string id, string viewNameForFailure, string viewNameForSuccess, object modelForSuccessViewIfDifferent = null) {
            object obj = GetObjectFromId<object>(id); //Assuming id is for a transient, this will re-create a transient of same type

            if (SaveObject(form, ref obj)) {
                return View(viewNameForSuccess, modelForSuccessViewIfDifferent ?? obj);
            }

            return View(viewNameForFailure, obj);
        }

        /// <summary>
        ///     Creates and populates the values in a transient object from a form
        /// </summary>
        /// <param name="form">Form to populate from</param>
        protected T RecreateTransient<T>(FormCollection form) where T : new() {
            var obj = Container.NewTransientInstance<T>();
            var naked = GetNakedObject(obj);
            RefreshTransient(naked, form);
            return obj;
        }

        /// <summary>
        ///     Apply changes from form and attempt to save
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="form"></param>
        /// <param name="obj"></param>
        /// <returns>true if changes in form are valid and object saved</returns>
        protected bool SaveObject<T>(FormCollection form, ref T obj) {
            var naked = GetNakedObject(obj);
            
            var oid = Surface.OidStrategy.GetOid(naked);
            var ac = Convert(form);

            var result = Surface.PutObject(oid, ac);

            //if (ValidateChanges(naked, new ObjectAndControlData() {Form = form})) {
            //    if (ApplyChanges(naked, new ObjectAndControlData() {Form = form})) {
            //        result = true;
            //    }
            //}

            obj = (T)naked.Object;
            return string.IsNullOrEmpty(result.Reason);
        }

        #endregion

        #region Static: Getting objects

        /// <summary>
        ///     If id is null or empty use creator function to create and return a new object otherwise
        ///     returns the domain object that has the specified objectId (typically extracted from the URL).
        /// </summary>
        protected T GetOrCreateFromId<T>(string id, Func<T> creator) {
            if (string.IsNullOrEmpty(id)) {
                return creator();
            }

            return GetObjectFromId<T>(id);
        }

        /// <summary>
        ///     Returns the domain object that has the specified objectId (typically extracted from the URL)
        /// </summary>
        protected T GetObjectFromId<T>(string objectId) {
            return (T) GetNakedObjectFromId(objectId).Object;
        }

        /// <summary>
        ///     Obtains the Id for the specified object
        /// </summary>
        protected string GetIdFromObject(object domainObject) {
            return Surface.OidStrategy.GetOid(domainObject).ToString();
        }

        #endregion

        #region private

        private ViewResult InvokeAction(object domainObject, LambdaExpression expression, FormCollection parameters, string viewNameForFailure, string viewNameForSuccess) {
            var nakedObject = GetNakedObject(domainObject);
            MethodInfo methodInfo = GetAction(expression);
            return InvokeAction(nakedObject, methodInfo.Name, parameters, viewNameForFailure, viewNameForSuccess);
        }

        private ViewResult InvokeAction(string objectId, LambdaExpression expression, FormCollection parameters, string viewNameForFailure, string viewNameForSuccess) {
            var nakedObject = GetNakedObjectFromId(objectId);
            MethodInfo methodInfo = GetAction(expression);
            return InvokeAction(nakedObject, methodInfo.Name, parameters, viewNameForFailure, viewNameForSuccess);
        }

        private T InvokeAction<T>(INakedObjectSurface nakedObject, LambdaExpression expression, FormCollection parameters, out bool valid) {
            MethodInfo methodInfo = GetAction(expression);
            var nakedObjectAction = nakedObject.Specification.GetActionLeafNodes().Single(a => a.Id == methodInfo.Name);
            return InvokeAction<T>(nakedObject, nakedObjectAction, parameters, out valid);
        }

        //private T ExecuteAction(ObjectAndControlData controlData, INakedObjectSurface nakedObject, INakedObjectActionSurface action) {
        //    if (ActionExecutingAsContributed(action, nakedObject) && action.ParameterCount == 1) {
        //        // contributed action being invoked with a single parm that is the current target
        //        //// no dialog - go straight through 
        //        //var newForm = new FormCollection { { IdHelper.GetParameterInputId(ScaffoldAction.Wrap(action), ScaffoldParm.Wrap(action.Parameters.First())), NakedObjectsContext.GetObjectId(nakedObject) } };

        //        //// horrid kludge 
        //        //var oldForm = controlData.Form;
        //        //controlData.Form = newForm;

        //        //if (ValidateParameters(nakedObject, action, controlData)) {
        //        var ac = new ArgumentsContext() { Values = new Dictionary<string, object>(), ValidateOnly = false };
        //        var oid = Surface.OidStrategy.GetOid(nakedObject);
        //        var result = Surface.ExecuteObjectAction(oid, action.Id, ac);
        //        return AppropriateView(controlData, GetResult(result), action);
        //        //}

        //        //controlData.Form = oldForm;
        //        //AddAttemptedValues(controlData);
        //    }

        //    if (!action.Parameters.Any()) {
        //        var ac = new ArgumentsContext() { Values = new Dictionary<string, object>(), ValidateOnly = false };
        //        var oid = Surface.OidStrategy.GetOid(nakedObject);
        //        var result = Surface.ExecuteObjectAction(oid, action.Id, ac);

        //        return AppropriateView(controlData, GetResult(result), action);
        //    }

        //    SetDefaults(nakedObject, action);
        //    // do after any parameters set by contributed action so this takes priority
        //    SetSelectedParameters(action);
        //    SetPagingValues(controlData, nakedObject);
        //    var property = DisplaySingleProperty(controlData, controlData.DataDict);

        //    // TODO temp hack
        //    IActionSpec oldAction = ((dynamic)action).WrappedSpec;
        //    return View(property == null ? "ActionDialog" : "PropertyEdit", new FindViewModel { ContextObject = nakedObject.Object, ContextAction = oldAction, PropertyName = property });
        //}

        private T GetResult<T>(ActionResultContextSurface contextSurface) {
            if (contextSurface.HasResult) {
                return (T) contextSurface.Result.Target.Object;
            }
            return default(T);
        }

        private T InvokeAction<T>(INakedObjectSurface nakedObject, INakedObjectActionSurface action, FormCollection parameters, out bool valid) {
            ArgumentsContext ac;
            ILinkObjectId oid = Surface.OidStrategy.GetOid(nakedObject);
            ActionResultContextSurface contextSurface;

            if (ActionExecutingAsContributed(action, nakedObject) && action.ParameterCount == 1) {
                // contributed action being invoked with a single parm that is the current target
                // no dialog - go straight through 
                ac = new ArgumentsContext() {Values = new Dictionary<string, object>(), ValidateOnly = false};
                contextSurface = Surface.ExecuteObjectAction(oid, action.Id, ac);
            }
            else {
                ac = GetParameterValues(action, new ObjectAndControlData {Form = parameters});
                contextSurface = Surface.ExecuteObjectAction(oid, action.Id, ac);
            }

            valid = contextSurface.HasResult;

            if (valid) {
                return GetResult<T>(contextSurface);
            }

            return default(T);
        }

        //private T InvokeAction<T>(INakedObjectAdapter nakedObject, IActionSpec action, FormCollection parameters, out bool valid) {
        //    if (ActionExecutingAsContributed(action, nakedObject)) {
        //        if (action.ParameterCount == 1) {
        //            // contributed action being invoked with a single parm that is the current target
        //            // no dialog - go straight through 
        //            INakedObjectAdapter result = action.Execute(nakedObject, new[] {nakedObject});
        //            valid = true;
        //            return result.GetDomainObject<T>();
        //        }
        //        if (action.ParameterCount > 1) {
        //            // contributed action being invoked with multiple parms - populate first that match the target 
        //            IActionParameterSpec parmToPopulate = action.Parameters.FirstOrDefault(p => nakedObject.Spec.IsOfType(p.Spec));
        //            if (parmToPopulate != null) {
        //                ViewData[IdHelper.GetParameterInputId(ScaffoldAction.Wrap(action), ScaffoldParm.Wrap(parmToPopulate))] = NakedObjectsContext.GetObjectId(nakedObject.Object);
        //            }
        //        }
        //    }

        //    if (ValidateParameters(nakedObject, action, new ObjectAndControlData {Form = parameters})) {
        //        IEnumerable<INakedObjectAdapter> parms = GetParameterValues(action, new ObjectAndControlData {Form = parameters});
        //        INakedObjectAdapter result = action.Execute(nakedObject, parms.ToArray());
        //        valid = true;
        //        return result.GetDomainObject<T>();
        //    }

        //    valid = false;
        //    return default(T);
        //}

        private ViewResult InvokeAction(INakedObjectSurface nakedObject, string actionName, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            bool valid;
            var result = InvokeAction<object>(nakedObject.Object, actionName, parameters, out valid);
            return View(valid ? viewNameForSuccess : viewNameForFailure, result ?? nakedObject.Object);
        }

        private ViewResult InvokeAction(INakedObjectAdapter nakedObject, string actionName, FormCollection parameters, String viewNameForFailure, string viewNameForSuccess = null) {
            bool valid;
            var result = InvokeAction<object>(nakedObject.Object, actionName, parameters, out valid);
            return View(valid ? viewNameForSuccess : viewNameForFailure, result ?? nakedObject.GetDomainObject());
        }

        private static MethodInfo GetAction(LambdaExpression expression) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }

            if (expression.Body.NodeType != ExpressionType.Convert) {
                throw new ArgumentException("must be method");
            }
            Expression actionExpr = ((MethodCallExpression) (((UnaryExpression) expression.Body).Operand)).Object;
            return (MethodInfo) ((ConstantExpression) actionExpr).Value;
        }

        #endregion
    }
}