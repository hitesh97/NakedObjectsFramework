// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Linq;
using System.Reflection;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Actions.Contributed;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Util;

namespace NakedObjects.Reflector.DotNet.Facets.Actions.Executed {
    /// <summary>
    ///     Creates an <see cref="INotContributedActionFacet" /> based on the presence of an
    ///     <see cref="NotContributedActionAttribute" /> annotation
    /// </summary>
    public class ContributedActionAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        public ContributedActionAnnotationFacetFactory(INakedObjectReflector reflector)
            : base(reflector, NakedObjectFeatureType.ActionsOnly) {}

        private bool Process(MemberInfo member, IFacetHolder holder) {
            var attribute = AttributeUtils.GetCustomAttribute<NotContributedActionAttribute>(member);
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        public override bool Process(MethodInfo method, IMethodRemover methodRemover, IFacetHolder holder) {
            return Process(method, holder);
        }

        private INotContributedActionFacet Create(NotContributedActionAttribute attribute, IFacetHolder holder) {
            return attribute == null ? null : new NotContributedActionFacetImpl(holder, attribute.NotContributedToTypes.Select(t => Reflector.LoadSpecification(t)).ToArray());
        }
    }
}