using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCVFilter.API.Data;
using SmartCVFilter.API.Models;
using System.Security.Claims;

namespace SmartCVFilter.API.Tests.TestBase;

public abstract class TestBaseClass
{
    protected ApplicationDbContext Context { get; private set; } = null!;
    protected UserManager<ApplicationUser> UserManager { get; private set; } = null!;
    protected RoleManager<IdentityRole> RoleManager { get; private set; } = null!;
    protected IConfiguration Configuration { get; private set; } = null!;
    protected Mock<ILogger<T>> GetMockLogger<T>() => new();

    protected virtual void Setup()
    {
        var services = new ServiceCollection();

        // Add in-memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Add configuration
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSecretKeyThatIsAtLeast32CharactersLong!",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["GeminiAI:ApiKey"] = "test-api-key"
            });
        Configuration = configurationBuilder.Build();
        services.AddSingleton(Configuration);

        var serviceProvider = services.BuildServiceProvider();

        Context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        Context.Database.EnsureCreated();
    }

    protected virtual async Task InitializeAsync()
    {
        // Create roles
        await CreateRolesAsync();
    }

    private async Task CreateRolesAsync()
    {
        var roles = new[] { "Recruiter", "Admin" };
        foreach (var role in roles)
        {
            if (!await RoleManager.RoleExistsAsync(role))
            {
                await RoleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    protected virtual void Cleanup()
    {
        Context?.Dispose();
    }

    protected async Task<ApplicationUser> CreateTestUserAsync(string email = "test@example.com", string role = "Recruiter")
    {
        // Ensure roles are created
        await InitializeAsync();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            CompanyName = "Test Company",
            CreatedAt = DateTime.UtcNow
        };

        var result = await UserManager.CreateAsync(user, "TestPassword123!");
        if (result.Succeeded)
        {
            await UserManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    protected async Task<JobPost> CreateTestJobPostAsync(string userId)
    {
        var jobPost = new JobPost
        {
            Title = "Test Job",
            Description = "Test Description",
            Location = "Test Location",
            Department = "Test Department",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid",
            RequiredSkills = "C#, .NET",
            PreferredSkills = "Vue.js",
            Responsibilities = "Test responsibilities",
            Benefits = "Test benefits",
            SalaryMin = 50000,
            SalaryMax = 70000,
            Status = "Active",
            PostedDate = DateTime.UtcNow,
            UserId = userId
        };

        Context.JobPosts.Add(jobPost);
        await Context.SaveChangesAsync();
        return jobPost;
    }

    protected async Task<Applicant> CreateTestApplicantAsync(int jobPostId)
    {
        var applicant = new Applicant
        {
            FirstName = "Test",
            LastName = "Applicant",
            Email = "applicant@example.com",
            PhoneNumber = "1234567890",
            JobPostId = jobPostId,
            AppliedDate = DateTime.UtcNow,
            Status = "Applied"
        };

        Context.Applicants.Add(applicant);
        await Context.SaveChangesAsync();
        return applicant;
    }

    protected ClaimsPrincipal CreateTestClaimsPrincipal(string userId, string email, string role = "Recruiter")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }
}
