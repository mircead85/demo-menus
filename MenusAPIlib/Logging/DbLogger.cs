/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenusAPI;
using MenusAPIlib.Model;

namespace MenusAPIlib.Logging
{
    public class DbLogger : Service, ILoggingService
    {
        public override bool Start()
        {
            return base.Start();
        }

        public bool LogException(Exception Ex, UserCredentials requester = null)
        {
            try
            {
                using (var context = new MenusEntityModelContainer())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    var newLogEntry = context.LogEntries.Create();
                    newLogEntry.Message = Ex.Message;
                    newLogEntry.Details = Ex.ToString();
                    newLogEntry.CredentialsSummary = requester != null ? requester.ToString() : "";
                    newLogEntry.Moment = DateTime.Now;

                    context.LogEntries.Add(newLogEntry);
                    context.SaveChanges();
                }
            }
            catch(Exception)
            {
                return false;
            }

            return true;
        }

        public bool LogMessage(string msg, UserCredentials requester = null)
        {
            try
            {
                using (var context = new MenusEntityModelContainer())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    var newLogEntry = context.LogEntries.Create();
                    newLogEntry.Message = msg;
                    newLogEntry.Details = "";
                    newLogEntry.CredentialsSummary = requester != null ? requester.ToString() : "";
                    newLogEntry.Moment = DateTime.Now;
                    context.LogEntries.Add(newLogEntry);
                    context.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
