// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Adapter;
using NakedObjects.Architecture.Spec;

namespace NakedObjects.Meta.Facet {
    [Serializable]
    public sealed class PropertyDefaultFacetAnnotation : PropertyDefaultFacetAbstract {
        private readonly object value;

        public PropertyDefaultFacetAnnotation(object value, ISpecification holder)
            : base(holder) {
            this.value = value;
        }

        public override bool CanAlwaysReplace {
            get { return false; }
        }

        public override object GetDefault(INakedObjectAdapter inObjectAdapter) {
            return value;
        }

        protected override string ToStringValues() {
            return "Value=" + value;
        }
    }

    // Copyright (c) Naked Objects Group Ltd.
}