/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class LogEntryBO: BusinessObject, IComparable<LogEntryBO>
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Details { get; set; }

        [DataMember]
        public string CredentialsSummary { get; set; }

        [DataMember]
        public DateTime Moment { get; set; }


        public LogEntryBO()
        {
            Moment = DateTime.Now;
        }

        public int CompareTo(LogEntryBO other)
        {
            if (this.Moment == other.Moment)
                return 0;

            if (this.Moment < other.Moment)
                return -1;

            return 1;
        }
    }
}
