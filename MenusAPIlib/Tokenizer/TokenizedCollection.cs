/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using MenusAPI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPIlib.Tokenizer
{
    public class TokenizedCollection<TValue> : Tokenizer, ITokenizedCollection<TValue>
                where TValue : class
    {

        protected ConcurrentDictionary<int, TValue> _TokenId2Object = new ConcurrentDictionary<int, TValue>();

        public TokenizedCollection(TimeSpan defaultExpiryTimespan, int seed = -1, int prime = 1000000009) : base(defaultExpiryTimespan, seed, prime)
        {
        }

        public TValue this[int key]
        {
            get
            {
                TValue obj = null;

                if (!this.TryGetValue(key, out obj))
                    return null;

                return obj;
            }
            set
            {
                if (this.ValidateToken(key, true) != true)
                    return; //Might consider throwing an Exception, although often times if a token (for which we have the key) expired, the desired behaviour is no action.

                AppToken tokenVal = base.GetToken(key);

                if (tokenVal == null)
                    return;

                _TokenId2Object[tokenVal.TokenId] = value;

                if (this.ValidateToken(key, true) != true)
                    Remove(key);
            }
        }

        public int Count
        {
            get
            {
                return _TokenId2Object.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public ICollection<int> Keys
        {
            get
            {
                return _Tokens.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _TokenId2Object.Values;
            }
        }

        public void Add(KeyValuePair<int, TValue> item)
        {
            throw new NotSupportedException("You cannot directy add a <tokenId, value> pair to a TokenizedCollection. Add just the value instead!");
        }

        public AppToken Add(TValue valToAdd)
        {
            var newToken = base.GetNewToken();
            _TokenId2Object[newToken.TokenId] = valToAdd;

            return newToken;
        }

        public void Add(int key, TValue value)
        {
            throw new NotSupportedException("You cannot directy add a <tokenId, value> pair to a TokenizedCollection. Add just the value instead!");
        }

        public void Clear()
        {
            lock(this)
            {
                _TokenId2Object.Clear();
                _Tokens.Clear();
            }
        }

        public bool Contains(KeyValuePair<int, TValue> item)
        {
            TValue obj = null;
            if (!this.TryGetValue(item.Key, out obj))
                return false;

            if (obj == item.Value)
                return true;

            return false;
        }

        public bool ContainsKey(int key)
        {
            if (this.ValidateToken(key, true) != true)
                return false;

            AppToken tokenVal = null;
            if (!_Tokens.TryGetValue(key, out tokenVal))
                return false;

            if (tokenVal == null)
                return false;

            return true;
        }

        public void CopyTo(KeyValuePair<int, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<int, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(int key)
        {
            this.InvalidateToken(key);
            TValue oldVal = null;
            return _TokenId2Object.TryRemove(key, out oldVal);
        }

        public bool TryGetValue(int key, out TValue value)
        {
            value = null;

            if (this.ValidateToken(key, true) != true)
                return false;

            AppToken tokenVal = base.GetToken(key);
            if (tokenVal == null)
                return false;

            TValue obj = _TokenId2Object[tokenVal.TokenId];
            value = obj;

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
