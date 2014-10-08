// Copyright � Naked Objects Group Ltd ( http://www.nakedobjects.net). 
// All Rights Reserved. This code released under the terms of the 
// Microsoft Public License (MS-PL) ( http://opensource.org/licenses/ms-pl.html) 

using System.Collections.Generic;
using NakedObjects.Architecture.Facets;
using NakedObjects.Reflector.Peer;

namespace NakedObjects.Reflector.DotNet.Facets.Ordering.MemberOrder {
    /// <summary>
    ///     Compares <see cref="INakedObjectMemberPeer" /> by <see cref="IFacetHolder.Identifier" />
    /// </summary>
    public class MemberIdentifierComparator<T> : IComparer<T> where T : IOrderableElement<T>, IFacetHolder  {
        #region IComparer<INakedObjectMemberPeer> Members

        public int Compare(T o1, T o2) {
            return o1.Identifier.CompareTo(o2.Identifier);
        }

        #endregion
    }
}