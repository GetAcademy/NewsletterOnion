using NewsletterOnion.Core._3_DomainModel;

namespace NewsletterOnion.Core._2_DomainServices
{
    public interface IEmailSender
    {
        Task Send(Email email);
    }
}
