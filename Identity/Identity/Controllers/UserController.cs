using Identity.Context;
using Identity.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Identity.Controllers;
public class UserController : Controller
{
    protected readonly UserDbContext _userDbContext;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IConfiguration _configuration;
    private readonly HashingOptions _hashingOptions;

    public UserController(IOpenIddictApplicationManager applicationManager, UserDbContext userDbContext, IConfiguration configuration, IOptions<HashingOptions> hashingOptions)
    {
        _applicationManager = applicationManager;
        _userDbContext = userDbContext;
        _configuration = configuration;
        _hashingOptions = hashingOptions.Value;
    }
    public IActionResult Index()
    {
        Request.Query.TryGetValue("redirectUri", out var redirectUri);
        ViewBag.RedirectUri = redirectUri.ToString();
        return View();
    }

    [HttpGet("~/connect/authorize")]
    public async Task<IActionResult> Authorization([FromQuery] string client_id, [FromQuery] string redirect_uri)
    {
        HttpContext.Session.SetString("redirectUri", redirect_uri);
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("Server cannot be retrieved");

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId)
            ?? throw new InvalidOperationException("The application cannot be found.");

        if (application == null)
        {
            return BadRequest("No user with these credentials");
        }
        return RedirectToAction("Index");
    }


    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange(string username, string password)
    {
        if (!AuthenticateUser(username, password))
        {
            return View();
        }
        var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("OpenIddict request isn't available.");
        var identity = await CreateIdentity(request);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("/register")]
    public IActionResult Registration(string username, string password)
    {
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            if (_userDbContext.Users.FirstOrDefault(u => u.Username == username) == null)
            {
                string hashed = HashPassword(password);
                _userDbContext.Add(new User(username, hashed));
                _userDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
        }
        return RedirectToAction("Index");
    }

    private async Task<ClaimsIdentity> CreateIdentity(OpenIddictRequest request)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                   throw new InvalidOperationException("The application cannot be found.");
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);
        identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application));
        identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));
        identity.SetAudiences(_configuration["Jwt:Audience"]!);
        identity.SetDestinations(static _ => [Destinations.AccessToken]);
        return identity;
    }

    private bool AuthenticateUser(string username, string password)
    {
        string hashedPassword = HashPassword(password);

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            if (_userDbContext.Users.FirstOrDefault(u => u.Password == hashedPassword && u.Username == username) != null)
            {
                return true;
            }
        }
        return false;
    }

    private string HashPassword(string password)
    {
        byte[] saltByte = Convert.FromBase64String(_hashingOptions.Salt);

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: saltByte,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: _hashingOptions.IterationCount,
            numBytesRequested: 32));

        return hashed;
    }
}

