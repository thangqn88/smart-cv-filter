using Microsoft.AspNetCore.Identity;
using SmartCVFilter.API.Models;
using System.Text.Json;

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

        // Create multiple recruiter users
        var recruiters = await CreateRecruiters(userManager);

        // Create sample job posts
        if (!context.JobPosts.Any())
        {
            var jobPosts = CreateJobPosts(recruiters);
            context.JobPosts.AddRange(jobPosts);
            await context.SaveChangesAsync();

            // Create sample applicants
            var applicants = CreateApplicants(jobPosts);
            context.Applicants.AddRange(applicants);
            await context.SaveChangesAsync();

            // Create CV files for applicants
            var cvFiles = CreateCVFiles(applicants);
            context.CVFiles.AddRange(cvFiles);
            await context.SaveChangesAsync();

            // Create screening results
            var screeningResults = CreateScreeningResults(applicants, jobPosts);
            context.ScreeningResults.AddRange(screeningResults);
            await context.SaveChangesAsync();
        }
    }

    private static async Task<List<ApplicationUser>> CreateRecruiters(UserManager<ApplicationUser> userManager)
    {
        var recruiters = new List<ApplicationUser>();
        var recruiterData = new[]
        {
            new { Email = "recruiter@techsolutions.com", FirstName = "John", LastName = "Doe", Company = "Tech Solutions Inc.", Password = "Recruiter123!" },
            new { Email = "sarah@innovatecorp.com", FirstName = "Sarah", LastName = "Johnson", Company = "InnovateCorp", Password = "Recruiter123!" },
            new { Email = "mike@startupx.com", FirstName = "Mike", LastName = "Chen", Company = "StartupX", Password = "Recruiter123!" },
            new { Email = "lisa@globaltech.com", FirstName = "Lisa", LastName = "Williams", Company = "GlobalTech Solutions", Password = "Recruiter123!" },
            new { Email = "david@fintech.com", FirstName = "David", LastName = "Brown", Company = "FinTech Innovations", Password = "Recruiter123!" }
        };

        foreach (var data in recruiterData)
        {
            var user = await userManager.FindByEmailAsync(data.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = data.Email,
                    Email = data.Email,
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    CompanyName = data.Company,
                    CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
                };

                await userManager.CreateAsync(user, data.Password);
                await userManager.AddToRoleAsync(user, "Recruiter");
            }
            recruiters.Add(user);
        }

        return recruiters;
    }

    private static List<JobPost> CreateJobPosts(List<ApplicationUser> recruiters)
    {
        var jobPosts = new List<JobPost>
        {
            // Tech Solutions Inc. Jobs
            new JobPost
            {
                Title = "Senior .NET Developer",
                Description = "We are looking for an experienced .NET developer to join our team. The ideal candidate will have strong experience with .NET 8, C#, and modern web development practices.",
                Location = "Ho Chi Minh City",
                Department = "Engineering",
                EmploymentType = "Full-time",
                ExperienceLevel = "Senior",
                RequiredSkills = "C#, .NET 8, ASP.NET Core, Entity Framework, SQL Server, REST APIs, Microservices, Git, Docker, Azure",
                PreferredSkills = "Vue.js, Kubernetes, Redis, RabbitMQ, AWS, CI/CD, TypeScript, React",
                Responsibilities = "Develop and maintain web applications, Design and implement APIs, Collaborate with cross-functional teams, Mentor junior developers, Lead technical discussions, Code reviews, Performance optimization",
                Benefits = "Competitive salary, Health insurance, Flexible working hours, Professional development opportunities, Stock options, Remote work, Learning budget",
                SalaryMin = 25000000,
                SalaryMax = 40000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-5),
                ClosingDate = DateTime.UtcNow.AddDays(25),
                UserId = recruiters[0].Id
            },
            new JobPost
            {
                Title = "Vue.js Frontend Engineer",
                Description = "Join our frontend team to build modern, responsive web applications using Vue.js and related technologies. We're looking for someone passionate about creating exceptional user experiences.",
                Location = "Hanoi",
                Department = "Engineering",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "Vue.js 3, JavaScript, TypeScript, HTML5, CSS3, REST APIs, Git, Responsive Design",
                PreferredSkills = "Nuxt.js, Vite, Tailwind CSS, Jest, Cypress, Webpack, PWA, Node.js",
                Responsibilities = "Build responsive web interfaces, Optimize application performance, Write clean and maintainable code, Collaborate with UX/UI designers, Implement testing strategies",
                Benefits = "Remote work options, Learning budget, Team building activities, Flexible schedule, Health insurance, Competitive salary",
                SalaryMin = 20000000,
                SalaryMax = 30000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-3),
                ClosingDate = DateTime.UtcNow.AddDays(27),
                UserId = recruiters[0].Id
            },
            new JobPost
            {
                Title = "DevOps Engineer",
                Description = "We need a skilled DevOps engineer to help us scale our infrastructure and improve our deployment processes. You'll work with cutting-edge cloud technologies and automation tools.",
                Location = "Da Nang",
                Department = "Engineering",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "Docker, Kubernetes, AWS/Azure, CI/CD, Linux, Bash scripting, Terraform, Git, Monitoring",
                PreferredSkills = "Ansible, Jenkins, GitLab CI, Prometheus, Grafana, ELK Stack, Python, Infrastructure as Code",
                Responsibilities = "Manage cloud infrastructure, Automate deployment processes, Monitor system performance, Ensure security best practices, Troubleshoot issues, Scale applications",
                Benefits = "Competitive salary, Health insurance, Remote work, Learning budget, Conference attendance, Flexible hours",
                SalaryMin = 22000000,
                SalaryMax = 35000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-7),
                ClosingDate = DateTime.UtcNow.AddDays(23),
                UserId = recruiters[0].Id
            },

            // InnovateCorp Jobs
            new JobPost
            {
                Title = "Product Manager",
                Description = "Lead product development initiatives and work closely with engineering teams to deliver exceptional products. We're looking for someone with strong analytical skills and user-centric thinking.",
                Location = "Ho Chi Minh City",
                Department = "Product",
                EmploymentType = "Full-time",
                ExperienceLevel = "Senior",
                RequiredSkills = "Product Management, Agile/Scrum, Data Analysis, User Research, Stakeholder Management, Roadmap Planning",
                PreferredSkills = "Figma, SQL, A/B Testing, Growth Hacking, Technical Background, Analytics Tools, User Testing",
                Responsibilities = "Define product roadmap, Conduct user research, Collaborate with engineering teams, Analyze product metrics, Manage stakeholders, Drive product strategy",
                Benefits = "Competitive salary, Health insurance, Flexible hours, Stock options, Professional development, Remote work, Learning budget",
                SalaryMin = 30000000,
                SalaryMax = 45000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-4),
                ClosingDate = DateTime.UtcNow.AddDays(26),
                UserId = recruiters[1].Id
            },
            new JobPost
            {
                Title = "UX/UI Designer",
                Description = "Create beautiful and intuitive user interfaces for our digital products. We're looking for a creative designer who understands user needs and can translate them into compelling designs.",
                Location = "Hanoi",
                Department = "Design",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "Figma, Adobe Creative Suite, User Research, Prototyping, Design Systems, Wireframing, User Testing",
                PreferredSkills = "Sketch, Principle, After Effects, HTML/CSS, User Testing, Design Thinking, Accessibility",
                Responsibilities = "Design user interfaces, Conduct user research, Create prototypes, Collaborate with development teams, Maintain design systems, User testing",
                Benefits = "Creative freedom, Health insurance, Flexible schedule, Design tools budget, Team events, Learning opportunities, Remote work",
                SalaryMin = 18000000,
                SalaryMax = 28000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-6),
                ClosingDate = DateTime.UtcNow.AddDays(24),
                UserId = recruiters[1].Id
            },
            new JobPost
            {
                Title = "Data Scientist",
                Description = "Join our data team to extract insights from large datasets and build machine learning models. You'll work on exciting projects that impact millions of users.",
                Location = "Ho Chi Minh City",
                Department = "Data Science",
                EmploymentType = "Full-time",
                ExperienceLevel = "Senior",
                RequiredSkills = "Python, R, Machine Learning, SQL, Statistics, Data Visualization, Pandas, NumPy, Scikit-learn",
                PreferredSkills = "TensorFlow, PyTorch, Spark, AWS SageMaker, Tableau, A/B Testing, Deep Learning, Big Data",
                Responsibilities = "Build ML models, Analyze data patterns, Create data visualizations, Present insights to stakeholders, Data preprocessing, Model deployment",
                Benefits = "Competitive salary, Health insurance, Learning budget, Conference attendance, Flexible hours, Remote work, Stock options",
                SalaryMin = 28000000,
                SalaryMax = 42000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-2),
                ClosingDate = DateTime.UtcNow.AddDays(28),
                UserId = recruiters[1].Id
            },

            // StartupX Jobs
            new JobPost
            {
                Title = "Full Stack Developer",
                Description = "Join our fast-growing startup as a full-stack developer. You'll work on both frontend and backend development, with opportunities to learn new technologies and take on leadership roles.",
                Location = "Ho Chi Minh City",
                Department = "Engineering",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "React, Node.js, JavaScript, TypeScript, MongoDB, Express.js, Git, REST APIs, HTML/CSS",
                PreferredSkills = "Next.js, GraphQL, AWS, Docker, Redis, Jest, PostgreSQL, Microservices, CI/CD",
                Responsibilities = "Develop full-stack applications, Write clean and maintainable code, Collaborate with product team, Participate in code reviews, Debug issues",
                Benefits = "Equity participation, Health insurance, Flexible hours, Learning budget, Startup experience, Remote work, Growth opportunities",
                SalaryMin = 15000000,
                SalaryMax = 25000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-8),
                ClosingDate = DateTime.UtcNow.AddDays(22),
                UserId = recruiters[2].Id
            },
            new JobPost
            {
                Title = "Marketing Manager",
                Description = "Lead our marketing efforts and help us grow our user base. We're looking for someone creative and data-driven who can develop and execute marketing strategies.",
                Location = "Hanoi",
                Department = "Marketing",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "Digital Marketing, Social Media, Content Marketing, Analytics, Campaign Management, SEO, Email Marketing",
                PreferredSkills = "Google Ads, Facebook Ads, Growth Hacking, Marketing Automation, A/B Testing, Brand Management, Lead Generation",
                Responsibilities = "Develop marketing strategies, Manage social media presence, Create content campaigns, Analyze marketing metrics, Lead generation, Brand management",
                Benefits = "Competitive salary, Health insurance, Flexible hours, Marketing budget, Creative freedom, Learning opportunities, Remote work",
                SalaryMin = 16000000,
                SalaryMax = 24000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-9),
                ClosingDate = DateTime.UtcNow.AddDays(21),
                UserId = recruiters[2].Id
            },

            // GlobalTech Solutions Jobs
            new JobPost
            {
                Title = "Senior Java Developer",
                Description = "We're looking for an experienced Java developer to join our enterprise development team. You'll work on large-scale applications and have opportunities to mentor junior developers.",
                Location = "Da Nang",
                Department = "Engineering",
                EmploymentType = "Full-time",
                ExperienceLevel = "Senior",
                RequiredSkills = "Java, Spring Boot, Spring Security, Hibernate, MySQL, REST APIs, Maven, Git, JUnit",
                PreferredSkills = "Microservices, Docker, Kubernetes, Redis, Apache Kafka, Elasticsearch, AWS, Design Patterns",
                Responsibilities = "Develop enterprise applications, Design system architecture, Mentor junior developers, Code reviews, Performance optimization, Technical leadership",
                Benefits = "Competitive salary, Health insurance, Professional development, Flexible hours, Team building, Learning budget, Remote work",
                SalaryMin = 26000000,
                SalaryMax = 38000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-10),
                ClosingDate = DateTime.UtcNow.AddDays(20),
                UserId = recruiters[3].Id
            },
            new JobPost
            {
                Title = "QA Engineer",
                Description = "Ensure the quality of our software products through comprehensive testing strategies. We're looking for someone detail-oriented with strong technical skills.",
                Location = "Ho Chi Minh City",
                Department = "Quality Assurance",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "Manual Testing, Automated Testing, Selenium, Test Planning, Bug Tracking, Test Cases, Regression Testing",
                PreferredSkills = "Cypress, Postman, API Testing, Performance Testing, Load Testing, Test Automation, CI/CD, Agile",
                Responsibilities = "Design test cases, Execute test plans, Report bugs, Collaborate with development teams, Test automation, Quality assurance",
                Benefits = "Competitive salary, Health insurance, Learning opportunities, Flexible schedule, Professional development, Remote work, Team events",
                SalaryMin = 14000000,
                SalaryMax = 22000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-11),
                ClosingDate = DateTime.UtcNow.AddDays(19),
                UserId = recruiters[3].Id
            },

            // FinTech Innovations Jobs
            new JobPost
            {
                Title = "Blockchain Developer",
                Description = "Join our blockchain team to develop innovative financial solutions. You'll work with cutting-edge blockchain technologies and contribute to the future of finance.",
                Location = "Ho Chi Minh City",
                Department = "Engineering",
                EmploymentType = "Full-time",
                ExperienceLevel = "Senior",
                RequiredSkills = "Solidity, Ethereum, Web3.js, Smart Contracts, Node.js, JavaScript, Git, Blockchain, Cryptography",
                PreferredSkills = "Rust, Substrate, Polkadot, DeFi, IPFS, Hardhat, Truffle, MetaMask, Wallet Integration",
                Responsibilities = "Develop smart contracts, Build blockchain applications, Research new technologies, Collaborate with product team, Security audits",
                Benefits = "Competitive salary, Health insurance, Stock options, Learning budget, Conference attendance, Remote work, Cutting-edge technology",
                SalaryMin = 32000000,
                SalaryMax = 48000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-12),
                ClosingDate = DateTime.UtcNow.AddDays(18),
                UserId = recruiters[4].Id
            },
            new JobPost
            {
                Title = "Financial Analyst",
                Description = "Analyze financial data and provide insights to support business decisions. We're looking for someone with strong analytical skills and financial knowledge.",
                Location = "Hanoi",
                Department = "Finance",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "Financial Analysis, Excel, SQL, Data Analysis, Financial Modeling, Risk Assessment, Budgeting",
                PreferredSkills = "Python, R, Tableau, Power BI, CFA certification, VBA, Statistical Analysis, Forecasting",
                Responsibilities = "Analyze financial data, Create financial models, Prepare reports, Support business decisions, Risk assessment, Budget planning",
                Benefits = "Competitive salary, Health insurance, Professional development, Flexible hours, Learning opportunities, Career growth, Remote work",
                SalaryMin = 20000000,
                SalaryMax = 30000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-13),
                ClosingDate = DateTime.UtcNow.AddDays(17),
                UserId = recruiters[4].Id
            },

            // Additional diverse jobs
            new JobPost
            {
                Title = "Mobile App Developer (React Native)",
                Description = "Develop cross-platform mobile applications using React Native. You'll work on both iOS and Android platforms and have opportunities to learn native development.",
                Location = "Ho Chi Minh City",
                Department = "Engineering",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "React Native, JavaScript, TypeScript, Redux, REST APIs, Git, Mobile Development, iOS/Android",
                PreferredSkills = "iOS Development, Android Development, Firebase, Push Notifications, App Store, Play Store, Native Modules",
                Responsibilities = "Develop mobile applications, Debug and fix issues, Collaborate with design team, Optimize app performance, App store deployment",
                Benefits = "Competitive salary, Health insurance, Flexible hours, Learning budget, Device allowance, Remote work, Creative freedom",
                SalaryMin = 18000000,
                SalaryMax = 28000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-14),
                ClosingDate = DateTime.UtcNow.AddDays(16),
                UserId = recruiters[0].Id
            },
            new JobPost
            {
                Title = "Cybersecurity Specialist",
                Description = "Protect our systems and data from cyber threats. You'll work on security assessments, incident response, and security architecture design.",
                Location = "Da Nang",
                Department = "Security",
                EmploymentType = "Full-time",
                ExperienceLevel = "Senior",
                RequiredSkills = "Cybersecurity, Network Security, Penetration Testing, Incident Response, Risk Assessment, Security Tools",
                PreferredSkills = "CISSP, CEH, Security+, SIEM, Forensics, Cloud Security, Vulnerability Assessment, Security Architecture",
                Responsibilities = "Conduct security assessments, Respond to security incidents, Design security policies, Train staff on security, Risk management",
                Benefits = "Competitive salary, Health insurance, Professional development, Flexible hours, Security certifications, Remote work, Learning budget",
                SalaryMin = 30000000,
                SalaryMax = 45000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-15),
                ClosingDate = DateTime.UtcNow.AddDays(15),
                UserId = recruiters[3].Id
            },
            new JobPost
            {
                Title = "Technical Writer",
                Description = "Create clear and comprehensive technical documentation for our products and APIs. We're looking for someone who can translate complex technical concepts into user-friendly content.",
                Location = "Hanoi",
                Department = "Documentation",
                EmploymentType = "Full-time",
                ExperienceLevel = "Mid",
                RequiredSkills = "Technical Writing, API Documentation, Markdown, Git, Software Development Knowledge, Documentation Tools",
                PreferredSkills = "Swagger, Postman, Confluence, Jira, Video Creation, User Guides, Tutorials, Content Management",
                Responsibilities = "Write technical documentation, Create API guides, Maintain documentation, Collaborate with development teams, User guides",
                Benefits = "Competitive salary, Health insurance, Flexible hours, Learning opportunities, Remote work options, Creative freedom, Professional development",
                SalaryMin = 12000000,
                SalaryMax = 20000000,
                Status = "Active",
                PostedDate = DateTime.UtcNow.AddDays(-16),
                ClosingDate = DateTime.UtcNow.AddDays(14),
                UserId = recruiters[1].Id
            }
        };

        return jobPosts;
    }

    private static List<Applicant> CreateApplicants(List<JobPost> jobPosts)
    {
        var applicants = new List<Applicant>();
        var random = new Random();

        // Vietnamese names for display (with accents)
        var firstNames = new[] { "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ", "Hồ", "Ngô", "Dương", "Lý" };
        var lastNames = new[] { "Văn", "Thị", "Minh", "Hồng", "Thanh", "Thu", "Linh", "Anh", "Hương", "Lan", "Phương", "Hạnh", "Nga", "Tuyết", "Mai" };
        var middleNames = new[] { "Văn", "Thị", "Minh", "Hồng", "Thanh", "Thu", "Linh", "Anh", "Hương", "Lan", "Phương", "Hạnh", "Nga", "Tuyết", "Mai" };

        // Latin versions for email and LinkedIn (without accents)
        var firstNamesLatin = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Phan", "Vu", "Vo", "Dang", "Bui", "Do", "Ho", "Ngo", "Duong", "Ly" };
        var lastNamesLatin = new[] { "Van", "Thi", "Minh", "Hong", "Thanh", "Thu", "Linh", "Anh", "Huong", "Lan", "Phuong", "Hanh", "Nga", "Tuyet", "Mai" };
        var domains = new[] { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "company.com", "tech.com", "dev.com" };
        var statuses = new[] { "Applied", "Under Review", "Shortlisted", "Rejected", "Hired" };
        var linkedInProfiles = new[] { "linkedin.com/in/", "linkedin.com/in/", "linkedin.com/in/" };

        // Create 3-8 applicants per job post
        foreach (var jobPost in jobPosts)
        {
            var applicantCount = random.Next(3, 9);
            for (int i = 0; i < applicantCount; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var middleName = middleNames[random.Next(middleNames.Length)];

                // Use Latin versions for email and LinkedIn
                var firstNameLatin = firstNamesLatin[random.Next(firstNamesLatin.Length)];
                var lastNameLatin = lastNamesLatin[random.Next(lastNamesLatin.Length)];

                var email = $"{firstNameLatin.ToLower()}{lastNameLatin.ToLower()}{i}{random.Next(100)}@{domains[random.Next(domains.Length)]}";
                var phoneNumber = $"0{random.Next(100000000, 999999999)}";
                var linkedInProfile = $"{linkedInProfiles[random.Next(linkedInProfiles.Length)]}{firstNameLatin.ToLower()}{lastNameLatin.ToLower()}{i}";
                var status = statuses[random.Next(statuses.Length)];
                var appliedDate = DateTime.UtcNow.AddDays(-random.Next(1, 30));

                var coverLetters = new[]
                {
                    $"I am excited to apply for the {jobPost.Title} position. With my experience in {jobPost.RequiredSkills.Split(',')[0]}, I believe I would be a great fit for your team.",
                    $"I am passionate about {jobPost.Department.ToLower()} and would love to contribute to your company's success. My background in {jobPost.RequiredSkills.Split(',')[1]} makes me an ideal candidate.",
                    $"I have been following your company's growth and am impressed by your innovative approach. I would be honored to join your team as a {jobPost.Title}.",
                    $"With my strong background in {jobPost.RequiredSkills.Split(',')[0]} and passion for technology, I am confident I can make a valuable contribution to your team.",
                    $"I am writing to express my interest in the {jobPost.Title} position. My experience in {jobPost.Department.ToLower()} and technical skills align perfectly with your requirements."
                };

                var coverLetter = TruncateToLength(coverLetters[random.Next(coverLetters.Length)], 1000);

                applicants.Add(new Applicant
                {
                    FirstName = firstName,
                    LastName = $"{middleName} {lastName}",
                    Email = email,
                    PhoneNumber = phoneNumber,
                    LinkedInProfile = linkedInProfile,
                    PortfolioUrl = random.Next(2) == 0 ? $"https://portfolio.{firstNameLatin.ToLower()}{lastNameLatin.ToLower()}.com" : null,
                    CoverLetter = coverLetter,
                    JobPostId = jobPost.Id,
                    AppliedDate = appliedDate,
                    Status = status,
                    LastUpdated = status != "Applied" ? appliedDate.AddDays(random.Next(1, 10)) : null
                });
            }
        }

        return applicants;
    }

    private static List<CVFile> CreateCVFiles(List<Applicant> applicants)
    {
        var cvFiles = new List<CVFile>();
        var random = new Random();
        var fileExtensions = new[] { "pdf", "doc", "docx" };
        var contentTypes = new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        var statuses = new[] { "Uploaded", "Processing", "Processed" };

        foreach (var applicant in applicants)
        {
            // Each applicant has 1-3 CV files
            var fileCount = random.Next(1, 4);
            for (int i = 0; i < fileCount; i++)
            {
                var extension = fileExtensions[random.Next(fileExtensions.Length)];
                var contentType = contentTypes[random.Next(contentTypes.Length)];
                // Convert names to Latin for file names
                var firstNameLatin = ConvertToLatin(applicant.FirstName);
                var lastNameLatin = ConvertToLatin(applicant.LastName);
                var fileName = $"CV_{firstNameLatin}_{lastNameLatin}_{i + 1}.{extension}";
                var filePath = $"/uploads/cv/{applicant.Id}/{fileName}";
                var fileSize = random.Next(500000, 5000000); // 500KB to 5MB
                var status = statuses[random.Next(statuses.Length)];

                // Sample extracted text for processed files
                var extractedText = status == "Processed" ? TruncateToLength(GenerateSampleCVText(applicant), 1000) : null;

                cvFiles.Add(new CVFile
                {
                    FileName = fileName,
                    FilePath = filePath,
                    ContentType = contentType,
                    FileSize = fileSize,
                    FileExtension = extension,
                    ExtractedText = extractedText,
                    UploadedDate = applicant.AppliedDate.AddHours(random.Next(1, 24)),
                    Status = status,
                    ApplicantId = applicant.Id
                });
            }
        }

        return cvFiles;
    }

    private static List<ScreeningResult> CreateScreeningResults(List<Applicant> applicants, List<JobPost> jobPosts)
    {
        var screeningResults = new List<ScreeningResult>();
        var random = new Random();

        // Create screening results for about 60% of applicants
        var applicantsWithScreening = applicants.Where(a => random.Next(10) < 6).ToList();

        foreach (var applicant in applicantsWithScreening)
        {
            var jobPost = jobPosts.First(jp => jp.Id == applicant.JobPostId);
            var overallScore = random.Next(40, 95);
            var status = overallScore > 80 ? "Completed" : overallScore > 60 ? "Completed" : "Completed";
            var completedAt = applicant.AppliedDate.AddDays(random.Next(1, 7));

            var strengths = GenerateStrengths(overallScore);
            var weaknesses = GenerateWeaknesses(overallScore);
            var summary = TruncateToLength(GenerateSummary(applicant, jobPost, overallScore), 2000);
            var detailedAnalysis = TruncateToLength(GenerateDetailedAnalysis(applicant, jobPost, overallScore), 2000);

            screeningResults.Add(new ScreeningResult
            {
                ApplicantId = applicant.Id,
                JobPostId = jobPost.Id,
                OverallScore = overallScore,
                Summary = summary,
                Strengths = JsonSerializer.Serialize(strengths),
                Weaknesses = JsonSerializer.Serialize(weaknesses),
                DetailedAnalysis = detailedAnalysis,
                Status = status,
                CreatedAt = applicant.AppliedDate.AddDays(random.Next(1, 3)),
                CompletedAt = completedAt,
                ErrorMessage = status == "Failed" ? "AI processing error occurred" : null
            });
        }

        return screeningResults;
    }

    private static string GenerateSampleCVText(Applicant applicant)
    {
        return $@"
CURRICULUM VITAE
{applicant.FirstName} {applicant.LastName}
Email: {applicant.Email}
Phone: {applicant.PhoneNumber}
LinkedIn: {applicant.LinkedInProfile}

PROFESSIONAL SUMMARY
Experienced software developer with 5+ years of experience in web development and software engineering. 
Strong background in modern programming languages and frameworks. Passionate about creating efficient 
and scalable solutions.

TECHNICAL SKILLS
- Programming Languages: C#, JavaScript, Python, Java
- Frameworks: .NET Core, Vue.js, React, Angular
- Databases: SQL Server, MySQL, MongoDB
- Tools: Git, Docker, Azure, AWS

EXPERIENCE
Senior Software Developer | Tech Company | 2020-2024
- Developed and maintained web applications
- Led a team of 3 developers
- Implemented CI/CD pipelines
- Improved application performance by 40%

Software Developer | Startup Inc | 2018-2020
- Built responsive web interfaces
- Collaborated with cross-functional teams
- Participated in code reviews and testing

EDUCATION
Bachelor of Computer Science | University Name | 2014-2018
- GPA: 3.5/4.0
- Relevant Coursework: Data Structures, Algorithms, Software Engineering

CERTIFICATIONS
- Microsoft Certified: Azure Developer Associate
- AWS Certified Solutions Architect
- Google Cloud Professional Developer
";
    }

    private static List<string> GenerateStrengths(int score)
    {
        var allStrengths = new List<string>
        {
            "Strong technical background",
            "Excellent problem-solving skills",
            "Good communication abilities",
            "Relevant work experience",
            "Strong educational background",
            "Proven track record of success",
            "Leadership experience",
            "Team collaboration skills",
            "Adaptability and learning agility",
            "Attention to detail",
            "Time management skills",
            "Creative thinking",
            "Analytical mindset",
            "Customer focus",
            "Continuous learning attitude"
        };

        var strengthCount = score > 80 ? 4 : score > 60 ? 3 : 2;
        return allStrengths.OrderBy(x => Guid.NewGuid()).Take(strengthCount).ToList();
    }

    private static List<string> GenerateWeaknesses(int score)
    {
        var allWeaknesses = new List<string>
        {
            "Limited experience with specific technology",
            "Could improve presentation skills",
            "Needs more leadership experience",
            "Limited industry knowledge",
            "Could benefit from additional training",
            "Needs to improve time management",
            "Limited project management experience",
            "Could improve written communication",
            "Needs more hands-on experience",
            "Limited exposure to agile methodologies"
        };

        var weaknessCount = score > 80 ? 1 : score > 60 ? 2 : 3;
        return allWeaknesses.OrderBy(x => Guid.NewGuid()).Take(weaknessCount).ToList();
    }

    private static string GenerateSummary(Applicant applicant, JobPost jobPost, int score)
    {
        var scoreDescription = score > 80 ? "excellent" : score > 60 ? "good" : "fair";
        return $"{applicant.FirstName} {applicant.LastName} shows {scoreDescription} potential for the {jobPost.Title} position. " +
               $"With a score of {score}/100, the candidate demonstrates strong technical skills and relevant experience. " +
               $"The AI analysis indicates a good cultural fit and alignment with the job requirements.";
    }

    private static string GenerateDetailedAnalysis(Applicant applicant, JobPost jobPost, int score)
    {
        var recommendation = score > 80 ? "Strongly recommend for interview. Exceptional qualifications." :
                           score > 60 ? "Recommend for interview. Good potential with development areas." :
                           "Consider for interview with reservations. May need additional training.";

        var analysis = $@"AI ANALYSIS REPORT
Candidate: {applicant.FirstName} {applicant.LastName}
Position: {jobPost.Title}
Score: {score}/100

TECHNICAL: {score - 10}% | EXPERIENCE: {score - 5}% | EDUCATION: {score - 15}%
COMMUNICATION: {score - 12}% | PROBLEM SOLVING: {score - 7}% | COLLABORATION: {score - 9}%
CULTURAL FIT: {score - 6}% | WORK STYLE: {score - 11}% | GROWTH: {score - 4}%

RECOMMENDATION: {recommendation}

NEXT STEPS:
1. Schedule technical interview
2. Review portfolio
3. Conduct behavioral assessment
4. Check references";

        return analysis;
    }

    private static string TruncateToLength(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 3) + "...";
    }

    private static string ConvertToLatin(string vietnameseName)
    {
        if (string.IsNullOrEmpty(vietnameseName))
            return string.Empty;

        // Dictionary mapping Vietnamese characters to Latin equivalents
        var vietnameseToLatin = new Dictionary<string, string>
        {
            // Vowels with diacritics
            { "à", "a" }, { "á", "a" }, { "ả", "a" }, { "ã", "a" }, { "ạ", "a" },
            { "ă", "a" }, { "ằ", "a" }, { "ắ", "a" }, { "ẳ", "a" }, { "ẵ", "a" }, { "ặ", "a" },
            { "â", "a" }, { "ầ", "a" }, { "ấ", "a" }, { "ẩ", "a" }, { "ẫ", "a" }, { "ậ", "a" },
            { "è", "e" }, { "é", "e" }, { "ẻ", "e" }, { "ẽ", "e" }, { "ẹ", "e" },
            { "ê", "e" }, { "ề", "e" }, { "ế", "e" }, { "ể", "e" }, { "ễ", "e" }, { "ệ", "e" },
            { "ì", "i" }, { "í", "i" }, { "ỉ", "i" }, { "ĩ", "i" }, { "ị", "i" },
            { "ò", "o" }, { "ó", "o" }, { "ỏ", "o" }, { "õ", "o" }, { "ọ", "o" },
            { "ô", "o" }, { "ồ", "o" }, { "ố", "o" }, { "ổ", "o" }, { "ỗ", "o" }, { "ộ", "o" },
            { "ơ", "o" }, { "ờ", "o" }, { "ớ", "o" }, { "ở", "o" }, { "ỡ", "o" }, { "ợ", "o" },
            { "ù", "u" }, { "ú", "u" }, { "ủ", "u" }, { "ũ", "u" }, { "ụ", "u" },
            { "ư", "u" }, { "ừ", "u" }, { "ứ", "u" }, { "ử", "u" }, { "ữ", "u" }, { "ự", "u" },
            { "ỳ", "y" }, { "ý", "y" }, { "ỷ", "y" }, { "ỹ", "y" }, { "ỵ", "y" },
            
            // Consonants with diacritics
            { "đ", "d" }, { "Đ", "D" },
            
            // Common Vietnamese names
            { "Nguyễn", "Nguyen" }, { "Trần", "Tran" }, { "Lê", "Le" }, { "Phạm", "Pham" },
            { "Hoàng", "Hoang" }, { "Phan", "Phan" }, { "Vũ", "Vu" }, { "Võ", "Vo" },
            { "Đặng", "Dang" }, { "Bùi", "Bui" }, { "Đỗ", "Do" }, { "Hồ", "Ho" },
            { "Ngô", "Ngo" }, { "Dương", "Duong" }, { "Lý", "Ly" },
            { "Văn", "Van" }, { "Thị", "Thi" }, { "Minh", "Minh" }, { "Hồng", "Hong" },
            { "Thanh", "Thanh" }, { "Thu", "Thu" }, { "Linh", "Linh" }, { "Anh", "Anh" },
            { "Hương", "Huong" }, { "Lan", "Lan" }, { "Phương", "Phuong" },
            { "Hạnh", "Hanh" }, { "Nga", "Nga" }, { "Tuyết", "Tuyet" }, { "Mai", "Mai" }
        };

        var result = vietnameseName;
        foreach (var mapping in vietnameseToLatin)
        {
            result = result.Replace(mapping.Key, mapping.Value);
        }

        return result;
    }
}