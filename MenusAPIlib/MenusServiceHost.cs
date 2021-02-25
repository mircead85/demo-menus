/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using MenusAPI;

namespace MenusAPIlib
{
    public class MenusServiceHost : Service
    {
        ServiceHost serviceHost;

        /// <summary>
        /// Remember to C:\Windows\system32>netsh http add urlacl url=http://+:8123/MenusAPIlib/Service/ user=COMPUTER-NAME\Username as admin.
        /// </summary>
        const string defaultBaseUri = "http://localhost:8123/MenusAPIlib/Service/";

        public MenusServiceHost(Uri baseAddressUri = null)
        {
            if(baseAddressUri == null)
                baseAddressUri = new Uri(defaultBaseUri);

            serviceHost = new ServiceHost(typeof(MenusService), baseAddressUri);
            serviceHost.AddServiceEndpoint(typeof(IMenusService).ToString(), new BasicHttpBinding(), "");
        }

        public override bool Start()
        {
            serviceHost.Open();
            return base.Start();
        }

        public override void Stop()
        {
            if(serviceHost.State == CommunicationState.Opened || serviceHost.State == CommunicationState.Opening)
                serviceHost.Close();
            base.Stop();
        }


    }
}
