using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using EstacionesServicio.Modelo;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace FacturacionelectronicaCore.Web.Authtentication
{
    public class JWTAuthentication : IAuthentication
    {

        private readonly AppSettings _appSettings;

        public JWTAuthentication(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string GenerateToken(Usuario usuario)
        {

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Secret));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                issuer: "http://localhost:5000",
                audience: "http://localhost:5000",
                claims: new List<Claim>()
                {
                        new Claim(ClaimTypes.NameIdentifier, usuario.guid.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Nombre),
                },
                expires: DateTime.Now.AddHours(5),
                signingCredentials: signinCredentials
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return token;
        }
    }
}
