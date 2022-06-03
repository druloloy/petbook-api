using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetBookAPI.Models.Micro
{
    public class OwnerInformationModel
    {

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public string Line { get; set; }
        public string Barangay { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string ContactNumber { get; set; }
    }
}