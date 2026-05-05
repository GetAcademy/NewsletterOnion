using System.Text.Json;
using NewsletterOnion.Core._2_DomainServices;
using NewsletterOnion.Core._3_DomainModel;

namespace NewsletterOnion.API.Infrastructure;

public class JsonSubscriptionRepository : ISubscriptionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonSubscriptionRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<Subscription?> GetByEmailAsync(string email)
    {
        await _fileLock.WaitAsync();
        try
        {
            var subscriptions = await ReadAllAsync();
            return subscriptions.FirstOrDefault(subscription =>
                string.Equals(subscription.Email, email, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<Subscription?> GetByIdAsync(Guid id)
    {
        await _fileLock.WaitAsync();
        try
        {
            var subscriptions = await ReadAllAsync();
            return subscriptions.FirstOrDefault(subscription => subscription.Id == id);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddAsync(Subscription subscription)
    {
        await _fileLock.WaitAsync();
        try
        {
            var subscriptions = await ReadAllAsync();
            subscriptions.Add(subscription);
            await WriteAllAsync(subscriptions);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task UpdateAsync(Subscription subscription)
    {
        await _fileLock.WaitAsync();
        try
        {
            var subscriptions = await ReadAllAsync();
            var index = subscriptions.FindIndex(existing => existing.Id == subscription.Id);
            if (index >= 0)
            {
                subscriptions[index] = subscription;
            }
            else
            {
                subscriptions.Add(subscription);
            }

            await WriteAllAsync(subscriptions);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<Subscription>> ReadAllAsync()
    {
        EnsureDirectoryExists();

        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        var subscriptions = await JsonSerializer.DeserializeAsync<List<Subscription>>(stream, JsonOptions);
        return subscriptions ?? [];
    }

    private async Task WriteAllAsync(List<Subscription> subscriptions)
    {
        EnsureDirectoryExists();

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, subscriptions, JsonOptions);
    }

    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
