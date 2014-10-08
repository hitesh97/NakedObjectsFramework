// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Adapter.Value;
using NakedObjects.Architecture.Facets;
using NakedObjects.Architecture.Reflect;

namespace NakedObjects.Reflector.DotNet.Value {
    public class SbyteValueTypeFacetFactory : ValueUsingValueSemanticsProviderFacetFactory<sbyte> {
        public SbyteValueTypeFacetFactory(INakedObjectReflector reflector)
            :base(reflector, typeof (ISbyteValueFacet)) {}

        public override bool Process(Type type, IMethodRemover methodRemover, IFacetHolder holder) {
            if (SbyteValueSemanticsProvider.IsAdaptedType(type)) {
                var spec = Reflector.LoadSpecification(SbyteValueSemanticsProvider.AdaptedType);
                AddFacets(new SbyteValueSemanticsProvider(spec, holder));
                return true;
            }
            return false;
        }
    }
}