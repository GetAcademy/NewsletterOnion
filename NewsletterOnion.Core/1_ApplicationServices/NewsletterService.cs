using NewsletterOnion.Core._2_DomainServices;
using NewsletterOnion.Core._3_DomainModel;

namespace NewsletterOnion.Core._1_ApplicationServices
{
    public class NewsletterService
    {
        private readonly ISubscriptionRepository _repo;
        private readonly IEmailSender _emailSender;
        private readonly string _baseUrl;

        public NewsletterService(
            ISubscriptionRepository repo, IEmailSender emailSender, string baseUrl = "http://localhost:5202")
        {
            _repo = repo;
            _emailSender = emailSender;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public async Task<Result> SubscribeAsync(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || !emailAddress.Contains("@"))
            {
                return Result.Fail("Invalid email");
            }

            var existingSubscription = await _repo.GetByEmailAsync(emailAddress);
            if (existingSubscription != null)
            {
                if (existingSubscription.IsConfirmed)
                {
                    return Result.Fail("Already subscribed");
                }

                await SendVerificationEmail(emailAddress, existingSubscription);
                return Result.Ok("Confirmation email resent");
            }

            var newSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Email = emailAddress,
                IsConfirmed = false
            };

            await _repo.AddAsync(newSubscription);
            await SendVerificationEmail(emailAddress, newSubscription);

            return Result.Ok("Subscription created");
        }

        public async Task<Result> VerifySubscriptionAsync(Guid code)
        {
            var subscription = await _repo.GetByIdAsync(code);
            if (subscription == null)
            {
                return Result.Fail("Verification code not found");
            }

            if (subscription.IsConfirmed)
            {
                return Result.Ok("Subscription already verified");
            }

            subscription.IsConfirmed = true;
            await _repo.UpdateAsync(subscription);

            return Result.Ok("Subscription verified");
        }

        private async Task SendVerificationEmail(string emailAddress, Subscription existing)
        {
            var verificationUrl = $"{_baseUrl}/verify.html?code={existing.Id}";
            var verificationLink = $"<a href=\"{verificationUrl}\">Verifiser!</a>";

            var email = new Email(emailAddress, "my-app@getacademy.no", "Verifiser abonnement", 
                $"Bla bla bla. {verificationLink}");
            await _emailSender.Send(email);
        }
    }
}
