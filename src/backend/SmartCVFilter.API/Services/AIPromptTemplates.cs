using System.Text;

namespace SmartCVFilter.API.Services;

public static class AIPromptTemplates
{
    public static class JobTypes
    {
        public const string Software = "Software Development";
        public const string Marketing = "Marketing";
        public const string Sales = "Sales";
        public const string Finance = "Finance";
        public const string HR = "Human Resources";
        public const string Design = "Design";
        public const string Data = "Data Science";
        public const string Management = "Management";
        public const string Operations = "Operations";
        public const string CustomerService = "Customer Service";
    }

    public static class ExperienceLevels
    {
        public const string Entry = "Entry Level";
        public const string Mid = "Mid Level";
        public const string Senior = "Senior Level";
        public const string Lead = "Lead Level";
        public const string Executive = "Executive Level";
    }

    public static string GetPromptTemplate(string jobType, string experienceLevel, string jobDescription, string requiredSkills, string preferredSkills, string responsibilities)
    {
        var basePrompt = GetBasePromptTemplate();
        var jobSpecificPrompt = GetJobSpecificPrompt(jobType, experienceLevel);
        var scoringCriteria = GetScoringCriteria(jobType, experienceLevel);

        return $@"{basePrompt}

JOB DETAILS:
- Job Type: {jobType}
- Experience Level: {experienceLevel}
- Job Description: {jobDescription}
- Required Skills: {requiredSkills}
- Preferred Skills: {preferredSkills}
- Key Responsibilities: {responsibilities}

{jobSpecificPrompt}

{scoringCriteria}

Please analyze the CV and provide a comprehensive assessment in the following JSON format:
{{
    ""OverallScore"": number (0-100),
    ""Summary"": ""string (2-3 sentences summarizing the candidate's fit)"",
    ""Strengths"": [""string1"", ""string2"", ""string3""],
    ""Weaknesses"": [""string1"", ""string2"", ""string3""],
    ""DetailedAnalysis"": ""string (comprehensive analysis of 3-4 paragraphs)"",
    ""SkillMatch"": {{
        ""RequiredSkillsMatch"": number (0-100),
        ""PreferredSkillsMatch"": number (0-100),
        ""MissingCriticalSkills"": [""skill1"", ""skill2""],
        ""StrongSkills"": [""skill1"", ""skill2""]
    }},
    ""ExperienceAssessment"": {{
        ""RelevantExperience"": number (0-100),
        ""ExperienceLevelMatch"": ""string (Entry/Mid/Senior/Lead/Executive)"",
        ""YearsOfExperience"": number,
        ""IndustryExperience"": ""string""
    }},
    ""Recommendation"": ""string (Hire/Interview/Reject with brief reasoning)"",
    ""InterviewQuestions"": [""question1"", ""question2"", ""question3""]
}}";
    }

    private static string GetBasePromptTemplate()
    {
        return @"You are an expert HR recruiter and technical interviewer with 15+ years of experience in talent acquisition. 
Your task is to analyze a candidate's CV against a specific job posting and provide a comprehensive, objective assessment.

ANALYSIS GUIDELINES:
1. Be objective and fair in your assessment
2. Focus on relevant skills, experience, and qualifications
3. Consider both technical and soft skills
4. Look for growth potential and cultural fit indicators
5. Identify any red flags or concerns
6. Provide actionable insights for hiring decisions

CV CONTENT TO ANALYZE:";
    }

    private static string GetJobSpecificPrompt(string jobType, string experienceLevel)
    {
        return jobType switch
        {
            JobTypes.Software => GetSoftwarePrompt(experienceLevel),
            JobTypes.Marketing => GetMarketingPrompt(experienceLevel),
            JobTypes.Sales => GetSalesPrompt(experienceLevel),
            JobTypes.Finance => GetFinancePrompt(experienceLevel),
            JobTypes.HR => GetHRPrompt(experienceLevel),
            JobTypes.Design => GetDesignPrompt(experienceLevel),
            JobTypes.Data => GetDataPrompt(experienceLevel),
            JobTypes.Management => GetManagementPrompt(experienceLevel),
            JobTypes.Operations => GetOperationsPrompt(experienceLevel),
            JobTypes.CustomerService => GetCustomerServicePrompt(experienceLevel),
            _ => GetGenericPrompt(experienceLevel)
        };
    }

    private static string GetSoftwarePrompt(string experienceLevel)
    {
        return $@"
SOFTWARE DEVELOPMENT FOCUS:
- Technical Skills: Programming languages, frameworks, databases, cloud platforms
- Development Experience: Full-stack, frontend, backend, mobile, DevOps
- Project Portfolio: GitHub, personal projects, open-source contributions
- Problem-Solving: Algorithm knowledge, system design, debugging skills
- Collaboration: Code reviews, pair programming, agile methodologies
- Continuous Learning: Certifications, courses, technology trends

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetMarketingPrompt(string experienceLevel)
    {
        return $@"
MARKETING FOCUS:
- Digital Marketing: SEO, SEM, social media, email marketing, content marketing
- Analytics: Google Analytics, data analysis, ROI measurement, KPI tracking
- Tools: Marketing automation platforms, CRM systems, design tools
- Campaign Management: Strategy development, execution, performance optimization
- Brand Management: Brand positioning, messaging, visual identity
- Industry Knowledge: Market trends, competitor analysis, customer insights

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetSalesPrompt(string experienceLevel)
    {
        return $@"
SALES FOCUS:
- Sales Experience: B2B, B2C, inside sales, field sales, account management
- Sales Process: Lead generation, prospecting, qualification, closing
- CRM Systems: Salesforce, HubSpot, or similar platforms
- Communication: Presentation skills, negotiation, relationship building
- Industry Knowledge: Product knowledge, market understanding, competitive landscape
- Performance Metrics: Sales targets, conversion rates, revenue generation

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetFinancePrompt(string experienceLevel)
    {
        return $@"
FINANCE FOCUS:
- Financial Analysis: Financial modeling, forecasting, budgeting, variance analysis
- Accounting: GAAP, financial reporting, audit experience, tax knowledge
- Software: Excel, QuickBooks, SAP, Oracle, or similar financial systems
- Certifications: CPA, CFA, CMA, or relevant financial certifications
- Industry Experience: Banking, investment, corporate finance, or specific sectors
- Regulatory Knowledge: Compliance, risk management, financial regulations

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetHRPrompt(string experienceLevel)
    {
        return $@"
HUMAN RESOURCES FOCUS:
- HR Functions: Recruitment, employee relations, performance management, training
- HR Systems: ATS, HRIS, payroll systems, performance management tools
- Compliance: Employment law, labor relations, workplace policies
- Communication: Conflict resolution, employee engagement, organizational development
- Certifications: PHR, SHRM-CP, or similar HR certifications
- Industry Knowledge: HR best practices, talent management, organizational culture

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetDesignPrompt(string experienceLevel)
    {
        return $@"
DESIGN FOCUS:
- Design Skills: UI/UX, graphic design, web design, product design
- Tools: Figma, Adobe Creative Suite, Sketch, InVision, or similar
- Portfolio: Design projects, case studies, creative problem-solving
- User Research: User testing, personas, wireframing, prototyping
- Collaboration: Working with developers, product managers, stakeholders
- Industry Knowledge: Design trends, accessibility, responsive design

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetDataPrompt(string experienceLevel)
    {
        return $@"
DATA SCIENCE FOCUS:
- Technical Skills: Python, R, SQL, machine learning, statistics
- Data Tools: Pandas, NumPy, Scikit-learn, TensorFlow, PyTorch
- Analytics: Data visualization, statistical analysis, predictive modeling
- Databases: SQL, NoSQL, data warehousing, ETL processes
- Domain Knowledge: Business intelligence, data engineering, AI/ML applications
- Communication: Data storytelling, presenting insights to stakeholders

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetManagementPrompt(string experienceLevel)
    {
        return $@"
MANAGEMENT FOCUS:
- Leadership: Team management, project leadership, strategic planning
- Communication: Stakeholder management, cross-functional collaboration
- Decision Making: Problem-solving, risk assessment, resource allocation
- Industry Experience: Relevant sector knowledge, market understanding
- Results: Performance improvement, team development, business impact
- Education: MBA, management certifications, or equivalent experience

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetOperationsPrompt(string experienceLevel)
    {
        return $@"
OPERATIONS FOCUS:
- Process Improvement: Lean, Six Sigma, operational efficiency
- Project Management: PMP, Agile, Scrum, or similar methodologies
- Systems: ERP, supply chain management, quality control systems
- Industry Knowledge: Manufacturing, logistics, service operations
- Problem Solving: Root cause analysis, continuous improvement
- Leadership: Team management, vendor relationships, cost optimization

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetCustomerServicePrompt(string experienceLevel)
    {
        return $@"
CUSTOMER SERVICE FOCUS:
- Communication: Verbal and written communication, active listening
- Problem Solving: Issue resolution, customer satisfaction, conflict management
- Systems: CRM, ticketing systems, knowledge management platforms
- Industry Knowledge: Product knowledge, service standards, customer expectations
- Soft Skills: Empathy, patience, professionalism, teamwork
- Performance: Customer satisfaction scores, resolution times, feedback

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetGenericPrompt(string experienceLevel)
    {
        return $@"
GENERAL PROFESSIONAL FOCUS:
- Relevant Experience: Industry experience, role-specific skills
- Education: Degree, certifications, continuous learning
- Skills: Technical and soft skills relevant to the position
- Achievements: Quantifiable accomplishments, career progression
- Communication: Written and verbal communication abilities
- Adaptability: Learning agility, change management, growth mindset

{GetExperienceLevelGuidance(experienceLevel)}";
    }

    private static string GetExperienceLevelGuidance(string experienceLevel)
    {
        return experienceLevel switch
        {
            ExperienceLevels.Entry => @"
ENTRY LEVEL CONSIDERATIONS:
- Focus on education, internships, and potential
- Look for transferable skills and eagerness to learn
- Consider cultural fit and motivation
- Evaluate problem-solving approach and communication skills
- Don't expect extensive industry experience",
            ExperienceLevels.Mid => @"
MID LEVEL CONSIDERATIONS:
- Look for 2-5 years of relevant experience
- Evaluate technical competency and project experience
- Consider leadership potential and mentoring abilities
- Assess problem-solving skills and independent work capability
- Look for growth trajectory and career progression",
            ExperienceLevels.Senior => @"
SENIOR LEVEL CONSIDERATIONS:
- Require 5+ years of relevant experience
- Evaluate technical expertise and architectural thinking
- Look for mentoring and leadership experience
- Assess strategic thinking and business impact
- Consider industry knowledge and best practices",
            ExperienceLevels.Lead => @"
LEAD LEVEL CONSIDERATIONS:
- Require 7+ years of relevant experience
- Evaluate team leadership and technical direction
- Look for project management and stakeholder management
- Assess strategic thinking and business acumen
- Consider innovation and process improvement experience",
            ExperienceLevels.Executive => @"
EXECUTIVE LEVEL CONSIDERATIONS:
- Require 10+ years of relevant experience
- Evaluate strategic leadership and vision
- Look for P&L responsibility and business impact
- Assess industry expertise and market knowledge
- Consider board-level communication and decision-making",
            _ => @"
EXPERIENCE LEVEL CONSIDERATIONS:
- Evaluate experience against job requirements
- Consider career progression and growth potential
- Look for relevant skills and achievements
- Assess cultural fit and motivation
- Consider learning agility and adaptability"
        };
    }

    private static string GetScoringCriteria(string jobType, string experienceLevel)
    {
        return $@"
SCORING CRITERIA (0-100 scale):
- 90-100: Exceptional match - exceeds requirements, strong hire
- 80-89: Strong match - meets most requirements, good hire
- 70-79: Good match - meets basic requirements, consider for interview
- 60-69: Fair match - some gaps, interview to assess potential
- 50-59: Weak match - significant gaps, consider only if other factors are strong
- Below 50: Poor match - does not meet basic requirements

FACTORS TO CONSIDER:
1. Technical/Functional Skills (30%)
2. Relevant Experience (25%)
3. Education/Certifications (15%)
4. Soft Skills/Communication (15%)
5. Cultural Fit/Potential (10%)
6. Career Progression/Growth (5%)

Be thorough but concise in your analysis. Provide specific examples from the CV to support your assessment.";
    }
}
