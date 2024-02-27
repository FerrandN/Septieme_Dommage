using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace JWTManager
{
    internal abstract class HashRequestBody
    {
        internal static string GetHashedRequest(string body)
        {
            using(SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(body));
                return BitConverter.ToString(hashedBytes).Replace("-","").ToLower();
            }
        }
    }
}
