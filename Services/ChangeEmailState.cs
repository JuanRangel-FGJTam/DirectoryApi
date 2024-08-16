using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Services
{
    public class ChangeEmailState
    {
        private readonly List<ChangeEmailRecord> changeEmailRecords = [];

        public void AddRecord( Guid person_id, string code, DateTime time, string email){
            var _record = this.changeEmailRecords.Where( item => item.PersonID == person_id).FirstOrDefault();
            if( _record != null ){
                // Update the record
                _record.ResetCode = code;
                _record.Time = time;
                _record.Email = email;
            }else{
                // Add new record
                this.changeEmailRecords.Add( new ChangeEmailRecord{
                    PersonID = person_id,
                    ResetCode = code,
                    Time = time,
                    Email = email

                });
            }
        }

        public Guid? Validate( string code, string newEmail ){
            var _record = this.changeEmailRecords
                .Where( item => item.ResetCode.Equals(code, StringComparison.CurrentCultureIgnoreCase) && item.Time >= DateTime.Now)
                .Where( item => item.Email == newEmail)
                .FirstOrDefault();
            return _record != null ? _record.PersonID : null;
        }

        public void Remove( Guid person_id ){
            this.changeEmailRecords.RemoveAll( item => item.PersonID == person_id);
        }

    }

    public class ChangeEmailRecord
    {
        public Guid PersonID {get; set;}
        public string ResetCode {get; set;} = null!;
        public string Email {get; set;} = null!;
        public DateTime Time {get; set;}
    }
}