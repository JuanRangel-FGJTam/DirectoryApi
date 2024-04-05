using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthApi.Data;
using AuthApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace AuthApi.Services
{
    public class PersonService(DirectoryDBContext dbContext)
    {
        
        private readonly DirectoryDBContext dbContext = dbContext;

        public enum SearchMode {
            Like = 1,
            Equals = 2
        }

        public IQueryable<Person> GetPeople()
        {
            return this.dbContext.People
                .Include(p => p.Gender)
                .Include(p => p.MaritalStatus)
                .Include(p => p.Nationality)
                .Include(p => p.Occupation)
                .OrderBy(p => p.CreatedAt)
                .AsQueryable();
        }

        public IEnumerable<Person> Search( string email, string? curp, string? name, SearchMode searchMode = SearchMode.Equals ){

            var peopleQuery = this.GetPeople();

            if( !string.IsNullOrEmpty( email)){
                peopleQuery = peopleQuery.Where( p =>
                    (searchMode == SearchMode.Equals)
                        ? (p.Email??"").ToLower().Equals(email.ToLower())
                        : p.Email != null && p.Email.Contains(email)
                );
            }

            if( !string.IsNullOrEmpty(curp)){
                peopleQuery = peopleQuery.Where( p =>
                    (searchMode == SearchMode.Equals)
                        ? p.Curp.ToLower().Equals(curp.ToLower())
                        : p.Curp.Contains(curp)
                );
            }

            if( !string.IsNullOrEmpty(name)){
                peopleQuery = peopleQuery.Where( p => 
                    (p.Name??"").Contains(name) ||
                    (p.FirstName??"").Contains(name) ||
                    (p.LastName??"").Contains(name)
                );
            }

            return peopleQuery.ToArray();

        }

    }
}