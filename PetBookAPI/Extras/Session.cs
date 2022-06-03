using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetBookAPI.Extras;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using PetBookAPI.Models;
using System.Security.Principal;

namespace PetBookAPI.Extras
{
    public enum SessionType
    {
        SESSION = 0,
        ACCESS = 1
    }


    public class Session
    {
        private Environment env = new Environment();
        private string SessionKey;
        private string AccessKey;
        private string Issuer;
        private string Audience;
        private string UserId;
        private string Username;

        public Session()
        {
            this.SessionKey = env.Key("JWT_SESSION_TOKEN");
            this.AccessKey = env.Key("JWT_ACCESS_TOKEN");
            this.Issuer = env.Key("JWT_ISSUER");
            this.Audience = env.Key("JWT_AUDIENCE");
        }

        public Session(string id, string username)
        {
            /**
             * Make sure you have "env.config.json" on your root folder before you call on json key
             **/
            this.SessionKey = env.Key("JWT_SESSION_TOKEN");
            this.AccessKey = env.Key("JWT_ACCESS_TOKEN");
            this.Issuer = env.Key("JWT_ISSUER");
            this.Audience = env.Key("JWT_AUDIENCE");
            this.UserId = id;
            this.Username = username;
        }
        private SigningCredentials Credentials(string key)
        {
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            return new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
        }

        public  List<Claim> CreateClaim()
        {
            try
            {
                var claims = new List<Claim>();

                claims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens
                                    .JwtRegisteredClaimNames.Jti, 
                                    Guid.NewGuid().ToString()));
                claims.Add(new Claim("userId", this.UserId));
                claims.Add(new Claim("username", this.Username));
                return claims;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return null;
            }
        }
        public Task<JwtSecurityToken> TokenizeAsync(SessionType type)
        {
            return Task.Run(()=>
            {
                try
                {
                    var claims = CreateClaim();
                    var signingCredentials = Credentials(type == 0 ? 
                        this.SessionKey : this.AccessKey);
                    var expires = type == 0 ? Expiration.MONTH : Expiration.SHORT;
                    var token = new JwtSecurityToken(
                            Issuer,
                            Audience,
                            claims,
                            expires: expires,
                            signingCredentials: signingCredentials
                        );
                    return token;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                    return null;
                }
            });
        }
        public async Task<dynamic> TokenHandlerAsync(SessionType type)
        {
            return await Task.Run(async () => {
                SecurityToken token = await TokenizeAsync(type);
                return new JwtSecurityTokenHandler().WriteToken(token);
            });
        }

        public TokenValidationParameters CreateValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = this.Issuer,
                ValidAudience = this.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(this.SessionKey))
            };
        }
        public async Task<bool> ValidateSessionToken(string token)
        {

            return await Task.Run(() =>
            {
                try
                {
                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    var validationParams = CreateValidationParameters();

                    SecurityToken validatedToken;
                    IPrincipal principal = tokenHandler.ValidateToken(token, validationParams, out validatedToken);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }
    }
}