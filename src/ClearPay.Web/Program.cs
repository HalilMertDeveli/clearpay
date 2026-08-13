using ClearPay.Application;
using ClearPay.Infrastructure.DependencyInjection;
using ClearPay.Infrastructure.Identity;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddClearPayIdentity(builder.Configuration, builder.Environment);
builder.Services.AddClearPay(builder.Configuration);
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AddPageRoute("/Account/Login", "/giris");
    options.Conventions.AddPageRoute("/Account/Register", "/kayit");
});
builder.Services.AddControllers();

var app = builder.Build();

await IdentitySeeder.EnsureCreatedAndRolesAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", product = "ClearPay" }));

app.Run();

public partial class Program;
