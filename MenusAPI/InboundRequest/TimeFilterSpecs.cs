/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class TimeFilterSpecs
    {
        /// <summary>
        /// The condition is considered met if the number of date of the entry is no lesser than this value.
        /// </summary>
        [DataMember]
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// The condition is considered met if the number of date of the entry is no greater than this value.
        /// </summary>
        [DataMember]
        public DateTime DateTo { get; set; }

        /// <summary>
        /// The condition is considered met if the number of minutes elapsed in the day the entry was made is no lesser than this value.
        /// </summary>
        [DataMember]
        public int MinuteInDayFrom { get; set; }

        /// <summary>
        /// The condition is considered met if the number of minutes elapsed in the day the entry was made is no greater than this value.
        /// </summary>
        [DataMember]
        public int MinuteInDayTo { get; set; }        
    }
}
