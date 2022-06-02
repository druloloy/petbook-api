using Microsoft.IdentityModel.Tokens;
using PetBookAPI.Extras.Extensions.JwtSecurityTokenExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PetBookAPI.Extras.Extensions.HttpRequestHeaders
{
    public static class HttpRequestHeadersExtensions
    {
        public static bool HasSessionToken(this System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            return headers.Contains("Session-Token");
        }

        public static bool IsSessionValid(this System.Net.Http.Headers.HttpRequestHeaders headers, TokenValidationParameters parameters)
        {
            string sessionToken = headers.GetValues("Session-Values").First();
            JwtToken token = new JwtToken(sessionToken, parameters);
            return token != null;
        }

    }
}