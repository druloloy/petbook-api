using PetBookAPI.Extras;
using PetBookAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

namespace PetBookAPI.Controllers
{
    
    public class SignupController : ApiController
    {
        PetBookEntities db = new PetBookEntities();


        [HttpPost]
        public async Task<IHttpActionResult> CreateAccount(account_credential newAcc)
        {
            string hashedPassword, sessionId = "";
            string sessionToken = null, accessToken = null;
            Session session = new Session();
            List<Extras.Object> claims = null;
            login_sessions loginSession = null;
            UID uid = new UID(IdSize.SHORT);
            try
            {
                bool userExists = await db.account_credential
                                    .AnyAsync(u=>u.Username.Equals(newAcc.Username) || u.Email.Equals(newAcc.Email));

                if (userExists) return BadRequest();

                string accountId = await uid.Generate();
                newAcc.Id = accountId;
                hashedPassword = await PasswordManager.Hash(newAcc.Password);
                newAcc.Password = hashedPassword;

                claims = new List<Extras.Object>(){
                    new Extras.Object("userId", newAcc.Id),
                    new Extras.Object("username", newAcc.Username)
                };

                sessionId = await uid.Generate();
                sessionToken = session.TokenHandler(SessionType.SESSION, claims);
                accessToken = session.TokenHandler(SessionType.ACCESS, claims);

                loginSession = new login_sessions()
                {
                    Id = sessionId,
                    AccountId = newAcc.Id, // use user id as foreign key
                    Token = sessionToken,
                    ExpiresAt = DateTime.Now.AddDays(30)
                };
                    
                  
                newAcc.login_sessions.Add(loginSession);
                db.account_credential.Add(newAcc);
                await db.SaveChangesAsync();



                return Content(HttpStatusCode.Created, new
                {
                    userId = newAcc.Id,
                    session = loginSession.Token,
                    access = accessToken
                });
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
                Console.WriteLine(e.StackTrace);
                return InternalServerError();
            }
        }



        private bool UsernameComparator (account_credential user, account_credential newuser)
        {
            return user.Username.Equals(newuser.Username);
        }
        private bool EmailComparator(account_credential user, account_credential newuser)
        {
            return user.Email.Equals(newuser.Email);
        }
    }
}

