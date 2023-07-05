using API.Converters;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Insert(0, new DateOnlyJsonConverter());
                    //options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("https://localhost:4200"));

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

using IServiceScope scope = app.Services.CreateScope();
IServiceProvider services = scope.ServiceProvider;
try
{
    DataContext context = services.GetRequiredService<DataContext>();
    UserManager<AppUser> userManager = services.GetRequiredService<UserManager<AppUser>>();
    RoleManager<AppRole> roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    //SignInManager<AppRole> signInManager = services.GetRequiredService<SignInManager<AppRole>>();
    await context.Database.MigrateAsync();
    await Seed.ClearConnections(context);
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    ILogger<Program> logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");
}

app.Run();