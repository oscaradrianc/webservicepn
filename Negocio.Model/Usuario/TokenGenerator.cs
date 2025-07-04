using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Negocio.Model
{
    public static class TokenGenerator
    {

        public static string GenerateTokenJwt(Usuario user, IConfiguration configuration)
        {

            var symmetricSecurityKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes( configuration.GetSection("JWT").GetSection("SecretKey").Value)
                );
            var signingCredentials = new SigningCredentials(
                    symmetricSecurityKey, SecurityAlgorithms.HmacSha256
                );
            //var Header = new JwtHeader(signingCredentials);
            


            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                   new Claim(ClaimTypes.Name, user.Nombres),
                   new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = signingCredentials
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);


        }
        
    }
}
