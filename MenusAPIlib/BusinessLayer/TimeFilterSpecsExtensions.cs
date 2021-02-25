/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenusAPI;

namespace MenusAPIlib
{
    public static class TimeFilterSpecsExtensions
    {
        public static Func<DateTime, bool> BuildFilter(this TimeFilterSpecs filterSpecs)
        {
            if (filterSpecs == null)
                return (DateTime m)=>true;

            return
                (DateTime moment) => (moment.Date >= filterSpecs.DateFrom.Date && moment.Date <= filterSpecs.DateTo.Date)
                && (moment.Hour * 60 + moment.Minute >= filterSpecs.MinuteInDayFrom && moment.Hour * 60 + moment.Minute <= filterSpecs.MinuteInDayTo);
        }
    }
}
