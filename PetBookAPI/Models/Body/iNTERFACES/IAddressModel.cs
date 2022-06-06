using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PetBookAPI.Models.Body.Interfaces
{
    public interface IAddressModel
    {
        [RegularExpression(@"^[a-zA-Z0-9\s#'.,\-()]*$", ErrorMessage = @"Acceptable characters are: a-z A-Z 0-9#'.,\-()")]
        string Line { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9\s#'.,\-()]*$", ErrorMessage = @"Acceptable characters are: a-z A-Z 0-9#'.,\-()")]
        string Barangay { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9\s#'.,\-()]*$", ErrorMessage = @"Acceptable characters are: a-z A-Z 0-9#'.,\-()")]
        string City { get; set; }
        [RegularExpression(@"^[a-zA-Z0-9\s#'.,\-()]*$", ErrorMessage = @"Acceptable characters are: a-z A-Z 0-9#'.,\-()")]
        string Country { get; set; }
    }
}