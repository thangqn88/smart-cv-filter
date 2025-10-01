using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using SmartCVFilter.Web.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartCVFilter.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        var baseUrl = _configuration["ApiSettings:BaseUrl"];
        _httpClient.BaseAddress = new Uri(baseUrl!);
        _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("ApiSettings:Timeout"));
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Calling backend API: {BaseUrl}/auth/login", _httpClient.BaseAddress);
            var response = await _httpClient.PostAsync("auth/login", content);

            _logger.LogInformation("Backend API response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Backend API response content: {Content}", responseContent);

                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

                if (authResponse != null)
                {
                    _logger.LogInformation("Authentication successful, setting token and signing in user");
                    SetToken(authResponse.Token);
                    await SignInUserAsync(authResponse.User);
                }

                return authResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Backend API error response: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return null;
        }
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

                if (authResponse != null)
                {
                    SetToken(authResponse.Token);
                    await SignInUserAsync(authResponse.User);
                }

                return authResponse;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync()
    {
        try
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsync("auth/validate", null);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        SetToken(string.Empty);
    }

    public async Task<T?> MakeRequestAsync<T>(string endpoint, HttpMethod method, object? content = null)
    {
        try
        {
            await EnsureAuthenticatedAsync();

            var request = new HttpRequestMessage(method, endpoint);

            if (content != null)
            {
                var json = JsonConvert.SerializeObject(content);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }

            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error making API request to {Endpoint}", endpoint);
            return default(T);
        }
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public string? GetToken()
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];
    }

    public void SetToken(string token)
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // Set to false for HTTP
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            response.Cookies.Append("auth_token", token, cookieOptions);
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task SignInUserAsync(UserInfo user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role ?? "User"),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
            new("CompanyName", user.CompanyName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
