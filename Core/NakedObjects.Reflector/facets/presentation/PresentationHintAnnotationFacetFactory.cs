// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Presentation;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Util;
using MemberInfo = System.Reflection.MemberInfo;
using MethodInfo = System.Reflection.MethodInfo;
using PropertyInfo = System.Reflection.PropertyInfo;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace NakedObjects.Reflector.DotNet.Facets.Presentation {
    public class PresentationHintAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        public PresentationHintAnnotationFacetFactory(IMetadata metadata)
            : base(metadata, NakedObjectFeatureType.Everything) { }

        public override bool Process(Type type, IMethodRemover methodRemover, IFacetHolder holder) {
            var attribute = type.GetCustomAttributeByReflection<PresentationHintAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        private static bool Process(MemberInfo member, IFacetHolder holder) {
            var attribute = member.GetCustomAttribute<PresentationHintAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        public override bool Process(MethodInfo method, IMethodRemover methodRemover, IFacetHolder holder) {
            return Process(method, holder);
        }

        public override bool Process(PropertyInfo property, IMethodRemover methodRemover, IFacetHolder holder) {
            return Process(property, holder);
        }

        public override bool ProcessParams(MethodInfo method, int paramNum, IFacetHolder holder) {
            ParameterInfo parameter = method.GetParameters()[paramNum];
            var attribute = parameter.GetCustomAttributeByReflection<PresentationHintAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        private static IPresentationHintFacet Create(PresentationHintAttribute attribute, IFacetHolder holder) {
            return attribute != null ? new PresentationHintFacetAnnotation(attribute.Value, holder) : null;
        }
    }
}