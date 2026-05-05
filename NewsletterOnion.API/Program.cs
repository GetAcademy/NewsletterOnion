using NewsletterOnion.API.Infrastructure;
using NewsletterOnion.Core._1_ApplicationServices;
using NewsletterOnion.Core._2_DomainServices;

var builder = WebApplication.CreateBuilder(args);

var configuredStorageDirectory = builder.Configuration["Storage:Directory"];
var storageDirectory = string.IsNullOrWhiteSpace(configuredStorageDirectory)
    ? Path.Combine(builder.Environment.ContentRootPath, "data")
    : Path.IsPathRooted(configuredStorageDirectory)
        ? configuredStorageDirectory
        : Path.Combine(builder.Environment.ContentRootPath, configuredStorageDirectory);

var newsletterBaseUrl = builder.Configuration["Newsletter:BaseUrl"] ?? "http://localhost:5202";

builder.Services.AddSingleton<ISubscriptionRepository>(
    new JsonSubscriptionRepository(Path.Combine(storageDirectory, "subscriptions.json")));
builder.Services.AddSingleton<IEmailSender>(
    new FileEmailSender(Path.Combine(storageDirectory, "outbox.txt")));
builder.Services.AddScoped(serviceProvider => new NewsletterService(
    serviceProvider.GetRequiredService<ISubscriptionRepository>(),
    serviceProvider.GetRequiredService<IEmailSender>(),
    newsletterBaseUrl));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/subscribe.html"));

app.MapGet("/verify", async (Guid code, NewsletterService service) =>
{
    var result = await service.VerifySubscriptionAsync(code);

    return result.IsSuccess
        ? Results.Ok(new ApiMessage(result.Message))
        : Results.BadRequest(new ApiMessage(result.Message));
});

app.MapPost("/subscribe", async (HttpRequest request, NewsletterService service) =>
{
    var email = await ReadEmailFromRequest(request);
    var result = await service.SubscribeAsync(email);

    return result.IsSuccess
        ? Results.Ok(new ApiMessage(result.Message))
        : Results.BadRequest(new ApiMessage(result.Message));
});

app.Run();

static async Task<string> ReadEmailFromRequest(HttpRequest request)
{
    if (request.HasFormContentType)
    {
        var form = await request.ReadFormAsync();
        return form["email"].ToString();
    }

    var subscribeRequest = await request.ReadFromJsonAsync<SubscribeRequest>();
    return subscribeRequest?.Email ?? "";
}

public record SubscribeRequest(string Email);
public record ApiMessage(string Message);
