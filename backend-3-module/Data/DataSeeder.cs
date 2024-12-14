using backend_3_module.Data.Entities;
using backend_3_module.Helpers;

namespace backend_3_module.Data;

public class DataSeeder
{
    private readonly BlogDbContext _dbContext;

    public DataSeeder(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Seed()
    {
        Guid userId1 = Guid.Empty;
        Guid communityId1 = Guid.Empty;

        Guid userId2 = Guid.Empty;
        Guid communityId2 = Guid.Empty;

        Guid userId3 = Guid.Empty;
        Guid communityId3 = Guid.Empty;

        if (!_dbContext.Communities.Any())
        {
            var communities = new List<Community>
            {
                new Community
                {
                    Id = Guid.NewGuid(),
                    Name = "Профбюро Высшей IT-Школы",
                    Description = "Быть, а не казаться",
                    IsClosed = false,
                    CreateTime = DateTime.UtcNow
                },
                new Community
                {
                    Id = Guid.NewGuid(),
                    Name = "старый бог",
                    Description = "здесь",
                    IsClosed = false,
                    CreateTime = DateTime.UtcNow
                },
                new Community
                {
                    Id = Guid.NewGuid(),
                    Name = "пирожки с котятами",
                    Description = "люблю котят и бабушку",
                    IsClosed = true,
                    CreateTime = DateTime.UtcNow
                }
            };

            await _dbContext.Communities.AddRangeAsync(communities);
            await _dbContext.SaveChangesAsync();

            communityId1 = communities[0].Id;
            communityId2 = communities[1].Id;
            communityId3 = communities[2].Id;
        }

        if (!_dbContext.Users.Any())
        {
            string Password1 = "Password1";
            byte[] passwordHash1, passwordSalt1;
            PasswordHasher.CreatePasswordHash(Password1, out passwordHash1, out passwordSalt1);

            string Password2 = "Password2";
            byte[] passwordHash2, passwordSalt2;
            PasswordHasher.CreatePasswordHash(Password2, out passwordHash2, out passwordSalt2);

            string Password3 = "Password3";
            byte[] passwordHash3, passwordSalt3;
            PasswordHasher.CreatePasswordHash(Password3, out passwordHash3, out passwordSalt3);

            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    FullName = "Тирион Ланнистер",
                    BirthDate = DateTime.Parse("1990-12-12 18:39:46.861+07").ToUniversalTime(),
                    Gender = 0,
                    Email = "tirion@gmail.com",
                    PhoneNumber = "89244527680",
                    PasswordHash = passwordHash1,
                    PasswordSalt = passwordSalt1
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    FullName = "Джон Сноу",
                    BirthDate = DateTime.Parse("1991-02-12 12:39:46.861+07").ToUniversalTime(),
                    Gender = 0,
                    Email = "john@gmail.com",
                    PhoneNumber = "89516372470",
                    PasswordHash = passwordHash2,
                    PasswordSalt = passwordSalt2
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    FullName = "Джейме Ланнистер",
                    BirthDate = DateTime.Parse("1999-12-12 18:39:46.861+07").ToUniversalTime(),
                    Gender = 0,
                    Email = "jaiyme@gmail.com",
                    PhoneNumber = "89246560102",
                    PasswordHash = passwordHash3,
                    PasswordSalt = passwordSalt3
                }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            userId1 = users[0].Id;
            userId2 = users[1].Id;
            userId3 = users[2].Id;
        }

        if (!_dbContext.CommunityUsers.Any())
        {
            var communityUsers = new List<CommunityUser>();

            if (userId1 != Guid.Empty && communityId1 != Guid.Empty)
            {
                communityUsers.Add(new CommunityUser
                {
                    UserId = userId1,
                    CommunityId = communityId1,
                    Role = Role.Администратор
                });
            }

            if (userId2 != Guid.Empty && communityId2 != Guid.Empty)
            {
                communityUsers.Add(new CommunityUser
                {
                    UserId = userId2,
                    CommunityId = communityId2,
                    Role = Role.Администратор
                });
            }

            if (userId3 != Guid.Empty && communityId3 != Guid.Empty)
            {
                communityUsers.Add(new CommunityUser
                {
                    UserId = userId3,
                    CommunityId = communityId3,
                    Role = Role.Администратор
                });
            }

            await _dbContext.CommunityUsers.AddRangeAsync(communityUsers);
            await _dbContext.SaveChangesAsync();
        }

        if (!_dbContext.Tags.Any())
        {
            var tags = new List<Tag>
            {
                new Tag
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    Name = "личный блог"
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    Name = "18+"
                },
                new Tag
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    Name = "IT"
                }
            };

            await _dbContext.Tags.AddRangeAsync(tags);
        }

        await _dbContext.SaveChangesAsync();
    }
}