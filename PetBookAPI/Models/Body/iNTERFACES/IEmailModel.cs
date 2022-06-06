using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetBookAPI.Models.Body.Interfaces
{
    public interface IEmailModel
    {
        [EmailAddress(ErrorMessage = "Please use a valid email address.")]
        string Email { get; set; }
    }
}
