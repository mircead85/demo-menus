/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    [KnownType(typeof(PagingInfo))]
    public class ReadObjectsRequest : APIInboundRequest
    {
        /// <summary>
        /// When set, only the object with the specified Id is retrieved.
        /// </summary>
        [DataMember]
        public int? SpecificObjectId { get; set; }

        /// <summary>
        /// Specifies pagination info so as not to retrieve too much data at once.
        /// </summary>
        [DataMember]
        public PagingInfo PageingInfo { get; set; }
    }
}
