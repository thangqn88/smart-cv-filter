using Microsoft.AspNetCore.Identity;
using SmartCVFilter.API.Models;

namespace SmartCVFilter.API.Data;

public static class SeedData
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Create roles
        if (!await roleManager.RoleExistsAsync("Recruiter"))
        {
            await roleManager.CreateAsync(new IdentityRole("Recruiter"));
        }

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create default admin user
        var adminEmail = "admin@smartcvfilter.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                CompanyName = "Smart CV Filter",
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Create sample recruiter user
        var recruiterEmail = "recruiter@example.com";
        var recruiterUser = await userManager.FindByEmailAsync(recruiterEmail);
        if (recruiterUser == null)
        {
            recruiterUser = new ApplicationUser
            {
                UserName = recruiterEmail,
                Email = recruiterEmail,
                FirstName = "John",
                LastName = "Doe",
                CompanyName = "Tech Solutions Inc.",
                CreatedAt = DateTime.UtcNow
            };

            await userManager.CreateAsync(recruiterUser, "Recruiter123!");
            await userManager.AddToRoleAsync(recruiterUser, "Recruiter");
        }

        // Create sample job posts
        if (!context.JobPosts.Any())
        {
            var jobPosts = new List<JobPost>
            {
                new JobPost
                {
                    Title = "Senior .NET Developer",
                    Description = "We are looking for an experienced .NET developer to join our team. The ideal candidate will have strong experience with .NET 8, C#, and modern web development practices.",
                    Location = "Ho Chi Minh City",
                    Department = "Engineering",
                    EmploymentType = "Full-time",
                    ExperienceLevel = "Senior",
                    RequiredSkills = "C#, .NET 8, ASP.NET Core, Entity Framework, SQL Server, REST APIs",
                    PreferredSkills = "Vue.js, Docker, Azure, Microservices",
                    Responsibilities = "Develop and maintain web applications, Design and implement APIs, Collaborate with cross-functional teams",
                    Benefits = "Competitive salary, Health insurance, Flexible working hours, Professional development opportunities",
                    SalaryMin = 25000000,
                    SalaryMax = 40000000,
                    Status = "Active",
                    PostedDate = DateTime.UtcNow.AddDays(-5),
                    UserId = recruiterUser.Id
                },
                new JobPost
                {
                    Title = "Vue.js Frontend Engineer",
                    Description = "Join our frontend team to build modern, responsive web applications using Vue.js and related technologies.",
                    Location = "Hanoi",
                    Department = "Engineering",
                    EmploymentType = "Full-time",
                    ExperienceLevel = "Mid",
                    RequiredSkills = "Vue.js 3, JavaScript, TypeScript, HTML5, CSS3, REST APIs",
                    PreferredSkills = "Nuxt.js, Vite, Tailwind CSS, Jest, Cypress",
                    Responsibilities = "Build responsive web interfaces, Optimize application performance, Write clean and maintainable code",
                    Benefits = "Remote work options, Learning budget, Team building activities",
                    SalaryMin = 20000000,
                    SalaryMax = 30000000,
                    Status = "Active",
                    PostedDate = DateTime.UtcNow.AddDays(-3),
                    UserId = recruiterUser.Id
                }
            };

            context.JobPosts.AddRange(jobPosts);
            await context.SaveChangesAsync();

            // Create sample applicants
            var applicants = new List<Applicant>
            {
                new Applicant
                {
                    FirstName = "Nguyễn",
                    LastName = "Văn A",
                    Email = "vana@example.com",
                    PhoneNumber = "0123456789",
                    LinkedInProfile = "https://linkedin.com/in/vana",
                    CoverLetter = "I am excited to apply for the Senior .NET Developer position. With 5 years of experience in .NET development, I believe I would be a great fit for your team.",
                    JobPostId = jobPosts[0].Id,
                    AppliedDate = DateTime.UtcNow.AddDays(-2),
                    Status = "Applied"
                },
                new Applicant
                {
                    FirstName = "Trần",
                    LastName = "Thị B",
                    Email = "thib@example.com",
                    PhoneNumber = "0987654321",
                    LinkedInProfile = "https://linkedin.com/in/thib",
                    CoverLetter = "I am a passionate frontend developer with extensive experience in Vue.js and modern JavaScript frameworks.",
                    JobPostId = jobPosts[1].Id,
                    AppliedDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Applied"
                }
            };

            context.Applicants.AddRange(applicants);
            await context.SaveChangesAsync();
        }
    }
}

