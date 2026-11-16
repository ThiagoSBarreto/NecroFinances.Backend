using Microsoft.EntityFrameworkCore;
using NecroFinances.Application.Interfaces.Repositories;
using NecroFinances.Application.Models;
using NecroFinances.Infrastructure.Persistence;

namespace NecroFinances.Infrastructure.Repositories
{
    public class UserRepositorie : IUserRepositorie
    {
        private readonly AppDbContext _context;
        public UserRepositorie(
            AppDbContext context
            )
        {
            _context = context;
        }
        public async Task<UserModel> AddNewUser(UserModel user)
        {
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<UserModel> IsRegisteredAsync(string username)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(f => f.Username == username);
        }

        public void AddRefreshToken(RefreshTokenModel refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();
        }

        public RefreshTokenModel? GetRefreshTokenByHash(string tokenHash)
        {
            return _context.RefreshTokens.FirstOrDefault(token => token.TokenHash == tokenHash);
        }

        public void UpdateRefreshToken(long id, string tokenHash, DateTime expiresAt)
        {
            var refreshToken = _context.RefreshTokens.FirstOrDefault(token => token.Id == id);
            if (refreshToken == null)
            {
                throw new InvalidOperationException("Sessão de refresh não encontrada.");
            }

            refreshToken.TokenHash = tokenHash;
            refreshToken.ExpiresAt = expiresAt;
            _context.SaveChanges();
        }

        public void RemoveRefreshToken(string tokenHash)
        {
            var refreshToken = _context.RefreshTokens.FirstOrDefault(token => token.TokenHash == tokenHash);
            if (refreshToken == null)
            {
                return;
            }

            _context.RefreshTokens.Remove(refreshToken);
            _context.SaveChanges();
        }

        public UserModel? GetUserById(long id)
        {
            return _context.Usuarios.FirstOrDefault(f => f.ID == id);
        }

        public UserModel UpdateUser(UserModel user)
        {
            _context.Entry(user).State = EntityState.Modified;
            return user;
        }
    }
}
