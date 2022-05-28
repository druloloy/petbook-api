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

namespace PetBookAPI.Extras
{
    public enum SessionType
    {
        SESSION = 0,
        ACCESS = 1
    }
    public class Object
    {
        public string Key { get; }
        public dynamic Value { get; }
        public Object(string key, dynamic value)
        {
            Key = key;
            Value = value;
        }
    }
    public class Session
    {
        private Environment env = new Environment();
        private string SessionKey;
        private string AccessKey;
        private string Issuer;


        public Session()
        {
            /**
             * Make sure you have "env.config.json" on your root folder before you call on json key
             **/
            this.SessionKey = env.Key("JWT_SESSION_TOKEN");
            this.AccessKey = env.Key("JWT_ACCESS_TOKEN");
            this.Issuer = env.Key("JWT_ISSUER");
        }
        private SigningCredentials Credentials(string key)
        {
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            return new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
        }

        public List<Claim> CreateClaim(List<Object> obj)
        {
            try
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                obj.ForEach(o =>
                {
                    claims.Add(new Claim(o.Key, o.Value));
                });

                return claims;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return null;
            }
        }
        public JwtSecurityToken Tokenize(List<Object> obj, SessionType type)
        {
            try
            {
                var claims = CreateClaim(obj);
                var signingCredentials = Credentials(type == 0 ? this.SessionKey : this.AccessKey);
                var expiration = type == 0 ? DateTime.Now.AddDays(30) : DateTime.Now.AddMinutes(15);
                var token = new JwtSecurityToken(
                        Issuer,
                        Issuer,
                        claims,
                        expires: expiration,
                        signingCredentials: signingCredentials
                    );
                return token;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return null;
            }
        }
        public dynamic TokenHandler(SessionType type, List<Object> claims)
        {
            SecurityToken token = Tokenize(claims, type);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}