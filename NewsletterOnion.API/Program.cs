using NewsletterOnion.Core._1_ApplicationServices;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();
app.MapPost("/subscribe", async (string email, NewsletterService service) =>
{
    var result = await service.SubscribeAsync(email);

    return result.IsSuccess
        ? Results.Ok(result.Message)
        : Results.BadRequest(result.Message);
});
app.Run();
