using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Tokens;
using System.Text;


[assembly: OwinStartup(typeof(PetBookAPI.Startup))]

namespace PetBookAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Extras.Environment env = new Extras.Environment();
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = env.Key("JWT_ISSUER"), 
                        ValidAudience = env.Key("JWT_AUDIENCE"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(env.Key("JWT_ACCESS_TOKEN")))
                    }
                });
        }
    }
}
