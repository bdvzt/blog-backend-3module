using backend_3_module.Data.Entities;

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
                    SubscribersCount = 0,
                    CreateTime = DateTime.UtcNow
                },
                new Community
                {
                    Id = Guid.NewGuid(),
                    Name = "старый бог",
                    Description = "здесь",
                    IsClosed = false,
                    SubscribersCount = 0,
                    CreateTime = DateTime.UtcNow
                },
                new Community
                {
                    Id = Guid.NewGuid(),
                    Name = "пирожки с котятами",
                    Description = "люблю котят и бабушку",
                    IsClosed = true,
                    SubscribersCount = 0,
                    CreateTime = DateTime.UtcNow
                }
            };

            await _dbContext.Communities.AddRangeAsync(communities);
        }

        if (!_dbContext.Users.Any())
        {
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.UtcNow,
                    FullName = "HELP",
                    BirthDate = DateTime.UtcNow,
                    Gender = 0,
                    Email = "string",
                    Password = "string",
                    PhoneNumber = "89244527680"
                }
            };
            await _dbContext.Users.AddRangeAsync(users);
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