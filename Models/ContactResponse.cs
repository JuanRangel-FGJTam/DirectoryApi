using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;

namespace AuthApi.Models
{
    public class ContactResponse
    {
        
        public string ContactId {get;set;}

        public int ContactTypeId {get;set;}
        public string ContactTypeName {get;set;}
        
        public string Value {get;set;}

        public  ContactResponse( Guid contactId){
            this.ContactId = contactId.ToString();
            this.ContactTypeId = 0;
            this.ContactTypeName = "";
            this.Value = "";
        }

        public ContactResponse( string contactId ){
            this.ContactId = contactId;
            this.ContactTypeId = 0;
            this.ContactTypeName = "";
            this.Value = "";

        }

        public static ContactResponse FromEntity(ContactInformation contactInformation){
            return new ContactResponse( contactInformation.Id){
                ContactTypeId = contactInformation.ContactType.Id,
                ContactTypeName = contactInformation.ContactType.Name,
                Value = contactInformation.Value
            };
        }

    }
}