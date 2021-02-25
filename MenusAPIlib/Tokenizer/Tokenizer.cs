/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenusAPI;

namespace MenusAPIlib.Tokenizer
{
    /// <summary>
    /// A class which provides and manages up to 100,000,000 tokens with default prime. Tokens cannot be reused. 
    /// </summary>
    public class Tokenizer : ITokenizerProvider
    {
        protected Random _prng;

        protected int _prime;
        protected int _a0;
        protected int _current_a;

        protected object _lockCurrenta = new object();

        protected ConcurrentDictionary<int, AppToken> _Tokens = new ConcurrentDictionary<int, AppToken>();

        public Tokenizer (TimeSpan defaultExpiryTimespan, int seed=-1, int prime=1000000009)
        {
            if (seed == -1)
                _prng = new Random();
            else
                _prng = new Random(seed);

            _prime = prime;

            _a0 = _prng.Next(1, prime - 1);
            _current_a = _a0;

            DefaultExpiryTimeSpan = defaultExpiryTimespan;
        }

        protected int getNexta()
        {
            lock (_lockCurrenta)
            {
                var skip = _prng.Next(1, 10);
                while (skip > 0)
                {
                    _current_a = (int)((_current_a * (long)_a0) % _prime);
                    if (_current_a == _a0)
                        throw new TooManyTokensGeneratedException();
                    skip--;
                }

                return _current_a;
            }
        }

        public TimeSpan DefaultExpiryTimeSpan { get; set; }
        
        public AppToken GetNewToken()
        {
            return GetNewToken(DefaultExpiryTimeSpan);
        }

        public virtual AppToken GetNewToken(DateTime expiryWhen)
        {
            var tokenId = getNexta();
            //if(_Tokens.ContainsKey(tokenId)) throw new Exception("Duplicate token in token collection?!! Are you sure you specified a prime number as prime?"); //Should never happen.

            _Tokens[tokenId] = new AppToken()
            {
                TokenId = tokenId,
                ExpiryMomentServer = expiryWhen
            };

            return _Tokens[tokenId];
        }

        public AppToken GetNewToken(TimeSpan expiryTimespan)
        {
            return GetNewToken(DateTime.Now + expiryTimespan);
        }

        public virtual bool InvalidateToken(int tokenId)
        {
            if (_Tokens.ContainsKey(tokenId))
            {
                _Tokens[tokenId] = null; //Note that key is not removed.
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public virtual bool? ValidateToken(int tokenId, bool bInvalidateExpiredToken)
        {
            AppToken token = null;
            if(_Tokens.TryGetValue(tokenId, out token))
            {
                if (token == null)
                    return false;

                if (token.ExpiryMomentServer > DateTime.Now)
                    return true;
                else if (bInvalidateExpiredToken)
                    _Tokens[tokenId] = null; //Note that if tokens' lifespan could be expanded (not current scenario), this might result in concurrency race bugs.
            }

            return null;
        }

        public virtual void CleanInvalidatedTokens()
        {
            var keysToDelete = _Tokens.Keys.Where(key => _Tokens[key] == null);
            AppToken nullToken = null;
            foreach (var keyToDelete in keysToDelete)
                _Tokens.TryRemove(keyToDelete, out nullToken);
        }

        public AppToken GetToken(int tokenId)
        {
            if (ValidateToken(tokenId, true) != true)
                return null;

            AppToken token = null;
            if (_Tokens.TryGetValue(tokenId, out token))
            {
                return token;
            }

            return null;
        }
    }
}
