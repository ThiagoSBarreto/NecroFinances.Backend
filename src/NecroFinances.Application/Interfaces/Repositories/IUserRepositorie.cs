using NecroFinances.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NecroFinances.Application.Interfaces.Repositories
{
    public interface IUserRepositorie
    {
        Task<UserModel> IsRegisteredAsync(string username);
        Task<UserModel> AddNewUser(UserModel user);
        void AddRefreshToken(RefreshTokenModel refreshToken);
        RefreshTokenModel? GetRefreshTokenByHash(string tokenHash);
        void UpdateRefreshToken(long id, string tokenHash, DateTime expiresAt);
        void RemoveRefreshToken(string tokenHash);
        UserModel? GetUserById(long userID);
        UserModel UpdateUser(UserModel user);
        void SaveHideValues(UserModel user);
    }
}
