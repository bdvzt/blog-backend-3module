namespace Email;

public interface IEmailSender
{
    public Task SendMessage(Message message);
}