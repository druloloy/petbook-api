using Microsoft.IdentityModel.Tokens;
using PetBookAPI.Extras;
using PetBookAPI.Extras.Extensions.HttpRequestHeaders;
using PetBookAPI.Extras.Extensions.JwtSecurity;
using PetBookAPI.Extras.Extensions.JwtSecurityTokenExtensions;
using PetBookAPI.Models;
using PetBookAPI.Models.Body;
using PetBookAPI.Models.Micro;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace PetBookAPI.Controllers
{
    public class AuthenticationController : ApiController
    {
        [AllowAnonymous]
        [HttpPost]
        [Route("auth/signup")]
        public async Task<IHttpActionResult> CreateAccount(account_credential newAcc)
        {
            string hashedPassword = "", sessionId = "",
                sessionToken = "", accessToken = "";
            Session session = null;
            login_sessions loginSession = null;
            UID uid = new UID(IdSize.SHORT);

            try
            {
                using (MainDbEntities db = new MainDbEntities())
                {
                    bool userExists = await db.account_credential
                                    .AnyAsync(u => u.Username.Equals(newAcc.Username) || u.Email.Equals(newAcc.Email));

                    if (userExists) return BadRequest();

                    string accountId = await uid.GenerateIdAsync();
                    newAcc.Id = accountId;
                    hashedPassword = await PasswordManager.HashAsync(newAcc.Password);
                    newAcc.Password = hashedPassword;

                    session = new Session(newAcc.Id, newAcc.Username);

                    sessionId = await uid.GenerateIdAsync();
                    sessionToken = await session.TokenHandlerAsync(SessionType.SESSION);
                    accessToken = await session.TokenHandlerAsync(SessionType.ACCESS);

                    loginSession = new login_sessions()
                    {
                        Id = sessionId, // unique id
                        AccountId = newAcc.Id, // use user id as foreign key
                        Token = sessionToken,
                        ExpiresAt = DateTime.Now.AddDays(30)
                    };


                    newAcc.login_sessions.Add(loginSession);
                    db.account_credential.Add(newAcc);
                    await db.SaveChangesAsync();

                    var response = Request.CreateResponse(HttpStatusCode.Created, new
                    {
                        userId = newAcc.Id,
                        session = loginSession.Token
                    });

                    var cookie = new CookieHeaderValue("access_token", accessToken);
                    cookie.Domain = Request.RequestUri.Host;
                    cookie.Path = "/";
                    cookie.Expires = Expiration.SHORT;

                    response.Headers.AddCookies(new CookieHeaderValue[] { cookie });


                    return ResponseMessage(response);
                }
            }
            catch (DbUpdateException e)
            {
                Debug.WriteLine(e.InnerException);
                return StatusCode(HttpStatusCode.BadRequest);
            }
            catch (DbEntityValidationException e)
            {
                var errs = e.EntityValidationErrors.ToList();
                string errorMessage = errs[0].ValidationErrors.ToList()[0].ErrorMessage;

                errs.ForEach(err =>
                {
                    var validationErrors = err.ValidationErrors.ToList();
                    validationErrors.ForEach(er =>
                    {
                        Debug.WriteLine($"property_name: {er.PropertyName}; errorMessage: {er.ErrorMessage}");
                    });
                });
                var errObj = new
                {
                    message = errorMessage,
                    code = HttpStatusCode.BadRequest,
                    stack = e.EntityValidationErrors.ToList()
                };
                return Content(HttpStatusCode.BadRequest, errObj);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return InternalServerError();
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("auth/login")]
        public async Task<IHttpActionResult> Login(LoginModel userLogin)
        {
            login_sessions loginSession = null;
            account_credential user = null;
            string sessionId = "", accessToken = "", sessionToken = "";
            Session session = null;
            UID uid = new UID(IdSize.SHORT);

            try
            {
                using (MainDbEntities db = new MainDbEntities())
                {
                    user = await db.account_credential
                             .FirstOrDefaultAsync(u => u.Username.Equals(userLogin.Username));

                    if(user == null)
                        return Content(HttpStatusCode.BadRequest, new { message = "Invalid username/password."});

                    // if password doesn't match
                    if(!await PasswordManager.IsMatchedAsync(userLogin.Password, user.Password))
                        return Content(HttpStatusCode.BadRequest, new { message = "Invalid username/password." });
                    
                    

                    // generate new token 
                    session = new Session(user.Id, user.Username);
                    sessionId = await uid.GenerateIdAsync();
                    sessionToken = await session.TokenHandlerAsync(SessionType.SESSION);
                    accessToken = await session.TokenHandlerAsync(SessionType.ACCESS);

                    loginSession = new login_sessions()
                    {
                        Id = sessionId, // unique id
                        AccountId = user.Id, // use user id as foreign key
                        Token = sessionToken,
                        ExpiresAt = DateTime.Now.AddDays(30)
                    };

                    user.login_sessions.Add(loginSession);
                    await db.SaveChangesAsync();

                    var response = Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        userId = user.Id,
                        session = loginSession.Token
                    });

                    var cookie = new CookieHeaderValue("access_token", accessToken) {
                        Domain = Request.RequestUri.Host,
                        Path = "/",
                        Expires = Expiration.SHORT,
                        HttpOnly = true,
                        Secure = true
                    };
                    

                   
                    response.Headers.AddCookies(new CookieHeaderValue[] { cookie });


                    return ResponseMessage(response);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return InternalServerError();
            }
        }

        [Authorize]
        [HttpPost]
        [Route("auth/logout")]
        public IHttpActionResult Logout()
        {
            try
            {
                var headers = Request.Headers;
                if (!headers.HasSessionTokenHeader())
                    return Ok("Where is your session header?"); // return anyway

                JwtToken token = new JwtToken(headers.GetSessionToken(), 
                                                new Session().CreateValidationParameters());

                if (token == null)
                    return Ok("Invalid session."); // logout anyway

                PayloadModel payload = token.GetPayload();

                // delete session token if exists
                using(MainDbEntities db = new MainDbEntities())
                {
                    // get session from db
                    login_sessions activeSession = db.login_sessions
                                           .Single(s => s.Token.Equals(token.Value));
                    if (activeSession == null)
                        return Ok("Session not found in database.");

                    db.login_sessions.Remove(activeSession);
                    db.SaveChanges();
                }
                return Ok("If you reached here, your session is now removed from the database.");
            }
            catch (Exception)
            {
                return Ok("Your session is either invalid or something went wrong in our server.");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("auth/refresh")]
        public async Task<IHttpActionResult> GetAccessToken()
        {
            try
            {
                var headers = Request.Headers;
                // check if session header exists
                if (!headers.HasSessionTokenHeader())
                    return Content(HttpStatusCode.Forbidden, "You have no right for this request.");
                // check if session is valid
                JwtToken token = new JwtToken(headers.GetSessionToken(),
                                        new Session().CreateValidationParameters());
                if (token == null)
                    return Content(HttpStatusCode.Forbidden, "Invalid session token.");
                // check if session is in database
                using(MainDbEntities db = new MainDbEntities())
                {
                    var session = db.login_sessions
                                    .Where(s => s.Token.Equals(token.Value))
                                    .First();

                    if (session==null)
                        return Content(HttpStatusCode.Forbidden,
                            "Nice try! Your session is not recognized in the database.");       
                }

                // reuse session userId and username for access token generation
                PayloadModel payload = await token.GetPayloadAsync();
              
                string accessToken = await new Session(payload.UserId, payload.Username)
                                        .TokenHandlerAsync(SessionType.ACCESS);


                var response = Request.CreateResponse(HttpStatusCode.OK, "Access token is granted.");

                var cookie = new CookieHeaderValue("access_token", accessToken)
                {
                    Domain = Request.RequestUri.Host,
                    Path = "/",
                    Expires = Expiration.SHORT,
                    HttpOnly = true,
                    Secure = true
                };
                response.Headers.AddCookies(new CookieHeaderValue[] { cookie });

                return ResponseMessage(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.InnerException);
                return InternalServerError();
            }
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("dev/refresh")]
        public async Task<IHttpActionResult> GetAccessTokenDevMode()
        {
            try
            {
                var headers = Request.Headers;
                // check if session header exists
                if (!headers.HasSessionTokenHeader())
                    return Content(HttpStatusCode.Forbidden, "You have no right for this request.");
                // check if session is valid
                JwtToken token = new JwtToken(headers.GetSessionToken(),
                                        new Session().CreateValidationParameters());
                if (token == null)
                    return Content(HttpStatusCode.Forbidden, "Invalid session token.");
                // check if session is in database
                using (MainDbEntities db = new MainDbEntities())
                {
                    var session = db.login_sessions
                                    .Where(s => s.Token.Equals(token.Value))
                                    .First();

                    if (session == null)
                        return Content(HttpStatusCode.Forbidden,
                            "Nice try! Your session is not recognized in the database.");
                }

                // reuse session userId and username for access token generation
                PayloadModel payload = await token.GetPayloadAsync();

                string accessToken = await new Session(payload.UserId, payload.Username)
                                        .TokenHandlerAsync(SessionType.ACCESS);


                var response = Request.CreateResponse(HttpStatusCode.OK, new {
                    token = accessToken
                });
                

                return ResponseMessage(response);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.InnerException);
                return InternalServerError();
            }
        }
    }
}