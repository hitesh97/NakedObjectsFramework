﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using NakedObjects.Reflector.Spec;

namespace NakedObjects.Architecture.Facets.Actions.Contributed {
    public abstract class NotContributedActionFacetAbstract : FacetAbstract, INotContributedActionFacet {
        private readonly List<IIntrospectableSpecification> notContributedToTypes = new List<IIntrospectableSpecification>();

        protected NotContributedActionFacetAbstract(IFacetHolder holder, IIntrospectableSpecification[] notContributedToTypes)
            : base(Type, holder) {
            this.notContributedToTypes.AddRange(notContributedToTypes);
        }

        public static Type Type {
            get { return typeof (INotContributedActionFacet); }
        }

        #region INotContributedActionFacet Members

        public bool NotContributedTo(IIntrospectableSpecification spec) {
            return NeverContributed() || notContributedToTypes.Any(spec.IsOfType);
        }

        public bool NeverContributed() {
            return !notContributedToTypes.Any();
        }

        #endregion
    }
}