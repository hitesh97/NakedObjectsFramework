﻿// Copyright Naked Objects Group Ltd, 45 Station Road, Henley on Thames, UK, RG9 1AT
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System;
using NakedObjects.Architecture.Adapter;

namespace NakedObjects.Surface.Nof4.Implementation {
    public class MVCObjectId : ILinkObjectId {
        private readonly string id;
        public MVCObjectId(INakedObjectAdapter no) {}

        public MVCObjectId(string id) {
            this.id = id;
        }

        #region ILinkObjectId Members

        public string DomainType {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string InstanceId {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        public override string ToString() {
            return id;
        }
    }
}