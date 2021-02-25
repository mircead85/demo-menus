/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPIlib.Tokenizer
{
    class TooManyTokensGeneratedException : Exception
    {
        public TooManyTokensGeneratedException() : base("Too many tokens generated!") { }
    }
}
