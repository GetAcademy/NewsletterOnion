using NewsletterOnion.Core._1_ApplicationServices;
using NewsletterOnion.Core._2_DomainServices;
using NewsletterOnion.Core._3_DomainModel;
using Moq;

namespace NewsletterOnion.Test
{
    internal class NewsletterServiceTestMoq
    {
        public class NewsletterServiceTestManualMock
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

            [Test]
            public async Task TestAlreadyRegistered()
            {
                // arrange
                var mockRepo = new Mock<ISubscriptionRepository>();
                mockRepo.Setup(sr => sr.GetByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Subscription { IsConfirmed = true });
                var service = new NewsletterService(mockRepo.Object, null);

                // act
                var result = await service.SubscribeAsync("terje@getacademy.no");

                // assert
                Assert.That(!result.IsSuccess);
                Assert.That(result.Message, Is.EqualTo("Already subscribed"));
            }

            [Test]
            public async Task TestExistingUnconfirmed()
            {
                // arrange
                var mockRepo = new Mock<ISubscriptionRepository>();
                mockRepo.Setup(sr => sr.GetByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync(new Subscription { IsConfirmed = false });
                var mockEmail = new Mock<IEmailSender>();
                var service = new NewsletterService(mockRepo.Object, mockEmail.Object);

                // act
                var result = await service.SubscribeAsync("terje@getacademy.no");

                // assert
                Assert.That(result.IsSuccess);
                mockEmail.Verify(e=>e.Send(It.IsAny<Email>()), Times.Exactly(1));
                Assert.That(result.Message, Is.EqualTo("Confirmation email resent"));
            }

            [Test]
            public async Task TestNewSubscription()
            {
                // arrange
                var mockRepo = new Mock<ISubscriptionRepository>();
                mockRepo.Setup(sr => sr.GetByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((Subscription?)null);
                var mockEmail = new Mock<IEmailSender>();
                var service = new NewsletterService(mockRepo.Object, mockEmail.Object);

                // act
                var result = await service.SubscribeAsync("terje@getacademy.no");

                // assert
                Assert.That(result.IsSuccess);
                mockEmail.Verify(e => e.Send(It.IsAny<Email>()), Times.Exactly(1));
                mockRepo.Verify(sr => sr.AddAsync(It.IsAny<Subscription>()), Times.Exactly(1));
                Assert.That(result.Message, Is.EqualTo("Subscription created"));
            }
        }
    }
}
