/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public abstract class BusinessObject
    {
        /// <summary>
        /// A value which is conserved through calls to the API.
        /// Used to correlate newly created returned business objects with the ones in the original request.
        /// </summary>
        [DataMember]
        public int? CorrelationId { get; set; }

        /// <summary>
        /// Gets or Sets an Id which is used by the Business Layer to identify an object.
        /// </summary>
        [DataMember]
        public int? ObjectID { get; set; }

        [DataMember]
        public bool IsNew { get; set; }

        [DataMember]
        public bool IsDeleted { get; set; }
        
        public BusinessObject()
        {
        }
    }
}
