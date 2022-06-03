using PetBookAPI.Extras;
using PetBookAPI.Extras.Extensions.String;
using PetBookAPI.Models;
using PetBookAPI.Models.Micro;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace PetBookAPI.Controllers
{
    public class OwnerProfileController : ApiController
    {
        [Authorize]
        [HttpPost]
        [Route("owner/add")]
        public async Task<IHttpActionResult> AddOwnerInformation(OwnerInformationModel ownerInput)
        {
            try
            {
                ClaimsIdentity identity = User.Identity as ClaimsIdentity;
                var userId = identity.Claims.First(c => c.Type.Equals("userId")).Value;

                using (MainDbEntities db = new MainDbEntities())
                {
                    owner_contact contactNumber = new owner_contact()
                    {
                        Id = await new UID(IdSize.SHORT).GenerateIdAsync(),
                        UserId = userId,
                        Contact = ownerInput.ContactNumber
                    };

                    address_details address = new address_details()
                    {
                        Id = userId,
                        Line = ownerInput.Line,
                        Barangay = ownerInput.Barangay,
                        City = ownerInput.City,
                        Country = ownerInput.Country
                    };
                    owner_profile profile = new owner_profile()
                    {
                        Id = userId,
                        FirstName = ownerInput.FirstName,
                        MiddleName = ownerInput.MiddleName,
                        LastName = ownerInput.LastName,
                        address_details = address
                    };
                    profile.owner_contact.Add(contactNumber);

                    db.owner_profile.Add(profile);
                    await db.SaveChangesAsync();
                    
                }
                return Content(HttpStatusCode.Created, "Profile created!");
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
    }
}
