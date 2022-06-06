using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PetBookAPI.Models.Body.Interfaces
{
    public interface IContactModel
    {
        [RegularExpression("[0-9]", ErrorMessage = "0-9 characters only.")]
        [MaxLength(15, ErrorMessage = "Max characters are fifteen (15).")]
        string Contact { get; set; }
    }
}