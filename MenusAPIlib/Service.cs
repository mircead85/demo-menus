/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;

namespace MenusAPIlib
{
    public class Service : IService
    {
        protected bool _IsStarted = false;
        public virtual bool IsStarted
        {
            get
            {
                return _IsStarted;
            }
        }

        public virtual bool Start()
        {
            _IsStarted = true;

            return _IsStarted;
        }

        public virtual void Stop()
        {
            _IsStarted = false;
        }
    }
}
