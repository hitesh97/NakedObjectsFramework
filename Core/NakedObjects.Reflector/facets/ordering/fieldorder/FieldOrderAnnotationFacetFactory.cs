// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Ordering.MemberOrder;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Util;

namespace NakedObjects.Reflector.DotNet.Facets.Ordering.FieldOrder {
    public class FieldOrderAnnotationFacetFactory : AnnotationBasedFacetFactoryAbstract {
        public FieldOrderAnnotationFacetFactory(INakedObjectReflector reflector)
            :base(reflector, NakedObjectFeatureType.ObjectsOnly) {}

        public override bool Process(Type type, IMethodRemover methodRemover, IFacetHolder holder) {
            var attribute = type.GetCustomAttributeByReflection<FieldOrderAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        private static IFieldOrderFacet Create(FieldOrderAttribute attribute, IFacetHolder holder) {
            return attribute == null ? null : new FieldOrderFacetAnnotation(attribute.Value, holder);
        }
    }
}