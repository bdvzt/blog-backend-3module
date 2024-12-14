using backend_3_module.Data;
using Email;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace backend_3_module.Jobs;

public class EmailJob : IJob
{
    private readonly BlogDbContext _blogDbContext;
    private readonly IEmailSender _emailSender;

    public EmailJob(BlogDbContext blogDbContext, IEmailSender emailSender)
    {
        _blogDbContext = blogDbContext;
        _emailSender = emailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var queue = await _blogDbContext.EmailNewPosts
            .Where(e => e.Status == false)
            .ToListAsync();

        foreach (var email in queue)
        {
            try
            {
                var message = new Message
                {
                    To = email.To,
                    Subject = email.Subject,
                    Content = email.Content
                };

                await _emailSender.SendMessage(message);

                email.Status = true;
                email.SentAt = DateTime.UtcNow;
            }
            catch
            {
                email.Status = false;
            }
        }

        await _blogDbContext.SaveChangesAsync();
    }
}