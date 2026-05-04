using NewsletterOnion.Core._3_DomainModel;

namespace NewsletterOnion.Core._2_DomainServices
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetByEmailAsync(string email);
        Task AddAsync(Subscription subscription);

    }
}
