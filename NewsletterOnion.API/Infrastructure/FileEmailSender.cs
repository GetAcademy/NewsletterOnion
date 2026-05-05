using System.Text;
using NewsletterOnion.Core._2_DomainServices;
using NewsletterOnion.Core._3_DomainModel;

namespace NewsletterOnion.API.Infrastructure;

public class FileEmailSender : IEmailSender
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public FileEmailSender(string filePath)
    {
        _filePath = filePath;
    }

    public async Task Send(Email email)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var entry = new StringBuilder()
            .AppendLine($"SentAt: {DateTimeOffset.Now:O}")
            .AppendLine($"From: {email.From}")
            .AppendLine($"To: {email.To}")
            .AppendLine($"Subject: {email.Subject}")
            .AppendLine("Body:")
            .AppendLine(email.Text)
            .AppendLine(new string('-', 72))
            .ToString();

        await _fileLock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(_filePath, entry);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
