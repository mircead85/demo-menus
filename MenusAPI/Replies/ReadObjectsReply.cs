/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    [KnownType(typeof(BusinessObject))]
    [KnownType(typeof(UserBO))]
    [KnownType(typeof(MenuEntryBO))]
    [KnownType(typeof(UserRoleBO))]
    [KnownType(typeof(LogEntryBO))]
    public class ReadObjectsReply : APIReply
    {
        public override APIReplyTypes TypeOfThisReply
        {
            get
            {
                return APIReplyTypes.ReadObjects;
            }
        }

        [DataMember]
        public List<BusinessObject> ReadObjects { get; protected set; }
        
        [DataMember]
        public PagingInfo PagingInfo { get; set; }

        public ReadObjectsReply()
        {
            ReadObjects = new List<BusinessObject>();
        }
    }
}
