﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Resolve;
using NakedObjects.Surface;
using NakedObjects.Surface.Utility;
using NakedObjects.Web.Mvc.Html;

namespace NakedObjects.Web.Mvc {
    public static class ObjectCache {
        #region ObjectFlag enum

        public enum ObjectFlag {
            None = 0,
            BreadCrumb = 1
        }

        #endregion

        [Serializable]
        private class CacheMemento {
            public DateTime Added { get; set; }
            public string Url { get; set; }
            public string Spec { get; set; }
        }

        private const string NoneBucket = "ObjectCache";
        private const string BreadCrumbBucket = "BreadCrumbCache";

        public const int CacheSize = 100;
        private static readonly string[] Bucket = new[] {NoneBucket, BreadCrumbBucket};

        public static void AddToCache(this HttpSessionStateBase session, INakedObjectsFramework framework, object domainObject, string url, ObjectFlag flag = ObjectFlag.None) {
            INakedObjectAdapter nakedObject = framework.GetNakedObject(domainObject);
            session.AddToCache(framework, nakedObject, url, flag);
        }

        public static void AddOrUpdateInCache(this HttpSessionStateBase session, INakedObjectsSurface surface, object domainObject, string url, ObjectFlag flag = ObjectFlag.None) {
            var nakedObject = surface.GetObject(domainObject);
            session.AddOrUpdateInCache(surface, nakedObject, url, flag);
        }

        public static void AddOrUpdateInCache(this HttpSessionStateBase session, INakedObjectsFramework framework, object domainObject, string url, ObjectFlag flag = ObjectFlag.None) {
            INakedObjectAdapter nakedObject = framework.GetNakedObject(domainObject);
            session.AddOrUpdateInCache(framework, nakedObject, url, flag);
        }

        public static void AddToCache(this HttpSessionStateBase session, INakedObjectsFramework framework, object domainObject, ObjectFlag flag = ObjectFlag.None) {
            INakedObjectAdapter nakedObject = framework.GetNakedObject(domainObject);
            session.AddToCache(framework, nakedObject, flag);
        }

        public static void AddToCache(this HttpSessionStateBase session, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, ObjectFlag flag = ObjectFlag.None) {
            session.AddToCache(framework, nakedObject, null, flag);
        }

        public static void AddToCache(this HttpSessionStateBase session, INakedObjectsSurface surface, object domainObject, ObjectFlag flag = ObjectFlag.None) {
            var nakedObject = surface.GetObject(domainObject);
            session.AddToCache(surface, nakedObject, flag);
        }

        public static void AddToCache(this HttpSessionStateBase session, INakedObjectsSurface surface, INakedObjectSurface nakedObject, ObjectFlag flag = ObjectFlag.None) {
            session.AddToCache(surface, nakedObject, null, flag);
        }

        private static void ClearPreviousTransients(this HttpSessionStateBase session, INakedObjectAdapter nakedObject, ObjectFlag flag) {
            if (nakedObject.Oid.HasPrevious) {
                if (nakedObject.Oid.Previous.IsTransient) {
                    session.GetCache(flag).Remove(FrameworkHelper.GetObjectId(nakedObject.Oid.Previous));
                }
            }
        }

        //private static void ClearPreviousTransients(this HttpSessionStateBase session, INakedObjectSurface nakedObject, ObjectFlag flag) {
        //    if (nakedObject.Oid.HasPrevious) {
        //        if (nakedObject.Oid.Previous.IsTransient) {
        //            session.GetCache(flag).Remove(FrameworkHelper.GetObjectId(nakedObject.Oid.Previous));
        //        }
        //    }
        //}

        public static void AddToCache(this HttpSessionStateBase session, INakedObjectsSurface surface, INakedObjectSurface nakedObject, string url, ObjectFlag flag = ObjectFlag.None) {
            // only add transients if we are storing transients in the session 

            if (!nakedObject.IsTransient() || nakedObject.Specification.IsCollection()) {
                //session.ClearPreviousTransients(nakedObject, flag);
                session.GetCache(flag).AddToCache(surface, nakedObject, url, flag);
            }
        }

        public static void AddToCache(this HttpSessionStateBase session, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, string url, ObjectFlag flag = ObjectFlag.None) {
            // only add transients if we are storing transients in the session 

            if (!nakedObject.ResolveState.IsTransient() || nakedObject.Spec.IsCollection) {
                session.ClearPreviousTransients(nakedObject, flag);
                session.GetCache(flag).AddToCache(framework, nakedObject, url, flag);
            }
        }

        public static void AddOrUpdateInCache(this HttpSessionStateBase session, INakedObjectsSurface surface, INakedObjectSurface nakedObject, string url, ObjectFlag flag = ObjectFlag.None) {
            // only add transients if we are storing transients in the session 

            if (!nakedObject.IsTransient() || nakedObject.Specification.IsCollection()) {
                //session.ClearPreviousTransients(nakedObject, flag);
                session.GetCache(flag).AddOrUpdateInCache(surface, nakedObject, url, flag);
            }
        }

        public static void AddOrUpdateInCache(this HttpSessionStateBase session, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, string url, ObjectFlag flag = ObjectFlag.None) {
            // only add transients if we are storing transients in the session 

            if (!nakedObject.ResolveState.IsTransient() || nakedObject.Spec.IsCollection) {
                session.ClearPreviousTransients(nakedObject, flag);
                session.GetCache(flag).AddOrUpdateInCache(framework, nakedObject, url, flag);
            }
        }

        internal static void TestAddToCache(this HttpSessionStateBase session, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, ObjectFlag flag = ObjectFlag.None) {
            session.GetCache(flag).AddToCache(framework, nakedObject, null, flag);
        }

        public static void RemoveFromCache(this HttpSessionStateBase session, INakedObjectsFramework framework, object domainObject, ObjectFlag flag = ObjectFlag.None) {
            INakedObjectAdapter nakedObject = framework.GetNakedObject(domainObject);
            session.RemoveFromCache(framework, nakedObject, flag);
        }

        private static INakedObjectSurface GetNakedObject(INakedObjectsSurface surface, object domainObject) {
            var oid = surface.OidStrategy.GetOid(domainObject);
            return surface.GetObject(oid).Target;
        }

        private static INakedObjectSurface GetNakedObjectFromId(INakedObjectsSurface surface, string id) {
            var oid = surface.OidStrategy.GetOid(id, "");
            return surface.GetObject(oid).Target;
        }

        public static void RemoveOthersFromCache(this HttpSessionStateBase session, INakedObjectsSurface surface, object domainObject, ObjectFlag flag = ObjectFlag.None) {
            var nakedObject = GetNakedObject(surface, domainObject);
            session.RemoveOthersFromCache(surface, nakedObject, flag);
        }

        public static void RemoveOthersFromCache(this HttpSessionStateBase session, INakedObjectsFramework framework, object domainObject, ObjectFlag flag = ObjectFlag.None) {
            INakedObjectAdapter nakedObject = framework.GetNakedObject(domainObject);
            session.RemoveOthersFromCache(framework, nakedObject, flag);
        }

        public static void RemoveOthersFromCache(this HttpSessionStateBase session, string objectId, ObjectFlag flag = ObjectFlag.None) {
            session.GetCache(flag).RemoveOthersFromCache(objectId);
        }

        public static void RemoveFromCache(this HttpSessionStateBase session, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, ObjectFlag flag = ObjectFlag.None) {
            session.GetCache(flag).RemoveFromCache(framework, nakedObject);
        }

        public static void RemoveFromCache(this HttpSessionStateBase session, INakedObjectsSurface surface, INakedObjectSurface nakedObject, ObjectFlag flag = ObjectFlag.None) {
            session.GetCache(flag).RemoveFromCache(surface, nakedObject);
        }

        public static void RemoveFromCache(this HttpSessionStateBase session, string objectId, ObjectFlag flag = ObjectFlag.None) {
            session.GetCache(flag).RemoveFromCache(objectId);
        }

        public static void RemoveOthersFromCache(this HttpSessionStateBase session, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, ObjectFlag flag = ObjectFlag.None) {
            session.GetCache(flag).RemoveOthersFromCache(framework, nakedObject);
        }

        public static void RemoveOthersFromCache(this HttpSessionStateBase session, INakedObjectsSurface framework, INakedObjectSurface nakedObject, ObjectFlag flag = ObjectFlag.None) {
            session.GetCache(flag).RemoveOthersFromCache(framework, nakedObject);
        }

        public static object LastObject(this HttpSessionStateBase session, INakedObjectsSurface surface, ObjectFlag flag = ObjectFlag.None) {
            KeyValuePair<string, CacheMemento> lastEntry = session.GetCache(flag).OrderBy(kvp => kvp.Value.Added).LastOrDefault();

            if (lastEntry.Equals(default(KeyValuePair<string, CacheMemento>))) {
                return null;
            }

            var lastObject = SafeGetNakedObjectFromId(lastEntry.Key, surface);

            // todo will this work ? 
            //if (lastObject.ResolveState.IsDestroyed()) {
            if (lastObject == null) {
                session.GetCache(flag).Remove(lastEntry.Key);
                return session.LastObject(surface, flag);
            }

            return lastObject.Object;
        }



        public static object LastObject(this HttpSessionStateBase session, INakedObjectsFramework framework, ObjectFlag flag = ObjectFlag.None) {
            KeyValuePair<string, CacheMemento> lastEntry = session.GetCache(flag).OrderBy(kvp => kvp.Value.Added).LastOrDefault();

            if (lastEntry.Equals(default(KeyValuePair<string, CacheMemento>))) {
                return null;
            }

            INakedObjectAdapter lastObject = SafeGetNakedObjectFromId(lastEntry.Key, framework);

            if (lastObject.ResolveState.IsDestroyed()) {
                session.GetCache(flag).Remove(lastEntry.Key);
                return session.LastObject(framework, flag);
            }

            return lastObject.Object;
        }

        // This is dangerous - retrieves all cached objects from the database - use with care !
        internal static IEnumerable<object> AllCachedObjects(this HttpSessionStateBase session, INakedObjectsFramework framework, ObjectFlag flag = ObjectFlag.None) {
            return session.GetAndTidyCachedNakedObjects(framework, flag).Where(no => !no.ResolveState.IsDestroyed()).Select(no => no.Object);
        }

        public static IEnumerable<string> AllCachedUrls(this HttpSessionStateBase session, ObjectFlag flag = ObjectFlag.None) {
            return session.GetCache(flag).OrderBy(kvp => kvp.Value.Added).Where(kvp => kvp.Value.Url != null).Select(kvp => kvp.Value.Url);
        }

        // This is dangerous - retrieves all cached objects from the database - use with care !
        private static IEnumerable<INakedObjectAdapter> GetAndTidyCachedNakedObjects(this HttpSessionStateBase session, INakedObjectsFramework framework, ObjectFlag flag) {
            session.ClearDestroyedObjects(framework, flag);
            return session.GetCache(flag).OrderBy(kvp => kvp.Value.Added).Select(kvp => framework.GetNakedObjectFromId(kvp.Key));
        }

        private static bool SameSpec(string name, ITypeSpec otherSpec, INakedObjectsFramework framework) {
            var thisSpec = framework.MetamodelManager.GetSpecification(name);
            return thisSpec.IsOfType(otherSpec);
        }

        private static bool SameSpec(string name, INakedObjectSpecificationSurface otherSpec, INakedObjectsSurface surface) {
            var thisSpec = surface.GetDomainType(name);
            return thisSpec.IsOfType(otherSpec);
        }

        private static IEnumerable<INakedObjectAdapter> GetAndTidyCachedNakedObjectsOfType(this HttpSessionStateBase session, INakedObjectsFramework framework, ITypeSpec spec, ObjectFlag flag) {
            session.ClearDestroyedObjectsOfType(framework, spec, flag);
            return session.GetCache(flag).Where(cm => SameSpec(cm.Value.Spec, spec, framework)).OrderBy(kvp => kvp.Value.Added).Select(kvp => framework.GetNakedObjectFromId(kvp.Key));
        }

        private static IEnumerable<INakedObjectSurface> GetAndTidyCachedNakedObjectsOfType(this HttpSessionStateBase session, INakedObjectsSurface surface, INakedObjectSpecificationSurface spec, ObjectFlag flag) {
            session.ClearDestroyedObjectsOfType(surface, spec, flag);
            return session.GetCache(flag).Where(cm => SameSpec(cm.Value.Spec, spec, surface)).OrderBy(kvp => kvp.Value.Added).Select(kvp => GetNakedObjectFromId(surface, kvp.Key));
        }

        public static IEnumerable<object> CachedObjectsOfType(this HttpSessionStateBase session, INakedObjectsFramework framework, ITypeSpec spec, ObjectFlag flag = ObjectFlag.None) {
            return session.GetAndTidyCachedNakedObjectsOfType(framework, spec, flag).Select(no => no.Object);
        }

        public static IEnumerable<object> CachedObjectsOfType(this HttpSessionStateBase session, INakedObjectsSurface surface, INakedObjectSpecificationSurface spec, ObjectFlag flag = ObjectFlag.None) {
            return session.GetAndTidyCachedNakedObjectsOfType(surface, spec, flag).Select(no => no.Object);
        }

        // This is dangerous - retrieves all cached objects from the database - use with care !
        private static void ClearDestroyedObjects(this HttpSessionStateBase session, INakedObjectsFramework framework, ObjectFlag flag = ObjectFlag.None) {
            Dictionary<string, CacheMemento> cache = session.GetCache(flag);
            List<string> toRemove = cache.Select(kvp => new {kvp.Key, no = SafeGetNakedObjectFromId(kvp.Key, framework)}).Where(ao => ao.no.ResolveState.IsDestroyed()).Select(ao => ao.Key).ToList();
            toRemove.ForEach(k => cache.Remove(k));
        }

        public static void ClearDestroyedObjectsOfType(this HttpSessionStateBase session, INakedObjectsFramework framework, ITypeSpec spec, ObjectFlag flag = ObjectFlag.None) {
            Dictionary<string, CacheMemento> cache = session.GetCache(flag);
            List<string> toRemove = cache.Where(cm => SameSpec(cm.Value.Spec, spec, framework)).Select(kvp => new {kvp.Key, no = SafeGetNakedObjectFromId(kvp.Key, framework)}).Where(ao => ao.no.ResolveState.IsDestroyed()).Select(ao => ao.Key).ToList();
            toRemove.ForEach(k => cache.Remove(k));
        }

        public static void ClearDestroyedObjectsOfType(this HttpSessionStateBase session, INakedObjectsSurface surface, INakedObjectSpecificationSurface spec, ObjectFlag flag = ObjectFlag.None) {
            Dictionary<string, CacheMemento> cache = session.GetCache(flag);
            List<string> toRemove = cache.Where(cm => SameSpec(cm.Value.Spec, spec, surface)).Select(kvp => new { kvp.Key, no = SafeGetNakedObjectFromId(kvp.Key, surface) }).Where(ao => ao.no == null).Select(ao => ao.Key).ToList();
            toRemove.ForEach(k => cache.Remove(k));
        }

        public static void ClearCachedObjects(this HttpSessionStateBase session, ObjectFlag flag = ObjectFlag.None) {
            Dictionary<string, CacheMemento> cache = session.GetCache(flag);
            List<string> toRemove = cache.Select(kvp => kvp.Key).ToList();
            toRemove.ForEach(k => cache.Remove(k));
        }

        private static INakedObjectSurface SafeGetNakedObjectFromId(string id, INakedObjectsSurface surface) {
            try {
                var oid = surface.OidStrategy.GetOid(id, "");
                return surface.GetObject(oid).Target;
            }
            catch (Exception) {
                // todo work out this 
                //// create a NakedObject just to carry the 'Destroyed' state
                //var no = framework.GetNakedObject(new object());
                //no.ResolveState.Handle(Events.StartResolvingEvent);
                //no.ResolveState.Handle(Events.DestroyEvent);
                //return no;
                return null;
            }
        }


        private static INakedObjectAdapter SafeGetNakedObjectFromId(string id, INakedObjectsFramework framework) {
            try {
                return framework.GetNakedObjectFromId(id);
            }
            catch (Exception) {
                // create a NakedObject just to carry the 'Destroyed' state
                var no = framework.GetNakedObject(new object());
                no.ResolveState.Handle(Events.StartResolvingEvent);
                no.ResolveState.Handle(Events.DestroyEvent);
                return no;
            }
        }

        private static Dictionary<string, CacheMemento> GetCache(this HttpSessionStateBase session, ObjectFlag flag) {
            var objs = (Dictionary<string, CacheMemento>) session[Bucket[(int) flag]];
            if (objs == null) {
                objs = new Dictionary<string, CacheMemento>();
                session.Add(Bucket[(int) flag], objs);
            }
            return objs;
        }

        private static void AddOrUpdateInCache(this Dictionary<string, CacheMemento> cache, INakedObjectsSurface surface, INakedObjectSurface nakedObject, string url, ObjectFlag flag) {
            string objectId = surface.OidStrategy.GetObjectId(nakedObject);

            if (cache.ContainsKey(objectId)) {
                cache[objectId].Spec = nakedObject.Specification.FullName();
                cache[objectId].Url = url;
            }
            else {
                cache[objectId] = new CacheMemento { Added = DateTime.Now, Spec = nakedObject.Specification.FullName(), Url = url };
                while (cache.Count > CacheSize) {
                    RemoveOldest(cache, flag);
                }
            }
        }

        private static void AddOrUpdateInCache(this Dictionary<string, CacheMemento> cache, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, string url, ObjectFlag flag) {
            string objectId = framework.GetObjectId(nakedObject);

            if (cache.ContainsKey(objectId)) {
                cache[objectId].Spec = nakedObject.Spec.FullName;
                cache[objectId].Url = url;
            }
            else {
                cache[objectId] = new CacheMemento {Added = DateTime.Now, Spec = nakedObject.Spec.FullName, Url = url};
                while (cache.Count > CacheSize) {
                    RemoveOldest(cache, flag);
                }
            }
        }

        private static void AddToCache(this Dictionary<string, CacheMemento> cache, INakedObjectsSurface surface, INakedObjectSurface nakedObject, string url, ObjectFlag flag) {
            string objectId = surface.OidStrategy.GetObjectId(nakedObject);
            cache[objectId] = new CacheMemento { Added = DateTime.Now, Spec = nakedObject.Specification.FullName(), Url = url };
            while (cache.Count > CacheSize) {
                RemoveOldest(cache, flag);
            }
        }

        private static void AddToCache(this Dictionary<string, CacheMemento> cache, INakedObjectsFramework framework, INakedObjectAdapter nakedObject, string url, ObjectFlag flag) {
            string objectId = framework.GetObjectId(nakedObject);
            cache[objectId] = new CacheMemento {Added = DateTime.Now, Spec = nakedObject.Spec.FullName, Url = url};
            while (cache.Count > CacheSize) {
                RemoveOldest(cache, flag);
            }
        }

        private static void RemoveOldest(Dictionary<string, CacheMemento> cache, ObjectFlag flag) {
            DateTime oldestEntry = cache.Values.Select(t => t.Added).Min();
            string oldestId = cache.Where(kvp => kvp.Value.Added == oldestEntry).Select(kvp => kvp.Key).First();
            cache.Remove(oldestId);
        }

        private static void RemoveFromCache(this Dictionary<string, CacheMemento> cache, INakedObjectsFramework framework, INakedObjectAdapter nakedObject) {
            cache.RemoveFromCache(framework.GetObjectId(nakedObject));
        }

        private static void RemoveFromCache(this Dictionary<string, CacheMemento> cache, INakedObjectsSurface surface, INakedObjectSurface nakedObject) {
            cache.RemoveFromCache(surface.OidStrategy.GetOid(nakedObject).ToString());
        }


        private static void RemoveFromCache(this Dictionary<string, CacheMemento> cache, string objectId) {
            cache.Remove(objectId);
        }

        private static void RemoveOthersFromCache(this Dictionary<string, CacheMemento> cache, INakedObjectsFramework framework, INakedObjectAdapter nakedObject) {
            string id = framework.GetObjectId(nakedObject);
            cache.RemoveOthersFromCache(id);
        }

        private static void RemoveOthersFromCache(this Dictionary<string, CacheMemento> cache, INakedObjectsSurface framework, INakedObjectSurface nakedObject) {
            string id = framework.OidStrategy.GetOid(nakedObject).ToString();
            cache.RemoveOthersFromCache(id);
        }

        private static void RemoveOthersFromCache(this Dictionary<string, CacheMemento> cache, string id) {
            List<string> toRemove = cache.Where(kvp => kvp.Key != id).Select(kvp => kvp.Key).ToList();
            toRemove.ForEach(k => cache.Remove(k));
        }
    }
}