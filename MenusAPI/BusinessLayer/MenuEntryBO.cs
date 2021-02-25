/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class MenuEntryBO : BusinessObject, IComparable<MenuEntryBO>
    {
        [DataMember]
        public int? EntryID { get; set; }
        
        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public DateTime Moment { get; set; }

        [DataMember]
        public int NumCalories { get; set; }

        /// <summary>
        /// If NULL no data retrieved or no user setting. If negative, RED, if positive or zero GREEN.
        /// </summary>
        [DataMember]
        public int? ColorCodingWithRegardToOwnerSettings { get; set; }

        [DataMember]
        public UserBO Owner { get; set; }
        
        public MenuEntryBO(int? entryID = null)
        {
            EntryID = entryID;
        }

        public int CompareTo(MenuEntryBO other)
        {
            return this.Moment.CompareTo(other.Moment);
        }
    }
}
