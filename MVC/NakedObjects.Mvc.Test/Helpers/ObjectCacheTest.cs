﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Expenses.ExpenseClaims;
using Expenses.ExpenseClaims.Items;
using Expenses.Fixtures;
using Expenses.RecordedActions;
using Expenses.Services;
using Microsoft.Practices.Unity;
using MvcTestApp.Tests.Util;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Spec;
using NakedObjects.Core.Util;
using NakedObjects.Mvc.Test.Data;
using NakedObjects.Persistor.Entity.Configuration;
using NakedObjects.Surface.Nof4.Utility;
using NakedObjects.Surface.Utility;
using NakedObjects.Web.Mvc;
using NakedObjects.Web.Mvc.Html;
using NakedObjects.Xat;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace MvcTestApp.Tests.Helpers {
    [TestFixture]
    public class ObjectCacheTest : AcceptanceTestCase {
        private DummyController controller;
        private ContextMocks mocks;

        protected override string[] Namespaces {
            get { return Types.Select(t => t.Namespace).Distinct().ToArray(); }
        }

        protected override Type[] Types {
            get {
                var types1 = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "NakedObjects.Mvc.Test.Data").
                    GetTypes().Where(t => t.FullName.StartsWith("Expenses") && !t.FullName.Contains("Repository")).ToArray();

                var types2 = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "NakedObjects.Mvc.Test.Data").
                    GetTypes().Where(t => t.FullName.StartsWith("MvcTestApp.Tests.Helpers") && t.IsPublic).ToArray();

                return types1.Union(types2).ToArray();
            }
        }

        protected override object[] MenuServices {
            get { return (DemoServicesSet.ServicesSet()); }
        }

        protected override object[] ContributedActions {
            get { return (new object[] {new RecordedActionContributedActions()}); }
        }

        protected override object[] Fixtures {
            get { return (DemoFixtureSet.FixtureSet()); }
        }

        protected override void RegisterTypes(IUnityContainer container) {
            base.RegisterTypes(container);
            var config = new EntityObjectStoreConfiguration {EnforceProxies = false};
            config.UsingCodeFirstContext(() => new MvcTestContext("ObjectCacheTest"));
            container.RegisterInstance<IEntityObjectStoreConfiguration>(config, (new ContainerControlledLifetimeManager()));
        }

        [TestFixtureSetUp]
        public void SetupTestFixture() {
            Database.SetInitializer(new DatabaseInitializer());
        }

        [TestFixtureTearDown]
        public void TearDownTest() {
            Database.Delete("ObjectCacheTest");
        }

        private void SetupViewData() {
            mocks.ViewDataContainer.Object.ViewData[IdConstants.NofServices] = NakedObjectsFramework.GetServices();
            mocks.ViewDataContainer.Object.ViewData[IdConstants.NoFramework] = NakedObjectsFramework;
            mocks.ViewDataContainer.Object.ViewData["IdHelper"] = new IdHelper();
        }

        [Test]
        public void AddCollection() {
            var claimRepo = NakedObjectsFramework.GetServices().OfType<ClaimRepository>().FirstOrDefault();
            var claimRepoAdapter = NakedObjectsFramework.GetNakedObject(claimRepo);
            var claims = claimRepo.AllClaims();
            var claimsAdapter = NakedObjectsFramework.GetNakedObject(claims);
            var mockOid = CollectionMementoHelper.TestMemento(NakedObjectsFramework.LifecycleManager, NakedObjectsFramework.NakedObjectManager, NakedObjectsFramework.MetamodelManager, claimRepoAdapter, claimRepoAdapter.GetActionLeafNode("AllClaims"), new INakedObjectAdapter[] {});

            claimsAdapter.SetATransientOid(mockOid);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claimsAdapter);
            Assert.IsTrue(((IEnumerable<object>) mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).First()).SequenceEqual(claims));
        }

        [Test]
        public void AddNakedObjectToCache() {
            INakedObjectAdapter claim = NakedObjectsFramework.GetNakedObject(NakedObjectsFramework.Persistor.Instances<Claim>().First());
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));
        }

        [Test]
        public void AddToCache() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim));
        }

        [Test]
        public void AddTransient() {
            var claim = NakedObjectsFramework.LifecycleManager.CreateInstance((IObjectSpec) NakedObjectsFramework.MetamodelManager.GetSpecification(typeof (Claim))).GetDomainObject<Claim>();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);

            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim));
            Assert.IsTrue(!mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Any());
        }

        [Test]
        public void AllCachedObjects() {
            Claim claim1 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderBy(c => c.Id).First();
            Claim claim2 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderByDescending(c => c.Id).First();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim1);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim2);

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim1));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim2));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Count() == 2);
        }

        [Test]
        public void CacheLimit() {
            var claims = NakedObjectsFramework.Persistor.Instances<Claim>().Where(c => c.Claimant.UserName == "dick");

            claims.ForEach(c => mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, c));

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Count() == ObjectCache.CacheSize);
        }

        [Test]
        public void CachedObjectsOfBaseType() {
            GeneralExpense item1 = NakedObjectsFramework.Persistor.Instances<GeneralExpense>().OrderBy(c => c.Id).First();
            GeneralExpense item2 = NakedObjectsFramework.Persistor.Instances<GeneralExpense>().OrderByDescending(c => c.Id).First();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, item1);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, item2);

            IObjectSpec spec = (IObjectSpec) NakedObjectsFramework.MetamodelManager.GetSpecification(typeof (AbstractExpenseItem));

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Contains(item1));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Contains(item2));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Count() == 2);
        }

        [Test]
        public void CachedObjectsOfDifferentType() {
            GeneralExpense item1 = NakedObjectsFramework.Persistor.Instances<GeneralExpense>().OrderBy(c => c.Id).First();
            GeneralExpense item2 = NakedObjectsFramework.Persistor.Instances<GeneralExpense>().OrderByDescending(c => c.Id).First();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, item1);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, item2);

            IObjectSpec spec = (IObjectSpec) NakedObjectsFramework.MetamodelManager.GetSpecification(typeof (Claim));

            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Contains(item1));
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Contains(item2));
            Assert.IsTrue(!mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Any());
        }

        [Test]
        public void CachedObjectsOfType() {
            Claim claim1 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderBy(c => c.Id).First();
            Claim claim2 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderByDescending(c => c.Id).First();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim1);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim2);

            IObjectSpec spec = (IObjectSpec) NakedObjectsFramework.MetamodelManager.GetSpecification(typeof (Claim));

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Contains(claim1));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Contains(claim2));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.CachedObjectsOfType(NakedObjectsFramework, spec).Count() == 2);
        }

        [Test]
        public void ClearCache() {
            INakedObjectAdapter claim = NakedObjectsFramework.GetNakedObject(NakedObjectsFramework.Persistor.Instances<Claim>().First());
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));

            mocks.HtmlHelper.ViewContext.HttpContext.Session.ClearCachedObjects();
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));
        }

        [Test]
        public void ClearDisposedFromCache() {
            var claim = NakedObjectsFramework.GetNakedObject(NakedObjectsFramework.Persistor.Instances<Claim>().OrderByDescending(c => c.Id).First());

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));

            NakedObjectsFramework.TransactionManager.StartTransaction();
            NakedObjectsFramework.Persistor.DestroyObject(claim);
            NakedObjectsFramework.TransactionManager.EndTransaction();

            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));
        }

        [Test]
        public void DoNotAddDuplicates() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Count() == 1);
        }

        [Test]
        public void PurgesOldest() {
            var claims = NakedObjectsFramework.Persistor.Instances<Claim>().Where(c => c.Claimant.UserName == "dick").Take(ObjectCache.CacheSize + 1);

            Claim claim1 = null;
            Claim claim2 = null;
            foreach (var c in claims.OrderBy(c => c.Id)) {
                claim1 = claim1 ?? c;
                mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, c);
                claim2 = c;
            }
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Count() == ObjectCache.CacheSize);
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim1));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim2));
        }

        [Test]
        public void RemoveFromCache() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim));

            mocks.HtmlHelper.ViewContext.HttpContext.Session.RemoveFromCache(NakedObjectsFramework, claim);
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim));
        }

        [Test]
        public void RemoveFromCacheNotThere() {
            Claim claim = NakedObjectsFramework.Persistor.Instances<Claim>().First();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.RemoveFromCache(NakedObjectsFramework, claim);
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim));
            Assert.IsTrue(!mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Any());
        }

        [Test]
        public void RemoveNakedObjectFromCache() {
            INakedObjectAdapter claim = NakedObjectsFramework.GetNakedObject(NakedObjectsFramework.Persistor.Instances<Claim>().First());
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim);
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));

            mocks.HtmlHelper.ViewContext.HttpContext.Session.RemoveFromCache(NakedObjectsFramework, claim);
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));
        }

        [Test]
        public void RemoveNakedObjectFromCacheNotThere() {
            INakedObjectAdapter claim = NakedObjectsFramework.GetNakedObject(NakedObjectsFramework.Persistor.Instances<Claim>().First());

            mocks.HtmlHelper.ViewContext.HttpContext.Session.RemoveFromCache(NakedObjectsFramework, claim);
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim.Object));
            Assert.IsTrue(!mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Any());
        }

        [Test]
        public void RemoveOthersFromCache() {
            Claim claim1 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderBy(c => c.Id).First();
            Claim claim2 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderByDescending(c => c.Id).First();

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim1);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim2);

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim1));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim2));

            mocks.HtmlHelper.ViewContext.HttpContext.Session.RemoveOthersFromCache(NakedObjectsFramework, claim1);

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim1));
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim2));
        }

        [Test]
        public void SeperateCaches() {
            Claim c1 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderBy(c => c.Id).First();
            Claim c2 = NakedObjectsFramework.Persistor.Instances<Claim>().OrderByDescending(c => c.Id).First();

            INakedObjectAdapter claim1 = NakedObjectsFramework.GetNakedObject(c1);
            INakedObjectAdapter claim2 = NakedObjectsFramework.GetNakedObject(c2);

            Assert.AreNotSame(claim1, claim2);

            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim1);
            mocks.HtmlHelper.ViewContext.HttpContext.Session.AddToCache(NakedObjectsFramework, claim2, ObjectCache.ObjectFlag.BreadCrumb);

            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim1.Object));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework, ObjectCache.ObjectFlag.BreadCrumb).Contains(claim2.Object));

            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim2.Object));
            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework, ObjectCache.ObjectFlag.BreadCrumb).Contains(claim1.Object));

            mocks.HtmlHelper.ViewContext.HttpContext.Session.ClearCachedObjects();

            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework).Contains(claim1.Object));
            Assert.IsTrue(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework, ObjectCache.ObjectFlag.BreadCrumb).Contains(claim2.Object));

            mocks.HtmlHelper.ViewContext.HttpContext.Session.ClearCachedObjects(ObjectCache.ObjectFlag.BreadCrumb);

            Assert.IsFalse(mocks.HtmlHelper.ViewContext.HttpContext.Session.AllCachedObjects(NakedObjectsFramework, ObjectCache.ObjectFlag.BreadCrumb).Contains(claim2.Object));
        }

        #region Nested type: DummyController

        private class DummyController : Controller {}

        #endregion

        #region Setup/Teardown

        private static bool runFixtures;

        private void RunFixturesOnce() {
            if (!runFixtures) {
                RunFixtures();
                runFixtures = true;
            }
        }

        [SetUp]
        public void SetupTest() {
            InitializeNakedObjectsFramework(this);
            RunFixturesOnce();
            StartTest();
            controller = new DummyController();
            mocks = new ContextMocks(controller);
            SetUser("sven");
            SetupViewData();
        }

        #endregion
    }
}