using Identity;
using Identity.Context;
using Identity.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddCors();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = "https://localhost:7206";
            options.Audience = "http://localhost:5019";
        });

        builder.Services.AddDbContext<UserDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("UserDbConnection"));

            options.UseOpenIddict();
        }
        );

        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<UserDbContext>();
            })
            .AddServer(options =>
            {
                options.DisableAccessTokenEncryption();

                options.UseAspNetCore()
                       .EnableTokenEndpointPassthrough()
                       .EnableAuthorizationEndpointPassthrough();

                options.AddSigningKey(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)));

                options.SetTokenEndpointUris("/connect/token");

                options.SetAuthorizationEndpointUris("/connect/authorize");

                options.AddDevelopmentSigningCertificate();

                options.AddEphemeralEncryptionKey();

                options.AllowClientCredentialsFlow();

                options.AllowAuthorizationCodeFlow();


            });
        builder.Services.Configure<HashingOptions>(builder.Configuration.GetSection(HashingOptions.SectionName));

        builder.Services.AddHostedService<Worker>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");

            app.UseHsts();
        }
        app.UseCors(o =>
        {
            o.AllowAnyMethod();
            o.AllowAnyOrigin();
        });
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=User}/{action=Index}/{id?}");

        app.Run();
    }
}