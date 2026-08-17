using ClearPay.Application;
using ClearPay.Infrastructure.Caching;
using ClearPay.Infrastructure.DependencyInjection;
using ClearPay.Infrastructure.Messaging;
using ClearPay.Infrastructure.Identity;
using ClearPay.Infrastructure.Persistence;
using ClearPay.Web;
using ClearPay.Application.Ports;
using ClearPay.Web.Localization;
using ClearPay.Web.OpenApi;
using ClearPay.Web.Realtime;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddClearPayAzureHosting(builder.Environment);
builder.Services.AddClearPayLocalization();
builder.Services.AddClearPayIdentity(builder.Configuration, builder.Environment);
builder.Services.AddClearPayExternalLogin(builder.Configuration);
builder.Services.AddClearPayJwt(builder.Configuration, builder.Environment);
builder.Services.AddClearPay(builder.Configuration);
builder.Services.AddClearPayHangfire(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/SetCulture");
    options.Conventions.AddPageRoute("/Account/Login", "/giris");
    options.Conventions.AddPageRoute("/Account/Register", "/kayit");
}).AddViewLocalization();
builder.Services.AddControllers();
builder.Services.AddClearPayCors(builder.Configuration, builder.Environment);
builder.Services.AddClearPaySwagger();
builder.Services.AddSignalR();
builder.Services.AddScoped<IWalletLiveNotifier, SignalRWalletLiveNotifier>();

var app = builder.Build();

app.UseClearPayForwardedHeaders();
await IdentitySeeder.EnsureCreatedAndRolesAsync(app.Services);
if (app.Configuration.GetValue("ClearPay:ApplyLedgerMigrations", true))
{
    await LedgerDatabase.EnsureMigratedAsync(app.Services, app.Logger);
    await DemoReceiptSeeder.EnsureExampleAsync(app.Services, app.Logger);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRequestLocalization();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(CorsExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapClearPaySwagger();
app.MapRazorPages();
app.MapControllers();
app.MapHub<WalletHub>(WalletHub.Path);
app.Services.MapClearPayHangfire(app.Configuration);
app.MapGet("/api/health", (RedisRuntimeStatus redis, RabbitRuntimeStatus rabbit) =>
    Results.Ok(new { status = "ok", product = "ClearPay", redis = redis.Value, rabbit = rabbit.Value }));

app.Run();

public partial class Program;
