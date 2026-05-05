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
            private Subscription _subscription;
            public int SubscriptionAddedCount { get; private set; }

            public SubscriptionRepositoryMock(Subscription subscription)
            {
                _subscription = subscription;
            }

            public Task<Subscription?> GetByEmailAsync(string email)
            {
                return Task.FromResult(_subscription)!;
            }

            public Task AddAsync(Subscription subscription)
            {
                SubscriptionAddedCount++;
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task TestAlreadyRegistered()
        {
            // arrange
            var mockRepo = new SubscriptionRepositoryMock(new Subscription{IsConfirmed = true});
            var service = new NewsletterService(mockRepo, null);

            // act
            var result = await service.SubscribeAsync("terje@getacademy.no");

            // assert
            Assert.That(!result.IsSuccess);
            Assert.That(result.Message, Is.EqualTo("Already subscribed"));
        }

        class EmailSenderMock : IEmailSender
        {
            public int EmailSentCount { get; private set; }

            public Task Send(Email email)
            {
                EmailSentCount++;
                return Task.CompletedTask;
            }
        }

        [Test]
        public async Task TestExistingUnconfirmed()
        {
            // arrange
            var mockRepo = new SubscriptionRepositoryMock(new Subscription{IsConfirmed = false});
            var mockEmail = new EmailSenderMock();
            var service = new NewsletterService(mockRepo, mockEmail);

            // act
            var result = await service.SubscribeAsync("terje@getacademy.no");

            // assert
            Assert.That(result.IsSuccess);
            Assert.That(mockEmail.EmailSentCount, Is.EqualTo(1));
            Assert.That(result.Message, Is.EqualTo("Confirmation email resent"));
        }

        [Test]
        public async Task TestNewSubscription()
        {
            // arrange
            var mockRepo = new SubscriptionRepositoryMock(null);
            var mockEmail = new EmailSenderMock();
            var service = new NewsletterService(mockRepo, mockEmail);

            // act
            var result = await service.SubscribeAsync("terje@getacademy.no");

            // assert
            Assert.That(result.IsSuccess);
            Assert.That(mockEmail.EmailSentCount, Is.EqualTo(1));
            Assert.That(mockRepo.SubscriptionAddedCount, Is.EqualTo(1));
            Assert.That(result.Message, Is.EqualTo("Subscription created"));
        }
    }
}
