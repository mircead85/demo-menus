/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using MenusAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPIlib.Tokenizer
{
    public interface ITokenizerProvider
    {
        TimeSpan DefaultExpiryTimeSpan { get; set; }

        /// <summary>
        /// Gets a new Token with default Expiry.
        /// </summary>
        /// <returns></returns>
        AppToken GetNewToken();

        AppToken GetNewToken(TimeSpan expiryTimespan);

        AppToken GetNewToken(DateTime expiryWhen);
        
        /// <summary>
        /// Validates that the specified token is (still valid).
        /// </summary>
        /// <returns>true if the token is valid. false if the token has expired. null if the token does not exist.</returns>
        bool? ValidateToken(int tokenId, bool bInvalidateExpiredToken);

        /// <summary>
        /// Deletes the specified token from the managed collection.
        /// </summary>
        /// <param name="tokenToInvalidate"></param>
        /// <returns></returns>
        bool InvalidateToken(int tokenId);
        
        AppToken GetToken(int tokenId);

        void CleanInvalidatedTokens();
    }
}
