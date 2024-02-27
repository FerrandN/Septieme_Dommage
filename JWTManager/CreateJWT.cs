using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace JWTManager
{
    public abstract class CreateJWT
    {
        public static string GenerateJWT(string body)
        {
            string validationSecret = "3494726eda86981a95a06b356c2137d2887e3e7feb1b3be07d55a4b946755831";
            string hashedRequest = HashRequestBody.GetHashedRequest(body);


            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.UTF8.GetBytes(validationSecret);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("data", "{\"checksum\":\"" + hashedRequest + "\"}")
                }),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string jwt = tokenHandler.WriteToken(token);
            return jwt;
        }
    }
}
