namespace NewsletterOnion.Core._3_DomainModel
{
    public class Email
    {
        public string To { get; }
        public string From { get; }
        public string Subject { get; }
        public string Text { get; }

        public Email(string to, string from, string subject, string text)
        {
            To = to;
            From = from;
            Subject = subject;
            Text = text;
        }
    }
}
