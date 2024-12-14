using backend_3_module.Data;
using backend_3_module.Data.DTO.Comment;
using backend_3_module.Data.Entities;
using backend_3_module.Data.Errors;
using backend_3_module.Helpers;
using backend_3_module.Services.IServices;
using Microsoft.EntityFrameworkCore;
using KeyNotFoundException = backend_3_module.Data.Errors.KeyNotFoundException;

namespace backend_3_module.Services;

public class CommentService : ICommentService
{
    private readonly BlogDbContext _dbContext;

    public CommentService(BlogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CommentInfoDTO>> GetAllNestedComments(Guid commentId, Guid userId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var rootComment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId);
        if (rootComment == null)
            throw new KeyNotFoundException("Комментарий не найден");

        var post = await _dbContext.Posts
            .Include(p => p.Community)
            .ThenInclude(community => community.CommunityUsers)
            .FirstOrDefaultAsync(p => p.Id == rootComment.PostId);

        if (post == null)
            throw new KeyNotFoundException("Пост не найден");

        if (post.Community.IsClosed)
        {
            var isAdminOrSubscriber = post.Community.CommunityUsers.Any(cu => cu.UserId == userId);
            if (!isAdminOrSubscriber)
                throw new ForbiddenException(
                    "Писать комментарии могут только администраторы и подписчики данного сообщества");
        }

        var nestedComments = new List<CommentInfoDTO>();
        return await GetSubComments(nestedComments, commentId);
    }

    private async Task<List<CommentInfoDTO>> GetSubComments(List<CommentInfoDTO> nestedComments, Guid commentId)
    {
        var childComments = await _dbContext.Comments
            .Where(c => c.ParentId == commentId)
            .ToListAsync();

        var childCommentDtos = childComments.Select(c => new CommentInfoDTO
        {
            Id = c.Id,
            CreateTime = c.CreateTime,
            ParentId = c.ParentId,
            Content = c.DeleteDate == null ? c.Content : null,
            AuthorId = c.AuthorId,
            Author = c.Author,
            ModifiedDate = c.ModifiedDate,
            DeleteDate = c.DeleteDate,
            SubComments = c.SubComments
        }).ToList();

        nestedComments.AddRange(childCommentDtos);

        foreach (var childComment in childComments)
            await GetSubComments(nestedComments, childComment.Id);

        return nestedComments;
    }

    public async Task AddComment(Guid postId, Guid userId, CommentDTO commentDto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var post = await _dbContext.Posts
            .Include(p => p.Community).ThenInclude(community => community.CommunityUsers)
            .FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null)
            throw new KeyNotFoundException($"Пост с id {postId} не найден.");

        if (post.Community.IsClosed)
        {
            var isAdminOrSubscriber = post.Community.CommunityUsers.Any(cu => cu.UserId == userId);
            if (!isAdminOrSubscriber)
                throw new ForbiddenException(
                    "Писать комментарии могут только администраторы и подписчики данного сообщества");
        }

        if (commentDto.ParentId != null)
        {
            var parentComment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentDto.ParentId);
            if (parentComment == null)
                throw new KeyNotFoundException("Родительский комментарий не найден.");
            if (parentComment.PostId != postId)
                throw new BadRequestException("Родительский комментарий не принадлежит этому посту.");
        }

        var newComment = new Comment
        {
            Id = Guid.NewGuid(),
            CreateTime = DateTime.UtcNow,
            ParentId = commentDto.ParentId,
            Content = commentDto.Content,
            ModifiedDate = null,
            DeleteDate = null,
            AuthorId = userId,
            Author = user.FullName,
            SubComments = 0,
            PostId = postId
        };

        _dbContext.Comments.Add(newComment);

        if (commentDto.ParentId != null)
        {
            var parentComment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentDto.ParentId);
            if (parentComment != null)
                parentComment.SubComments += 1;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task EditComment(Guid commentId, Guid userId, CommentContentDTO commentContentDto)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var comment = await _dbContext.Comments
            .Include(c => c.Post)
            .ThenInclude(p => p.Community).ThenInclude(community => community.CommunityUsers)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new KeyNotFoundException("Комментарий не найден.");
        
        if (comment.Content == null)
            throw new BadRequestException("Нельзя редактировать удаленный комментарий.");

        if (comment.AuthorId != userId)
            throw new ForbiddenException(
                "Вы не можете редактировать этот комментарий, так как вы не являетесь его автором.");

        var post = comment.Post;
        if (post == null)
            throw new KeyNotFoundException("Пост, связанный с этим комментарием, не найден");

        var community = post.Community;
        if (community.IsClosed)
        {
            var isAdminOrSubscriber =
                post.Community.CommunityUsers.Any(cu => cu.CommunityId == community.Id && cu.UserId == userId);
            if (!isAdminOrSubscriber)
                throw new ForbiddenException(
                    "Писать комментарии могут только администраторы и подписчики данного сообщества");
        }

        comment.Content = commentContentDto.Content;
        comment.ModifiedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteComment(Guid commentId, Guid userId)
    {
        if (!await _dbContext.Users.AnyAsync(u => u.Id == userId))
            throw new KeyNotFoundException($"Пользователь с id {userId} не найден.");

        var comment = await _dbContext.Comments
            .Include(c => c.Post)
            .ThenInclude(p => p.Community).ThenInclude(community => community.CommunityUsers)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            throw new KeyNotFoundException("Комментарий не найден");
        
        if (comment.Content == null)
            throw new BadRequestException("Нельзя удалить удаленный комментарий.");

        if (comment.AuthorId != userId &&
            !comment.Post.Community.CommunityUsers.Any(cu => cu.UserId == userId && cu.Role == Role.Администратор))
            throw new ForbiddenException(
                "Вы не можете удалить этот комментарий, так как вы не являетесь его автором или администратором сообщества.");

        var post = comment.Post;
        if (post == null)
            throw new KeyNotFoundException("Пост, связанный с этим комментарием, не найден.");

        var hasChildComments = await _dbContext.Comments.AnyAsync(c => c.ParentId == commentId);

        if (hasChildComments)
        {
            comment.Content = null;
            comment.DeleteDate = DateTime.UtcNow;
        }
        else
        {
            _dbContext.Comments.Remove(comment);
            if (comment.ParentId != null)
            {
                var parentComment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == comment.ParentId);
                if (parentComment != null)
                    parentComment.SubComments -= 1;
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}