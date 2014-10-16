// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System;
using NakedObjects.Architecture.Facets;
using NakedObjects.Reflector.Peer;
using NakedObjects.Reflector.Spec;

namespace NakedObjects.Reflector.DotNet.Reflect.Actions {
    public class DotNetNakedObjectActionParamPeer : Specification, INakedObjectActionParamPeer {
        private readonly IObjectSpecImmutable specification;

        public DotNetNakedObjectActionParamPeer(IObjectSpecImmutable specification) {
            this.specification = specification;
        }

        #region INakedObjectActionParamPeer Members

        public IObjectSpecImmutable Specification {
            get { return specification; }
        }

        public override IIdentifier Identifier {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}