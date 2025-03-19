using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthApi.Services
{
    public class ResetPasswordState
    {
        private readonly List<ResetPasswordRecord> resetPasswordRecords = [];

        public void AddRecord( Guid person_id, string code, DateTime time){
            var _record = this.resetPasswordRecords.Where( item => item.PersonID == person_id).FirstOrDefault();
            if( _record != null ){
                // Update the record
                _record.ResetCode = code;
                _record.Time = time;
            }else{
                // Add new record
                this.resetPasswordRecords.Add( new ResetPasswordRecord{
                    PersonID = person_id,
                    ResetCode = code,
                    Time = time
                });
            }
        }

        public Guid? Validate( string code ){
            var _record = this.resetPasswordRecords.Where( item => item.ResetCode.Equals(code, StringComparison.CurrentCultureIgnoreCase) && item.Time >= DateTime.Now).FirstOrDefault();
            return _record != null ? _record.PersonID : null;
        }

        public void Remove( Guid person_id ){
            this.resetPasswordRecords.RemoveAll( item => item.PersonID == person_id);
        }

        public ResetPasswordRecord? GetByPersonId(Guid person_id)
        {
            return resetPasswordRecords.Where(item => item.PersonID == person_id).OrderByDescending(item => item.Time).FirstOrDefault();
        }
    }

    public class ResetPasswordRecord
    {
        public Guid PersonID {get; set;}
        public string ResetCode {get; set;} = null!;
        public DateTime Time {get; set;}   
    }
}