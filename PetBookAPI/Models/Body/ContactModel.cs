using PetBookAPI.Models.Body.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PetBookAPI.Models.Body
{
    [MetadataType(typeof(IContactModel))]
    public class ContactModel : IContactModel
    {
        
        public string Contact
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}