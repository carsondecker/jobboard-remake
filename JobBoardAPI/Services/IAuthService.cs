using JobBoardAPI.Data;

namespace JobBoardAPI.Services
{
    public enum AuthResult 
    {
        Ok,
        Created,
        IllegalCredentials,
        UserAlreadyExists,
        UserNotFound,
        IncorrectCredentials,
        DBError,
    }

    public class AuthResponse
    {
        public AuthResult Result;
        public string? Token;
        public Guid? UserId;
        public string? Username;
    }

    public interface IAuthService
    {
        public Task<AuthResponse> Register(string username, string password);
        public Task<AuthResponse> Login(string username, string password);

    }
}
