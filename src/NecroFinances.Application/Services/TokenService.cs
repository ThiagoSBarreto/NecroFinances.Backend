using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NecroFinances.Application.Interfaces;
using NecroFinances.Application.Interfaces.Repositories;
using NecroFinances.Application.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace NecroFinances.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwt;
        private readonly IUserRepositorie _userRepositorie;
        public TokenService(
            IOptions<JwtSettings> jwt,
            IUserRepositorie userRepositorie
            )
        {
            _jwt= jwt.Value;
            _userRepositorie = userRepositorie;
        }

        public TokenResponse GenerateToken(UserModel user)
        {
            return new TokenResponse
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        private string GenerateAccessToken(UserModel user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.TokenKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationTimeMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(UserModel user)
        {
            var refreshToken = CreateRefreshToken();
            _userRepositorie.AddRefreshToken(new RefreshTokenModel
            {
                UserID = user.ID,
                TokenHash = HashRefreshToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationTimeDays)
            });

            return refreshToken;
        }

        public TokenResponse RefreshToken(string refreshToken)
        {
            var tokenHash = HashRefreshToken(refreshToken);
            var refreshTokenSession = _userRepositorie.GetRefreshTokenByHash(tokenHash);
            if (refreshTokenSession == null)
            {
                throw new UnauthorizedAccessException("Sessão inválida.");
            }

            if (refreshTokenSession.ExpiresAt <= DateTime.UtcNow)
            {
                _userRepositorie.RemoveRefreshToken(tokenHash);
                throw new UnauthorizedAccessException("Sessão expirada, por favor, faça o Login novamente.");
            }

            var user = _userRepositorie.GetUserById(refreshTokenSession.UserID);
            if (user == null)
            {
                _userRepositorie.RemoveRefreshToken(tokenHash);
                throw new UnauthorizedAccessException("Sessão inválida.");
            }

            var nextRefreshToken = CreateRefreshToken();
            _userRepositorie.UpdateRefreshToken(
                refreshTokenSession.Id,
                HashRefreshToken(nextRefreshToken),
                DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationTimeDays));

            return new TokenResponse
            {
                AccessToken = GenerateAccessToken(user),
                RefreshToken = nextRefreshToken
            };
        }

        public void RevokeRefreshToken(string refreshToken)
        {
            _userRepositorie.RemoveRefreshToken(HashRefreshToken(refreshToken));
        }

        private static string CreateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private static string HashRefreshToken(string refreshToken)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
