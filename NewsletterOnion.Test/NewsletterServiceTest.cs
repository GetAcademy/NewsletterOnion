using NewsletterOnion.Core._1_ApplicationServices;
using NewsletterOnion.Core._2_DomainServices;
using NewsletterOnion.Core._3_DomainModel;

namespace NewsletterOnion.Test
{
    public class NewsletterServiceTest
    {
        [Test]
        public async Task TestInvalidEmail()
        {
            // arrange
            var service = new NewsletterService(null, null);

            // act
            var result = await service.SubscribeAsync("terje");

            // assert
            Assert.That(!result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo("Invalid email"));
        }

        class SubscriptionRepositoryMock : ISubscriptionRepository
        {
            private bool _isConfirmed;

            public SubscriptionRepositoryMock(bool isConfirmed)
            {
                _isConfirmed = isConfirmed;
            }

            public Task<Subscription?> GetByEmailAsync(string email)
            {
                var subscription = new Subscription();
                subscription.IsConfirmed = _isConfirmed;
                return Task.FromResult(subscription)!;
            }

            public Task AddAsync(Subscription subscription)
            {
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task TestAlreadyRegistered()
        {
            // arrange
            var mockRepo = new SubscriptionRepositoryMock(true);
            var service = new NewsletterService(mockRepo, null);

            // act
            var result = await service.SubscribeAsync("terje@getacademy.no");

            // assert
            Assert.That(!result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo("Already subscribed"));
        }

        class EmailSenderMock : IEmailSender
        {
            public Task Send(Email email)
            {
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task TestExistingUnconfirmed()
        {
            // arrange
            var mockRepo = new SubscriptionRepositoryMock(false);
            var mockEmail = new EmailSenderMock();
            var service = new NewsletterService(mockRepo, mockEmail);

            // act
            var result = await service.SubscribeAsync("terje@getacademy.no");

            // assert
            Assert.That(result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo("Confirmation email resent"));
        }
    }
}
