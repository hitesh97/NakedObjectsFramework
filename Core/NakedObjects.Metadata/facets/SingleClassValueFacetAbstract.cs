// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Spec;
using NakedObjects.Reflector.Spec;

namespace NakedObjects.Architecture.Facets {
    public abstract class SingleClassValueFacetAbstract : FacetAbstract, ISingleClassValueFacet {
        private readonly IIntrospectableSpecification valueSpec;
        private readonly Type valueType;

        protected SingleClassValueFacetAbstract(Type facetType, IFacetHolder holder, Type valueType, IIntrospectableSpecification valueSpec)
            : base(facetType, holder) {
            this.valueType = valueType;
            this.valueSpec = valueSpec;
        }

        #region ISingleClassValueFacet Members

        public virtual Type Value {
            get { return valueType; }
        }

        /// <summary>
        ///     The <see cref="INakedObjectSpecification" /> of the <see cref="Value" />
        /// </summary>
        public virtual IIntrospectableSpecification ValueSpec {
            get { return valueSpec; }
        }

        #endregion
    }
}