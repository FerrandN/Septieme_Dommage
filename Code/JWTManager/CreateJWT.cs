using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace JWTManager
{
    public class CreateJWT
    {
        string encodedJwt;
        public CreateJWT(string requestBody) 
        {
            StringBuilder sb = new StringBuilder();

            //hash 256
            byte[] result = { };
            byte[] byteRequestBody = { };
            using (HMACSHA256 hash = new HMACSHA256())
            {
                Encoding enc = Encoding.UTF8;

                result = hash.ComputeHash(enc.GetBytes("3494726eda86981a95a06b356c2137d2887e3e7feb1b3be07d55a4b946755831"));

                byteRequestBody = hash.ComputeHash(enc.GetBytes(requestBody));
            }

            foreach (byte b in byteRequestBody)
                sb.Append(b.ToString("x2"));

            string test = "{\"checksum\":\"" + sb + "\"}";

            var claims = new[] {
                new Claim("data",test)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(result);
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds
                );
            JwtSecurityTokenHandler JSTH = new JwtSecurityTokenHandler();
            try
            {
                encodedJwt = JSTH.WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public string GetEncodedJwt() { return encodedJwt;}
    }
}
