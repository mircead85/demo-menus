/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    [KnownType(typeof(UserBO))]
    [KnownType(typeof(MenuEntryBO))]
    [KnownType(typeof(UserRoleBO))]
    public class CUDRequest:APIInboundRequest
    {
        public override UserRequestTypes TypeOfThisRequest
        {
            get
            {
                return UserRequestTypes.Other;
            }
        }

        [DataMember]
        public ICollection<BusinessObject> ObjectsToCUD { get; set; }

        public CUDRequest()
        {
            ObjectsToCUD = new List<BusinessObject>();
        }
    }
}
