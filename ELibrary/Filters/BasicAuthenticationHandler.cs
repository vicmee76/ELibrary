using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using static Newtonsoft.Json.JsonConvert;

namespace ELibrary.Filters
{

    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly List<OAuthUser> _users;
        private readonly ILogger<BasicAuthenticationHandler> _logger;
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration)
            : base(options, logger, encoder, clock) 
        {
            _users = [.. configuration.GetSection("OAuthUsers").GetChildren().Select(static s => new OAuthUser { Password = s.GetValue<string>("Password") ?? string.Empty, Username = s.GetValue<string>("Username") ?? string.Empty })];       
            _logger = logger.CreateLogger<BasicAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                var username = credentials[0];
                var password = credentials[1];

                var authenticatedUser = _users.FirstOrDefault(u => u.Username.ToLower() == username?.ToLower() && u.Password == password);

                if (authenticatedUser == null)
                {
                    _logger.LogWarning("Authentication failed for user {Username}", username);
                    return AuthenticateResult.Fail("Invalid Username or Password");
                }

                var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username),
            };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }

}
