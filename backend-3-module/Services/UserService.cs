using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend_3_module.Data;
using backend_3_module.Data.DataBases;
using backend_3_module.Data.DTO;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Errors;
using backend_3_module.Helpers;
using backend_3_module.Services.IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace backend_3_module.Services;

public class UserService : IUserService
{
    private readonly BlogDbContext _dbContext;
    private readonly RedisDbContext _redisDbContext;
    private readonly Token _tokenHelper;

    public UserService(BlogDbContext dbContext, RedisDbContext redisDbContext, Token tokenHelper)
    {
        _dbContext = dbContext;
        _redisDbContext = redisDbContext;
        _tokenHelper = tokenHelper;
    }

    public async Task<bool> IsUniqueEmailAsync(string email)
    {
        return !await _dbContext.Users.AnyAsync(user => user.Email == email);
    }

    public async Task<TokenResponseDTO> Register(RegistrationDTO registrationDto)
    {
        if (!await IsUniqueEmailAsync(registrationDto.Email))
            throw new BadRequestException("Такой Email уже существует.");

        byte[] passwordHash, passwordSalt;
        PasswordHasher.CreatePasswordHash(registrationDto.Password, out passwordHash, out passwordSalt);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            FullName = registrationDto.FullName,
            BirthDate = registrationDto.BirthDate,
            Gender = registrationDto.Gender,
            Email = registrationDto.Email,
            PhoneNumber = registrationDto.PhoneNumber,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        var token = _tokenHelper.GenerateToken(newUser);

        return new TokenResponseDTO { Token = token };
    }

    public async Task Logout(string token)
    {
        await _redisDbContext.AddTokenToBlackList(token);
    }

    public async Task<TokenResponseDTO> Login(LoginDTO loginDto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
            throw new BadRequestException("Пользователь не найден.");

        if (!PasswordHasher.VerifyPasswordHash(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            throw new BadRequestException($"Неверный пароль для {loginDto.Email}");

        var token = _tokenHelper.GenerateToken(user);

        return new TokenResponseDTO { Token = token };
    }

    public async Task<UserDTO> GetProfile(Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден.");

        return new UserDTO()
        {
            Id = user.Id,
            CreateTime = user.CreateTime,
            FullName = user.FullName,
            BirthDate = user.BirthDate,
            Gender = user.Gender,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };
    }

    public async Task EditProfile(Guid userId, EditUserDTO editUserDto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден.");

        user.FullName = editUserDto.FullName;
        user.BirthDate = editUserDto.BirthDate;
        user.Gender = editUserDto.Gender;
        if (user.Email != editUserDto.Email)
        {
            if (!await IsUniqueEmailAsync(editUserDto.Email))
                throw new BadRequestException("Такой Email уже существует.");
            user.Email = editUserDto.Email;
        }

        user.PhoneNumber = editUserDto.PhoneNumber;
        await _dbContext.SaveChangesAsync();
    }
}