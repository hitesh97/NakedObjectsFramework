﻿// Copyright © Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Facets.Properties.Eagerly;
using NakedObjects.Architecture.Reflect;
using NakedObjects.Util;
using MethodInfo = System.Reflection.MethodInfo;
using PropertyInfo = System.Reflection.PropertyInfo;

namespace NakedObjects.Reflector.DotNet.Facets.Properties.Eagerly {
    public class EagerlyAnnotationFacetFactory : FacetFactoryAbstract {
        public EagerlyAnnotationFacetFactory(INakedObjectReflector reflector)
            : base(reflector, NakedObjectFeatureType.EverythingButParameters) {}

        public override bool Process(Type type, IMethodRemover methodRemover, IFacetHolder holder) {
            var attribute = type.GetCustomAttributeByReflection<EagerlyAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        public override bool Process(PropertyInfo property, IMethodRemover methodRemover, IFacetHolder holder) {
            var attribute = property.GetCustomAttribute<EagerlyAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        public override bool Process(MethodInfo method, IMethodRemover methodRemover, IFacetHolder holder) {
            var attribute = method.GetCustomAttribute<EagerlyAttribute>();
            return FacetUtils.AddFacet(Create(attribute, holder));
        }

        private static IEagerlyFacet Create(EagerlyAttribute attribute, IFacetHolder holder) {
            return attribute == null ? null : new EagerlyFacetAnnotation(EagerlyAttribute.Do.Rendering, holder);
        }
    }
}