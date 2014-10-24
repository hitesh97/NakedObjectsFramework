// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Linq;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Component;
using NakedObjects.Architecture.Facet;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Metamodel.Facet {
    public abstract class CollectionFacetAbstract : FacetAbstract, ICollectionFacet {
        protected CollectionFacetAbstract(ISpecification holder)
            : base(typeof (ICollectionFacet), holder) {
            IsASet = false;
        }

        protected CollectionFacetAbstract(ISpecification holder, bool isASet)
            : this(holder) {
            IsASet = isASet;
        }

        protected object Call(string name, INakedObject collection, params object[] pp) {
            var m = GetType().GetMethod(name);
            var t = collection.Object.GetType().GenericTypeArguments.First();

            return m.MakeGenericMethod(t).Invoke(this, pp);
        }

        #region ICollectionFacet Members

        public abstract bool IsQueryable { get; }
        public abstract bool Contains(INakedObject collection, INakedObject element);
        public abstract INakedObject Page(int page, int size, INakedObject collection, INakedObjectManager manager, bool forceEnumerable);

        public bool IsASet { get; private set; }
        public abstract IEnumerable<INakedObject> AsEnumerable(INakedObject collection, INakedObjectManager manager);
        public abstract IQueryable AsQueryable(INakedObject collection);

        #endregion

        public abstract void Init(INakedObject collection, INakedObject[] initData);
    }


    // Copyright (c) Naked Objects Group Ltd.
}