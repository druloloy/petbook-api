using PetBookAPI.Extras.Extensions.JwtSecurityTokenExtensions;
using PetBookAPI.Models.Body;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;

namespace PetBookAPI.Extras.Extensions.JwtSecurity
{
    public static class JwtSecurityTokenExtensions
    {
        public static PayloadModel GetPayload(this JwtToken token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.Value);

            PayloadModel payload = null;
                jwt.Claims
                    .ToList()
                    .ForEach(claim =>
                    {
                        payload = new PayloadModel()
                        {
                            JTI = claim.Type.Equals("jti") ? claim.Value : "",
                            UserId = claim.Type.Equals("userId") ? claim.Value : "",
                            Username = claim.Type.Equals("username") ? claim.Value : "",
                            Expiration = claim.Type.Equals("exp") ? claim.Value : "",
                            Issuer = claim.Type.Equals("iss") ? claim.Value : "",
                            Audience = claim.Type.Equals("aud") ? claim.Value : ""
                        };

                    });

            return payload;
        }
    }
}