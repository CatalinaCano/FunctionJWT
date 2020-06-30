using FxJWT.Class.Serialization;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FxJWT.Class.JWT
{
    class TokenService
    {

        private readonly string SecretKeyVariable = "SECRET_KEY";
        private const string Issuer = "Computec";

        private string SecretKey { get; set; }

        public TokenService()
        {
            try
            {
                SecretKey = Settings.GetVariable(SecretKeyVariable);
            }
            catch (NullReferenceException nre)
            {
                throw nre;
            }
        }

        public string GenerateToken(User user)
        {           
            //Se crea el Header
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(signingCredentials);

            //Se calcula la fecha de expedicion y expiracion del token
            var now = DateTime.Now;
            var expires = now.AddMinutes(user.MinutesAlive);

            //Se crean los Claims
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("client_id", user.IdClient.ToString())
            };

            //Se crea el Payload
            var payload = new JwtPayload(Issuer, null, claims, now, expires, now);

            //Se genera el token
            var token = new JwtSecurityToken(header, payload);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public IEnumerable<Claim> ValidateToken(string token)
        {
            try
            {
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));

                SecurityToken securityToken;
                var tokenHandler = new JwtSecurityTokenHandler();
                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Issuer,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    LifetimeValidator = this.LifetimeValidator,
                    IssuerSigningKey = symmetricSecurityKey,
                };

                return tokenHandler.ValidateToken(token, validationParameters, out securityToken).Claims;
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException();
            }            
        }

        private bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            return (expires != null && DateTime.Now < expires.Value.ToLocalTime());
        }

    }
}
