using backend_3_module.Data;
using backend_3_module.Data.DTO.Comment;
using backend_3_module.Data.DTO.Post;
using backend_3_module.Data.DTO.Tag;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Errors;
using backend_3_module.Data.Queries;
using backend_3_module.Helpers;
using backend_3_module.Models.Address;
using backend_3_module.Services.IServices;
using Email;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = backend_3_module.Data.Errors.KeyNotFoundException;

namespace backend_3_module.Services;

public class PostService : IPostService
{
    private readonly BlogDbContext _dbContext;
    private readonly ICommunityService _communityService;
    private readonly AddressExsists _addressExsists;

    public PostService(BlogDbContext dbContext, ICommunityService communityService, AddressExsists addressExsists)
    {
        _dbContext = dbContext;
        _communityService = communityService;
        _addressExsists = addressExsists;
    }

    public async Task<PostAndPaginationDTO> GetAllPosts(AllPostFilters query, Guid? userId)
    {
        if (userId.HasValue && !await _dbContext.Users.AnyAsync(u => u.Id == userId.Value))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var posts = _dbContext.Posts
            .Include(p => p.Community)
            .Include(p => p.Community.CommunityUsers)
            .AsQueryable();

        if (!userId.HasValue)
        {
            posts = posts.Where(p =>
                p.CommunityId == null ||
                !p.Community.IsClosed);
        }
        else
        {
            posts = posts.Where(p =>
                p.CommunityId == null ||
                !p.Community.IsClosed ||
                p.Community.CommunityUsers.Any(cu => cu.UserId == userId.Value));
        }

        if (query.PageNumber <= 0)
            throw new BadRequestException("Номер страницы должен быть больше 0.");

        if (query.PageSize is <= 0 or > 100)
            throw new BadRequestException("Количество постов должно быть в диапазоне от 1 до 100.");

        if (!string.IsNullOrWhiteSpace(query.Author))
            posts = posts.Where(p => p.Author.Contains(query.Author));

        if (userId.HasValue && query.OnlyMyCommunities == true)
        {
            var communities = await _communityService.GetIdMyCommunity(userId);
            posts = posts.Where(p => communities.Contains(p.CommunityId ?? Guid.Empty));
        }

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

    public async Task CreatePost(CreatePostDTO postDto, Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException($"Пользователь с {userId} не найден.");

        var tags = await _dbContext.Tags
            .Where(t => postDto.Tags != null && postDto.Tags.Contains(t.Id))
            .ToListAsync();

        if (tags.Count != postDto.Tags.Count)
            throw new KeyNotFoundException("Теги не найдены");

        if (postDto.AddressId.HasValue && !await _addressExsists.AddressExistsAsync(postDto.AddressId.Value))
            throw new KeyNotFoundException($"Адрес с uuid {postDto.AddressId} не найден.");

        var newPost = new Post
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            Title = postDto.Title,
            Description = postDto.Description,
            ReadingTime = postDto.ReadingTime,
            Image = postDto.Image,
            AuthorId = userId,
            Author = user.FullName,
            CommunityId = null,
            CommunityName = null,
            AddressId = postDto.AddressId,
            User = user,
            Comments = new List<Comment>()
        };

        newPost.PostTags = tags.Select(tag => new PostTags
        {
            PostId = newPost.Id,
            TagId = tag.Id
        }).ToList();

        await _dbContext.Posts.AddAsync(newPost);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PostInfoDTO> GetPostInfo(Guid postId, Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException($"Пользователь с {userId} не найден.");

        var post = await _dbContext.Posts
            .Include(p => p.UserLikes)
            .Include(p => p.Comments)
            .Include(p => p.PostTags)!
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new KeyNotFoundException($"Пост с id {postId} не найден");

        var hasLike = post.UserLikes.Any(ul => ul.UserId == userId);

        var postInfo = new PostInfoDTO
        {
            Id = post.Id,
            CreateTime = post.CreateTime,
            Title = post.Title,
            Description = post.Description,
            ReadingTime = post.ReadingTime,
            Image = post.Image,
            AuthorId = post.AuthorId,
            Author = post.Author,
            CommunityId = post.CommunityId,
            CommunityName = post.CommunityName,
            AddressId = post.AddressId,
            Likes = post.UserLikes.Count,
            HasLike = hasLike,
            CommentsCount = post.Comments.Count,
            Comments = post.Comments
                .Where(c => c.ParentId == null)
                .Select(c => new CommentInfoDTO()
                {
                    Id = c.Id,
                    Content = c.DeleteDate == null ? c.Content : null,
                    CreateTime = c.CreateTime,
                    ModifiedDate = c.ModifiedDate,
                    DeleteDate = c.DeleteDate,
                    AuthorId = c.AuthorId,
                    Author = c.Author,
                    SubComments = c.SubComments
                }).ToList(),
        };
        if (post.PostTags != null)
        {
            postInfo.Tags = post.PostTags.Select(pt => new TagInfoDTO()
            {
                Id = pt.Tag.Id,
                Name = pt.Tag.Name,
                CreateTime = pt.Tag.CreateTime
            }).ToList();
        }

        return postInfo;
    }

    public async Task LikePost(Guid postId, Guid userId)
    {
        var post = await _dbContext.Posts
            .FirstOrDefaultAsync(c => c.Id == postId);

        if (post == null)
            throw new KeyNotFoundException($"Пост с id {postId} не найден");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(c => c.Id == userId);

        if (user == null)
            throw new KeyNotFoundException($"Пользователь с {userId} не найден.");

        var isPostLiked = await _dbContext.UserLikes
            .AnyAsync(ul => ul.UserId == userId && ul.PostId == postId);

        if (isPostLiked)
            throw new BadRequestException("Пользователь уже лайкнул этот пост");

        if (post.CommunityId != null)
        {
            var community = await _dbContext.Communities
                .Include(c => c.CommunityUsers)
                .FirstOrDefaultAsync(c => c.Id == post.CommunityId);

            if (community is { IsClosed: true })
            {
                var isSubscriberOrAdmin = community.CommunityUsers
                    .Any(cu => cu.UserId == userId && (cu.Role == Role.Подписчик || cu.Role == Role.Администратор));

                if (!isSubscriberOrAdmin)
                    throw new ForbiddenException(
                        "Поставить лайк посту из закрытой группы могут только подписчики или администраторы этого сообщества");
            }
        }

        var userLike = new UserLikes
        {
            UserId = userId,
            PostId = postId
        };

        await _dbContext.UserLikes.AddAsync(userLike);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UnlikePost(Guid postId, Guid userId)
    {
        var post = await _dbContext.Posts
            .FirstOrDefaultAsync(c => c.Id == postId);

        if (post == null)
            throw new KeyNotFoundException($"Пост с id {postId} не найден");

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(c => c.Id == userId);

        if (user == null)
            throw new KeyNotFoundException($"Пользователь с {userId} не найден.");

        var isPostLiked = await _dbContext.UserLikes
            .AnyAsync(ul => ul.UserId == userId && ul.PostId == postId);

        if (!isPostLiked)
            throw new BadRequestException("Пользователь не лайкал этот пост");

        if (post.CommunityId != null)
        {
            var community = await _dbContext.Communities
                .Include(c => c.CommunityUsers)
                .FirstOrDefaultAsync(c => c.Id == post.CommunityId);

            if (community is { IsClosed: true })
            {
                var isSubscriberOrAdmin = community.CommunityUsers
                    .Any(cu => cu.UserId == userId && (cu.Role == Role.Подписчик || cu.Role == Role.Администратор));

                if (!isSubscriberOrAdmin)
                    throw new ForbiddenException(
                        "Убрать лайк посту из закрытой группы могут только подписчики или администраторы этого сообщества");
            }
        }

        var userLike = await _dbContext.UserLikes
            .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.PostId == postId);

        if (userLike == null)
            throw new BadRequestException("Пользователь не лайкал этот пост");

        _dbContext.UserLikes.Remove(userLike);
        await _dbContext.SaveChangesAsync();
    }
}