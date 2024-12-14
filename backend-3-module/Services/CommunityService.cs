using backend_3_module.Data;
using backend_3_module.Data.DTO;
using backend_3_module.Data.DTO.Community;
using backend_3_module.Data.DTO.Post;
using backend_3_module.Data.DTO.Tag;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Errors;
using backend_3_module.Data.Queries;
using backend_3_module.Helpers;
using backend_3_module.Services.IServices;
using Email;
using Microsoft.EntityFrameworkCore;
using ForbiddenException = backend_3_module.Data.Errors.ForbiddenException;
using KeyNotFoundException = backend_3_module.Data.Errors.KeyNotFoundException;

namespace backend_3_module.Services;

public class CommunityService : ICommunityService
{
    private readonly BlogDbContext _dbContext;
    private readonly AddressExsists _addressExsists;
    private readonly IEmailSender _emailSender;

    public CommunityService(BlogDbContext dbContext, AddressExsists addressExsists, IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _addressExsists = addressExsists;
        _emailSender = emailSender;
    }

    public async Task<List<GetCommunityDTO>> GetCommunities()
    {
        var communities = await _dbContext.Communities
            .Include(c => c.CommunityUsers)
            .Select(c => new GetCommunityDTO
            {
                Id = c.Id,
                CreateTime = c.CreateTime,
                Name = c.Name,
                Description = c.Description,
                IsClosed = c.IsClosed,
                SubscribersCount = c.CommunityUsers.Count()
            })
            .ToListAsync();
        return communities;
    }

    public async Task CreateCommunity(CreateCommunityDTO createCommunityDto, Guid userId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        // if (await _dbContext.Communities.AnyAsync(c => c.Name == createCommunityDto.Name))
        //     throw new BadRequestException("Сообщество с таким именем уже существует.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var newCommunity = new Community
            {
                Name = createCommunityDto.Name,
                Description = createCommunityDto.Description,
                IsClosed = createCommunityDto.IsClosed,
                CreateTime = DateTime.UtcNow,
                Id = Guid.NewGuid()
            };

            var communityUser = new CommunityUser
            {
                UserId = userId,
                CommunityId = newCommunity.Id,
                Role = Role.Администратор
            };

            _dbContext.Communities.Add(newCommunity);
            _dbContext.CommunityUsers.Add(communityUser);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw new InternalServerErrorException("Ошибка на стороне сервера");
        }
    }

    public async Task<List<GetMyCommunitiesDTO>> GetMyCommunities(Guid userId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с {userId} не найден.");

        var communityUsers = await _dbContext.CommunityUsers
            .Where(cu => cu.UserId == userId)
            .Include(cu => cu.Community)
            .ToListAsync();
        var communities = communityUsers
            .Select(cu => new GetMyCommunitiesDTO
            {
                Id = cu.Community.Id,
                Name = cu.Community.Name,
                Role = cu.Role
            })
            .ToList();
        return communities;
    }

    public async Task<CommunityInfoDTO> GetCommunityInfo(Guid communityId)
    {
        var community = await _dbContext.Communities
            .Include(c => c.CommunityUsers)
            .ThenInclude(communityUser => communityUser.User)
            .FirstOrDefaultAsync(community => community.Id == communityId);
        if (community == null)
            throw new KeyNotFoundException($"Не нашлось группы с id {communityId}");

        var communityDto = new CommunityInfoDTO
        {
            Id = community.Id,
            CreateTime = community.CreateTime,
            Name = community.Name,
            Description = community.Description,
            IsClosed = community.IsClosed,
            SubscribersCount = community.CommunityUsers.Count(),
            Administrators = community.CommunityUsers
                .Where(cu => cu.Role == Role.Администратор)
                .Select(cu => new UserDTO()
                {
                    Id = cu.User.Id,
                    CreateTime = cu.User.CreateTime,
                    FullName = cu.User.FullName,
                    BirthDate = cu.User.BirthDate,
                    Gender = cu.User.Gender,
                    Email = cu.User.Email,
                    PhoneNumber = cu.User.PhoneNumber
                })
                .ToList()
        };
        return communityDto;
    }

    public async Task<PostAndPaginationDTO> GetCommunityPosts(CommunityPostFilters query, Guid communityId,
        Guid? userId)
    {
        if (userId.HasValue && !await _dbContext.Users.AnyAsync(u => u.Id == userId.Value))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var community = await _dbContext.Communities
            .Include(c => c.CommunityUsers)
            .FirstOrDefaultAsync(c => c.Id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Сообщество с id '{communityId}' не найдено.");

        if (community.IsClosed)
        {
            var isAdminOrSubscriber = community.CommunityUsers
                .Any(cu => cu.UserId == userId);
            if (!userId.HasValue || !isAdminOrSubscriber)
                throw new ForbiddenException(
                    "Просматривать посты могут только администратор и подписчики данного сообщества");
        }

        var posts = _dbContext.Posts.Where(p => p.CommunityId == communityId).AsQueryable();

        if (query.MinReadingTime.HasValue)
            posts = posts.Where(p => p.ReadingTime >= query.MinReadingTime.Value);

        if (query.MaxReadingTime.HasValue)
            posts = posts.Where(p => p.ReadingTime <= query.MaxReadingTime.Value);

        if (query.Tags != null && query.Tags.Any())
            posts = posts.Where(p => p.PostTags.Any(pt => query.Tags.Contains(pt.TagId)));

        if (query.Sorting.HasValue)
        {
            posts = query.Sorting switch
            {
                Sorting.CreateAsc => posts.OrderBy(p => p.CreateTime),
                Sorting.CreateDesc => posts.OrderByDescending(p => p.CreateTime),
                Sorting.LikeAsc => posts.OrderBy(p => p.UserLikes.Count),
                Sorting.LikeDesc => posts.OrderByDescending(p => p.UserLikes.Count),
                _ => posts
            };
        }
        else
        {
            posts = posts.OrderByDescending(p => p.CreateTime);
        }

        var totalCountAsync = Math.Ceiling((await posts.CountAsync() / (double)query.PageSize!));
        var skipNumber = (query.PageNumber - 1) * query.PageSize;
        posts = posts.Skip((int)skipNumber!).Take((int)query.PageSize!);

        if (totalCountAsync < query.PageNumber)
            throw new BadRequestException("Номер страницы не существует");

        var postDtOs = await posts
            .Select(p => new PostDTO
            {
                Id = p.Id,
                CreateTime = p.CreateTime,
                Title = p.Title,
                Description = p.Description,
                ReadingTime = p.ReadingTime,
                Image = p.Image,
                AuthorId = p.AuthorId,
                Author = p.Author,
                CommunityId = p.CommunityId,
                CommunityName = p.CommunityName,
                AddressId = p.AddressId,
                Likes = p.UserLikes.Count,
                HasLike = p.UserLikes.Any(ul => ul.UserId == userId),
                CommentsCount = p.Comments.Count,
                Tags = p.PostTags!.Select(pt => new TagInfoDTO
                {
                    Id = pt.Tag.Id,
                    Name = pt.Tag.Name,
                    CreateTime = pt.Tag.CreateTime
                }).ToList()
            })
            .ToListAsync();

        var pagination = new PageInfoDTO
        {
            Size = query.PageSize ?? 5,
            Count = totalCountAsync,
            CurrentPage = query.PageNumber ?? 1
        };

        return new PostAndPaginationDTO
        {
            Posts = postDtOs,
            PageInfo = pagination
        };
    }

    public async Task<RoleDTO> GetRole(Guid userId, Guid communityId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с {userId} не найден.");

        if (!await _dbContext.Communities.AnyAsync(с => с.Id == communityId))
            throw new KeyNotFoundException($"Сообщество с {communityId} не найдено.");

        var communityUser = await _dbContext.CommunityUsers
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CommunityId == communityId);

        if (communityUser == null)
            return new RoleDTO { Role = Role.НеПодписан };

        return new RoleDTO { Role = communityUser.Role };
    }

    public async Task SubscribeToCommunity(Guid userId, Guid communityId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var community = await _dbContext.Communities
            .FirstOrDefaultAsync(c => c.Id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Не нашлось группы с id {communityId}");

        var isSubscribed = await _dbContext.CommunityUsers
            .AnyAsync(cu => cu.UserId == userId && cu.CommunityId == communityId);

        if (isSubscribed)
            throw new BadRequestException("Пользователь уже в этом сообществе.");

        var communityUser = new CommunityUser
        {
            UserId = userId,
            CommunityId = communityId,
            Role = Role.Подписчик
        };

        await _dbContext.CommunityUsers.AddAsync(communityUser);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UnsubscribeFromCommunity(Guid userId, Guid communityId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var community = await _dbContext.Communities
            .Include(community => community.CommunityUsers)
            .FirstOrDefaultAsync(c => c.Id == communityId);

        if (community == null)
            throw new KeyNotFoundException($"Не нашлось группы с id {communityId}");

        var isSubscribed = await _dbContext.CommunityUsers
            .AnyAsync(cu => cu.UserId == userId && cu.CommunityId == communityId);

        if (!isSubscribed)
            throw new BadRequestException("Пользователь не подписан на это сообщество.");

        var isAdmin = community.CommunityUsers
            .Any(cu => cu.UserId == userId && cu.Role == Role.Администратор);

        if (isAdmin)
            throw new ForbiddenException("Администратор не может отписаться от своего сообщества.");

        var communityUser = await _dbContext.CommunityUsers
            .FirstOrDefaultAsync(cu => cu.UserId == userId && cu.CommunityId == communityId);

        if (communityUser != null)
            _dbContext.CommunityUsers.Remove(communityUser);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreatePost(Guid communityId, CreatePostDTO createPostDto, Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден");

        var community = await _dbContext.Communities
            .Include(c => c.CommunityUsers)
            .FirstOrDefaultAsync(c => c.Id == communityId);
        if (community == null)
            throw new KeyNotFoundException("Сообщество не найдено");

        var isAdmin = community.CommunityUsers
            .Any(cu => cu.UserId == userId && cu.Role == Role.Администратор);

        if (!isAdmin)
            throw new ForbiddenException("Создавать посты могут только администраторы данного сообщества");

        var tags = await _dbContext.Tags
            .Where(t => createPostDto.Tags.Contains(t.Id))
            .ToListAsync();

        if (tags.Count != createPostDto.Tags.Count)
            throw new KeyNotFoundException("Некоторые теги не найдены");

        if (createPostDto.AddressId.HasValue &&
            !await _addressExsists.AddressExistsAsync(createPostDto.AddressId.Value))
            throw new KeyNotFoundException($"Адрес с uuid {createPostDto.AddressId} не найден.");

        var subscribersEmails = await _dbContext.CommunityUsers
            .Where(cu => cu.CommunityId == communityId)
            .Select(cu => cu.User.Email)
            .ToListAsync();

        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Title = createPostDto.Title,
            Description = createPostDto.Description,
            ReadingTime = createPostDto.ReadingTime,
            Image = createPostDto.Image,
            AuthorId = userId,
            Author = user.FullName,
            CommunityId = communityId,
            CommunityName = community.Name,
            AddressId = createPostDto.AddressId,
            User = user,
            Comments = new List<Comment>()
        };

        newPost.PostTags = tags.Select(tag => new PostTags
        {
            PostId = newPost.Id,
            TagId = tag.Id
        }).ToList();

        await _dbContext.Posts.AddAsync(newPost);

        foreach (var email in subscribersEmails)
        {
            var newEmail = new EmailNewPost
            {
                To = email,
                Subject = $"Новый пост в {community.Name}",
                Content = $"Вышел новый пост в сообществе {community.Name}:" +
                          $"\n{createPostDto.Description}",
                Status = false
            };
            await _dbContext.EmailNewPosts.AddAsync(newEmail);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetIdMyCommunity(Guid? userId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с {userId} не найден.");

        return await _dbContext.CommunityUsers
            .Where(cu => cu.UserId == userId)
            .Select(cu => cu.CommunityId)
            .ToListAsync();
    }
}