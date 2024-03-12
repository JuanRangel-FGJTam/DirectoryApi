using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthApi.Models;
using AuthApi.Data;
using AuthApi.Entities;
using AuthApi.Helper;

namespace AuthApi.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _appSettings;
        private readonly DirectoryDBContext db;
        private readonly ICryptographyService cryptographyService;

        public UserService( IConfiguration appSettings, DirectoryDBContext _db, ICryptographyService cryptographyService)
        {
            _appSettings = appSettings;
            db = _db;
            this.cryptographyService = cryptographyService;
        }


        public async Task<User?> AddAndUpdateUser(User userObj)
        {
            bool isSuccess = false;
            if (userObj.Id > 0)
            {
                var obj = await db.Users.FirstOrDefaultAsync(c => c.Id == userObj.Id);
                if (obj != null)
                {
                    // obj.Address = userObj.Address;
                    obj.FirstName = userObj.FirstName;
                    obj.LastName = userObj.LastName;
                    db.Users.Update(obj);
                    isSuccess = await db.SaveChangesAsync() > 0;
                }
            }
            else
            {
                await db.Users.AddAsync(userObj);
                isSuccess = await db.SaveChangesAsync() > 0;
            }

            return isSuccess ? userObj: null;
        }

        public async Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model)
        {
            var user = await db.Users.SingleOrDefaultAsync(x => x.Username == model.Username && x.Password == cryptographyService.HashData(model.Password));

            // return null if user not found
            if (user == null) return null;

            // authentication successful so generate jwt token
            var _secret = _appSettings.GetValue<string>("Secret");
            var token = await JwTokenHelper.GenerateJwtToken(user, _secret! );

            return new AuthenticateResponse( user, token);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await db.Users.Where(x => x.isActive == true).ToListAsync();
        }

        public async Task<User?> GetById(int id)
        {
            return await db.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

    }
}