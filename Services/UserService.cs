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

            if( !ValidatePassword( userRequest.Password!, userRequest.ConfirmPassword, out KeyValuePair<string,string>? validationResult )){
                errorMessages.Add( validationResult!.Value  );
                throw new SimpleValidationException( "Validations fail at create user", errorMessages );
            }

            var _newUser = new User(){
                FirstName = userRequest.FirstName!,
                LastName = userRequest.LastName??"",
                Email = userRequest.Email!,
                Password = cryptographyService.HashData( userRequest.Password!),
                isActive = true
            };

            // Store user
            db.Users.Add( _newUser );
            await db.SaveChangesAsync();
            return _newUser.Id;
        }
        public async Task<User?> UpdateUser(int userId, UserUpdateRequest userUpdateRequest)
        {
            var errorMessages = new List<KeyValuePair<string,string>>();
            
            // Get and validate user
            var _user = await db.Users.FirstOrDefaultAsync( u => u.Id == userId);
            if( _user == null){
                errorMessages.Add( new KeyValuePair<string, string>("userId", "User not found"));
                throw new SimpleValidationException( "Validations fail at update user", errorMessages );
            }


            // Set new values
            _user.FirstName = userUpdateRequest.FirstName != null ? userUpdateRequest.FirstName : _user.FirstName;
            _user.LastName = userUpdateRequest.LastName !=null ?userUpdateRequest.LastName :_user.LastName;
            _user.Email = userUpdateRequest.Email != null ?userUpdateRequest.Email : _user.Email;
            if( userUpdateRequest.Password != null){
                if( !ValidatePassword(userUpdateRequest.Password, userUpdateRequest.ConfirmPassword, out KeyValuePair<string,string>? validationResult )){
                    errorMessages.Add( validationResult!.Value  );
                    throw new SimpleValidationException( "Validations fail at create user", errorMessages );
                }
                _user.Password = cryptographyService.HashData( userUpdateRequest.Password);
            }

            // Update entity
            db.Users.Update( _user );
            var count = await db.SaveChangesAsync();
            return _user;
           
        }

        public async Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model)
        {
            var user = await db.Users.SingleOrDefaultAsync(x => x.Email == model.Email && x.Password == cryptographyService.HashData(model.Password!));

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
        
        
        #region Local Functions
        private bool ValidatePassword( string password, string? confirmPassword, out KeyValuePair<string,string>? validationResult )
        {
            if( password != confirmPassword)
            {
                validationResult = new KeyValuePair<string,string>("password", "The passwords are not equals");
                return false;
            }

            validationResult = null;
            return true;
        }
        #endregion
    }
}