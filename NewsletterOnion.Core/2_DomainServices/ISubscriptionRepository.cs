using NewsletterOnion.Core._3_DomainModel;

namespace NewsletterOnion.Core._2_DomainServices
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetByEmailAsync(string email);
        Task<Subscription?> GetByIdAsync(Guid id);
        Task AddAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);

    }
}
