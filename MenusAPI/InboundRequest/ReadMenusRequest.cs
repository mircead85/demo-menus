/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class ReadMenusRequest : ReadObjectsRequest
    {
        public override UserRequestTypes TypeOfThisRequest
        {
            get
            {
                return UserRequestTypes.ReadMenus;
            }
        }

        /// <summary>
        /// User for which it is desired to obtain Menus. Leave null for all Menus.
        /// </summary>
        [DataMember]
        public UserBO OwnerOfMenuEntries { get; set; }

        /// <summary>
        /// Leave null for no filtering.
        /// </summary>
        [DataMember]
        public TimeFilterSpecs TimeFilterSpecs { get; set; }

        /// <summary>
        /// If set to true, will cause menu entries to have the ColorCodingWithRegardToOwnerSettings filled out. Otherwise this field wil be left null for all entries.
        /// </summary>
        [DataMember]
        public bool bFillOutCaloricColorCoding { get; set; }
    }
}
