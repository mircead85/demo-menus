/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class PagingInfo
    {
        /// <summary>
        /// Specifies for server-inbound request the Page Number to retrieve. For outbound replies specifies which page was retrieved.
        /// </summary>
        /// <remarks>If negative value is used, then all content will be (was) retrieved.</remarks>
        [DataMember]
        public int PageNumber { get; set; }

        /// <summary>
        /// Specifies how many entries are retrieved per one page. Both for inbound and outbound.
        /// If null for inbound, the default value from the server's App.config is used.
        /// </summary>
        [DataMember]
        public int? ItemsPerPage { get; set; }

        /// <summary>
        /// Specifies for outbound request what was the total number items on all pages on the server.
        /// </summary>
        [DataMember]
        public int? TotalItems { get; set; }
    }
}
