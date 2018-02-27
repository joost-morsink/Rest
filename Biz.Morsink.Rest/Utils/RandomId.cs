using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// Helper class to provide cryptographically strong new random indentifiers
    /// </summary>
    public static class RandomId
    {
        /// <summary>
        /// Gets a cryptographicaly strong new random identifier.
        /// The result contains 6 bits of entropy per character.
        /// </summary>
        /// <param name="length">
        /// The desired length of the string.
        /// The default length is 24 for 144 bits of entropy.
        /// </param>
        /// <returns>A cryptographically strong new random identifier of the specified length.</returns>
        public static string Next(int length = 24)
        {
            using (var csp = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[((length - 1) / 4 + 1) * 3];
                csp.GetBytes(bytes);
                var chars = new char[length + 4];
                Convert.ToBase64CharArray(bytes, 0, bytes.Length, chars, 0);

                for (int i = 0; i < chars.Length; i++)
                {
                    switch (chars[i])
                    {
                        case '/':
                            chars[i] = '_';
                            break;
                        case '+':
                            chars[i] = '-';
                            break;
                    }
                }
                return new string(chars, 0, length);
            }
        }
    }
}
