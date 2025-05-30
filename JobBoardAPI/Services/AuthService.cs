using JobBoardAPI.Config;
using JobBoardAPI.Data;
using JobBoardAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bcrypt = BCrypt.Net.BCrypt;

namespace JobBoardAPI.Services
{
    public class AuthService : IAuthService
    {
        private const int MinPasswordLength = 8;
        private readonly DataContext _dbContext;
        private readonly JwtSettings _jwtSettings;
        public AuthService(DataContext dbContext, JwtSettings jwtSettings)
        {
            _dbContext = dbContext;
            _jwtSettings = jwtSettings;
        }

        public async Task<AuthResponse> Register(string username, string password)
        {
            try
            {
                if (password.Length < MinPasswordLength || username.Length < MinPasswordLength)
                {
                    return new AuthResponse { Result = AuthResult.IllegalCredentials };
                }

                var usernameUsed = await _dbContext.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
                if (usernameUsed)
                {
                    return new AuthResponse { Result = AuthResult.UserAlreadyExists };
                }

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = Bcrypt.HashPassword(password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _dbContext.Users.AddAsync(user);

                await _dbContext.SaveChangesAsync();

                return new AuthResponse
                {
                    Result = AuthResult.Created,
                    UserId = user.UserId,
                    Username = user.Username,
                    Token = GenerateJwtToken(user)
                };
            }
            catch (Exception)
            {
                return new AuthResponse { Result = AuthResult.DBError };
            }
        }

        public async Task<AuthResponse> Login(string username, string password)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

                if (user == null)
                {
                    return new AuthResponse { Result = AuthResult.UserNotFound };
                }

                if (!Bcrypt.Verify(password, user.PasswordHash))
                {
                    return new AuthResponse { Result = AuthResult.IncorrectCredentials };
                }
                return new AuthResponse {
                    Result = AuthResult.Ok,
                    Token = GenerateJwtToken(user),
                    UserId = user.UserId,
                    Username = username
                };
            }
            catch (Exception)
            {
                return new AuthResponse { Result = AuthResult.DBError };
            }
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
