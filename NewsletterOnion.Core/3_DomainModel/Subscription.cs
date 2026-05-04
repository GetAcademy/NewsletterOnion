namespace NewsletterOnion.Core._3_DomainModel
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public bool IsConfirmed { get; set; }
    }
}
