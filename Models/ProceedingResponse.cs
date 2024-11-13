using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Entities;
using AuthApi.Services;

namespace AuthApi.Models
{
    public class ProceedingResponse( string peopleId, string name)
    {
        public int Id {get;set;}
        public string PeopleId {get;set;} = peopleId;
        public string Name {get;set;} = name;
        public string? Folio {get;set;}
        public string? DenunciaId {get;set;}
        public string? Observations {get;set;}

        public string? Status {get;set;}
        public int StatusId {get;set;}

        public string? Area {get;set;}
        public int AreaId {get;set;}

        public DateTime? CreatedAt {get;set;}

        public IEnumerable<ProceedingFileResponse> Files {get;set;} = Array.Empty<ProceedingFileResponse>();


        public static ProceedingResponse FromIdentity(Proceeding p){
            var item = new ProceedingResponse(p.PersonId.ToString(), p.Name ?? "")
            {
                Id = p.Id,
                Folio = p.Folio,
                CreatedAt = p.CreatedAt,
                DenunciaId = p.DenunciaId,
                Observations = p.Observations
            };
            
            if ( p.Status != null){
                item.Status = p.Status.Name;
                item.StatusId = p.Status.Id;
            }
            
            if(p.Area != null){
                item.Area = p.Area.Name;
                item.AreaId = p.Area.Id;
            }

            if(p.Files?.Count > 0){
                item.Files = p.Files.Select( f => ProceedingFileResponse.FromEnity(f));
            }
            
            return item;
        }
    }
}