using System;
using System.Collections.Generic;
using AuthApi.Entities;
using AuthApi.Models;

namespace AuthApi.Data
{
    public interface IUserService
    {
        Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model);
        Task<IEnumerable<User>> GetAll();
        Task<User?> GetById(int id);
        Task<int?> CreateUser(UserRequest userRequest);
        Task<User?> AddAndUpdateUser(User user);
    }
}