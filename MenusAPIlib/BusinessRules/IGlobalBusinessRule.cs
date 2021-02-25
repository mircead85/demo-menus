/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenusAPIlib.Model;

namespace MenusAPIlib.BusinessRules
{
    public interface IGlobalBusinessRule
    {
        bool IsRespectedBy(MenusEntityModelContainer context, BusinessContext businessContext);
    }
}
