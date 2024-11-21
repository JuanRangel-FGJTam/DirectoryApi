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
        Task<User?> UpdateUser(int userId, UserUpdateRequest userUpdateRequest);
        Task<User?> GetByEmail(string email);

        Task<IEnumerable<Role>> GetRolesAvailables();
        Task<int> AttachRoleToUser(User user, Role role);
        Task<int> DetachRoleFromUser(User user, Role role);
        Task<int> AttachClaimUser(User user, string claimType, string claimValue);
        Task<int> DetachClaimUser(User user, string claimType);
    }
}