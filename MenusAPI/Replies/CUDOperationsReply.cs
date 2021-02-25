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
    [KnownType(typeof(UserRoleBO))]
    [KnownType(typeof(MenuEntryBO))]
    [KnownType(typeof(LogEntryBO))]
    public class CUDOperationsReply:APIReply
    {
        [DataMember]
        public int NumberOfEntriesWhichGeneratedChanges { get; set; }

        [DataMember]
        public List<BusinessObject> NewlyCreatedObjectsWithIds { get; set; }
    }
}
