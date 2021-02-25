/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using MenusAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPIlib.Tokenizer
{
    public interface ITokenizedCollection<TValue> : IDictionary<int, TValue>, ITokenizerProvider
                where TValue : class
    {
        AppToken Add(TValue valToAdd);
    }
}
