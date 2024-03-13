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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using AuthApi.Data.Exceptions;

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


        /// <summary>
        ///  Create new user 
        /// </summary>
        /// <param name="userRequest"></param>
        /// <returns>Id generated of the user</returns>
        /// <exception cref="DbUpdateException"></exception>
        /// <exception cref="SimpleValidationException"></exception>
        public async Task<int?> CreateUser(UserRequest userRequest)
        {

            // Validate password
            var errorMessages = new List<KeyValuePair<string,string>>();
            if( userRequest.Password != userRequest.ConfirmPassword)
            {
                errorMessages.Add( new KeyValuePair<string,string>("password", "The passwords are not equals") );
            }
            if( !errorMessages.IsNullOrEmpty() ){
                throw new SimpleValidationException( "Validation fail at create user", errorMessages );
            }

            var _newUser = new User(){
                FirstName = userRequest.FirstName,
                LastName = userRequest.LastName,
                Username = userRequest.Username,
                Password = cryptographyService.HashData( userRequest.Password),
                isActive = true
            };

            db.Users.Add( _newUser );
            await db.SaveChangesAsync();
            return _newUser.Id;
        }
        public async Task<User?> AddAndUpdateUser(User user)
        {
            bool isSuccess = false;
            if (user.Id > 0)
            {
                var obj = await db.Users.FirstOrDefaultAsync(c => c.Id == user.Id);
                if (obj != null)
                {
                    // obj.Address = userObj.Address;
                    obj.FirstName = user.FirstName;
                    obj.LastName = user.LastName;
                    db.Users.Update(obj);
                    isSuccess = await db.SaveChangesAsync() > 0;
                }
            }
            else
            {
                await db.Users.AddAsync(user);
                isSuccess = await db.SaveChangesAsync() > 0;
            }

            return isSuccess ? user: null;
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