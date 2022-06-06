using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PetBookAPI.Models.Body.Interfaces
{
    public interface IFullNameModel
    {
        [RegularExpression(@"^[a-zA-Z0-9\s#'.,\-()]*$", ErrorMessage = @"Acceptable characters are: a-z A-Z 0-9#'.,\-()")]
        string FirstName { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9\s#'.,\-()]*$", ErrorMessage = @"Acceptable characters are: a-z A-Z 0-9#'.,\-()")]
        string MiddleName { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9\s#'.,\-()]*$", ErrorMessage = @"Acceptable characters are: a-z A-Z 0-9#'.,\-()")]
        string LastName { get; set; }
    }
}