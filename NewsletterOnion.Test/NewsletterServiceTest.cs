using NewsletterOnion.Core._1_ApplicationServices;

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
            Assert.That(result.Message, !Is.Null, "Invalid email");
        }
    }
}
