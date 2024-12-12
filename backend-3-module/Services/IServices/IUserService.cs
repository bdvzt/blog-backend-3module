using backend_3_module.Data.DTO;

namespace backend_3_module.Services.IServices;

public interface IUserService
{
    public Task<bool> IsUniqueEmailAsync(string email);
    public Task<TokenResponseDTO> Login(LoginDTO loginDto);
    public Task<TokenResponseDTO> Register(RegistrationDTO registrationDto);
    public Task Logout(string token);
    public Task<UserDTO> GetProfile(Guid token);
    public Task EditProfile(Guid token, EditUserDTO editUserDto);
}