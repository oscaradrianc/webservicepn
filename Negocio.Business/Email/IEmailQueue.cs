namespace Negocio.Business.Email
{
    public interface IEmailQueue
    {
        void Queue(EmailMessage message);
    }
}