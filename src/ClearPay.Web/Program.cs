using ClearPay.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SqlOptions>(builder.Configuration.GetSection(SqlOptions.SectionName));
builder.Services.AddProblemDetails();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", product = "ClearPay" }));

app.Run();

public partial class Program;
