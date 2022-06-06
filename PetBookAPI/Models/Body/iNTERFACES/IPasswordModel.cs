using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetBookAPI.Models.Body.Interfaces
{
    interface IPasswordModel
    {

        string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(16, ErrorMessage = "New password must be 8-128 characters.")]
        [MaxLength(128, ErrorMessage = "New password must be 8-128 characters.")]
        string NewPassword{ get; set; }
    }
}
