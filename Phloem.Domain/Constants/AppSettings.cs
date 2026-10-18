namespace Phloem.Domain.Constants;

/// <summary>
/// Every configuration key Phloem reads (appsettings.json, User Secrets, environment variables),
/// in one place so no code repeats "magic strings".
/// </summary>
/// <remarks>
/// <para><b>Rule:</b> when a new setting is added to appsettings.json, add its key here in the same change.</para>
/// <para><b>Sections</b> are what <c>configuration.GetSection(...)</c> binds to a typed settings class
/// (e.g. <c>EmailSettings</c>). <b>Keys</b> are for reading one value directly
/// (<c>configuration[AppSettings.Email.Host]</c>).</para>
/// <para>Environment variables use a double underscore instead of a colon: <c>Email:Password</c> → <c>Email__Password</c>.</para>
/// <para><b>Secrets</b> (passwords, keys, connection strings with passwords) appear in appsettings.json only as empty
/// placeholders (<c>""</c>), so CI/CD can map them: User Secrets in development, GitHub Secrets / host environment
/// variables in production.</para>
/// </remarks>
public static class AppSettings
{
    // ==================== Application ====================

    /// <summary>
    /// The public addresses of Phloem itself. Development overrides <see cref="ApiBaseUrl"/> in
    /// appsettings.Development.json.
    /// </summary>
    public static class Application
    {
        /// <summary>The whole section.</summary>
        public const string Section = "AppSettings";

        /// <summary>
        /// The API's own public address, without a trailing slash (e.g. "https://api.phloem.app").
        /// Used to build file links (photos, private-document links) and links in emails.
        /// </summary>
        public const string ApiBaseUrl = "AppSettings:ApiBaseUrl";

        /// <summary>
        /// The public website's address, without a trailing slash (e.g. "https://phloem.app").
        /// Used for links to web pages (terms, help, password-reset page).
        /// </summary>
        public const string WebsiteBaseUrl = "AppSettings:WebsiteBaseUrl";
    }


    // ==================== OpenAPI / Swagger ====================

    /// <summary>
    /// The API description (<c>/openapi/v1.json</c>) and the Swagger page.
    /// Bound to <c>Phloem.Presentation.Settings.OpenApi.OpenApiSettings</c>.
    /// </summary>
    /// <remarks>Off in appsettings.json (production), on in appsettings.Development.json.</remarks>
    public static class OpenApi
    {
        /// <summary>The whole section.</summary>
        public const string Section = "OpenApi";

        /// <summary>Publish the document and the Swagger page (true/false).</summary>
        public const string Enabled = "OpenApi:Enabled";

        /// <summary>Title at the top of the page, e.g. "Phloem API".</summary>
        public const string Title = "OpenApi:Title";

        /// <summary>Document name and version, e.g. "v1" (also used in the document's URL).</summary>
        public const string Version = "OpenApi:Version";

        /// <summary>Text under the title.</summary>
        public const string Description = "OpenApi:Description";

        /// <summary>Where the JSON document is served; must contain "{documentName}".</summary>
        public const string DocumentPath = "OpenApi:DocumentPath";

        /// <summary>Address of the Swagger page, without slashes, e.g. "swagger" → /swagger.</summary>
        public const string UiPath = "OpenApi:UiPath";

        /// <summary>Who builds and looks after the API, shown under the title. Empty values are left out.</summary>
        public static class Contact
        {
            /// <summary>The whole sub-section.</summary>
            public const string Section = "OpenApi:Contact";

            /// <summary>Developer's name.</summary>
            public const string Name = "OpenApi:Contact:Name";

            /// <summary>Developer's email.</summary>
            public const string Email = "OpenApi:Contact:Email";

            /// <summary>Developer's website or profile (absolute URL).</summary>
            public const string Url = "OpenApi:Contact:Url";
        }
    }

    // ==================== Connection strings ====================

    /// <summary>
    /// Database connections. Read with <c>configuration.GetConnectionString(AppSettings.ConnectionStrings.DefaultName)</c>.
    /// </summary>
    /// <remarks>
    /// SECRET (it holds the database password): empty in appsettings.json; User Secrets in development,
    /// GitHub Secrets / host environment variables in production (<c>ConnectionStrings__DefaultConnection</c>).
    /// </remarks>
    public static class ConnectionStrings
    {
        /// <summary>The whole section.</summary>
        public const string Section = "ConnectionStrings";

        /// <summary>The main SQL Server database, as a name for <c>GetConnectionString(...)</c>.</summary>
        public const string DefaultName = "DefaultConnection";

        /// <summary>The main SQL Server database, as a full key.</summary>
        public const string Default = "ConnectionStrings:DefaultConnection";
    }

    // ==================== Seed data ====================

    /// <summary>
    /// What the database seeder needs on first start-up: the first Admin account.
    /// Bound to <c>Phloem.Persistence.Seeding.SeedDataSettings</c>.
    /// </summary>
    /// <remarks>
    /// <para>The admin is created only if no user with <see cref="AdminEmail"/> exists yet. Changing these values later
    /// does NOT change an existing account (change it in the app instead).</para>
    /// <para>If the email or password is empty, admin seeding is skipped with a warning in the log.</para>
    /// <para>SECRET: <see cref="AdminEmail"/> and <see cref="AdminPassword"/> are empty in appsettings.json; User Secrets
    /// in development, GitHub Secrets / host environment variables in production
    /// (<c>SeedData__AdminEmail</c>, <c>SeedData__AdminPassword</c>).</para>
    /// </remarks>
    public static class SeedData
    {
        /// <summary>The whole section.</summary>
        public const string Section = "SeedData";

        /// <summary>The first Admin's email address (also their sign-in name). SECRET.</summary>
        public const string AdminEmail = "SeedData:AdminEmail";

        /// <summary>The first Admin's password; must meet the Identity password rules. SECRET.</summary>
        public const string AdminPassword = "SeedData:AdminPassword";

        /// <summary>The first Admin's first name on their profile, e.g. "Phloem".</summary>
        public const string AdminFirstName = "SeedData:AdminFirstName";

        /// <summary>The first Admin's last name on their profile, e.g. "Admin".</summary>
        public const string AdminLastName = "SeedData:AdminLastName";

        /// <summary>The first Admin's country, by enum name: "Ghana", "Nigeria" or "UnitedKingdom".</summary>
        public const string AdminCountry = "SeedData:AdminCountry";

        /// <summary>Seed the starter data (commissions, delivery pricing, dish list, routes) on an empty database
        /// (true/false).</summary>
        public const string StarterDataEnabled = "SeedData:StarterDataEnabled";

        /// <summary>The starter-data JSON file, relative to the app folder, e.g. "Seeding/Data/starter-data.json".</summary>
        public const string StarterDataPath = "SeedData:StarterDataPath";
    }

    // ==================== Identity ====================

    /// <summary>
    /// Sign-in rules: password strength, lockout after wrong passwords, email confirmation.
    /// Bound to <c>Phloem.Infrastructure.Settings.Identity.IdentitySettings</c>.
    /// </summary>
    /// <remarks>The only place password strength is defined: the Application validators only check "not empty" and
    /// Identity's own message comes back as a Result.</remarks>
    public static class Identity
    {
        /// <summary>The whole section.</summary>
        public const string Section = "Identity";

        /// <summary>Refuse sign-in until the email is confirmed (true/false).</summary>
        public const string RequireConfirmedEmail = "Identity:RequireConfirmedEmail";

        /// <summary>The name shown in authenticator apps next to the 6-digit code, e.g. "Phloem".</summary>
        public const string AuthenticatorIssuer = "Identity:AuthenticatorIssuer";

        /// <summary>Password strength.</summary>
        public static class Password
        {
            /// <summary>The whole sub-section.</summary>
            public const string Section = "Identity:Password";

            /// <summary>Minimum length.</summary>
            public const string RequiredLength = "Identity:Password:RequiredLength";

            /// <summary>Needs a digit 0–9 (true/false).</summary>
            public const string RequireDigit = "Identity:Password:RequireDigit";

            /// <summary>Needs a lowercase letter (true/false).</summary>
            public const string RequireLowercase = "Identity:Password:RequireLowercase";

            /// <summary>Needs an uppercase letter (true/false).</summary>
            public const string RequireUppercase = "Identity:Password:RequireUppercase";

            /// <summary>Needs a symbol such as ! or # (true/false).</summary>
            public const string RequireNonAlphanumeric = "Identity:Password:RequireNonAlphanumeric";

            /// <summary>Minimum number of different characters.</summary>
            public const string RequiredUniqueChars = "Identity:Password:RequiredUniqueChars";
        }

        /// <summary>Temporary lockout after too many wrong passwords.</summary>
        public static class Lockout
        {
            /// <summary>The whole sub-section.</summary>
            public const string Section = "Identity:Lockout";

            /// <summary>Wrong passwords allowed before the lockout.</summary>
            public const string MaxFailedAccessAttempts = "Identity:Lockout:MaxFailedAccessAttempts";

            /// <summary>How long the lockout lasts, in minutes.</summary>
            public const string LockoutMinutes = "Identity:Lockout:LockoutMinutes";

            /// <summary>Whether new accounts can be locked out at all (true/false).</summary>
            public const string AllowedForNewUsers = "Identity:Lockout:AllowedForNewUsers";
        }

        /// <summary>Pages on the website that email links open (joined to <c>AppSettings:WebsiteBaseUrl</c>).</summary>
        public static class Links
        {
            /// <summary>The whole sub-section.</summary>
            public const string Section = "Identity:Links";

            /// <summary>"Confirm your email" page, e.g. "/confirm-email" (gets ?userId=…&amp;token=…).</summary>
            public const string ConfirmEmailPath = "Identity:Links:ConfirmEmailPath";

            /// <summary>"Choose a new password" page, e.g. "/reset-password" (gets ?email=…&amp;token=…).</summary>
            public const string ResetPasswordPath = "Identity:Links:ResetPasswordPath";
        }
    }

    // ==================== JWT ====================

    /// <summary>
    /// Sign-in tokens: the short-lived access token (JWT), the long-lived refresh token, and the 2FA challenge.
    /// Bound to <c>Phloem.Infrastructure.Settings.Identity.JwtSettings</c>.
    /// </summary>
    public static class Jwt
    {
        /// <summary>The whole section.</summary>
        public const string Section = "Jwt";

        /// <summary>Key that signs every token (HS256, at least 32 characters).
        /// SECRET: User Secrets / environment variables only (<c>Jwt__Key</c>).</summary>
        public const string Key = "Jwt:Key";

        /// <summary>Who signed the token, e.g. "Phloem-API".</summary>
        public const string Issuer = "Jwt:Issuer";

        /// <summary>Who the token is for, e.g. "Phloem-Client".</summary>
        public const string Audience = "Jwt:Audience";

        /// <summary>Access-token lifetime, in minutes.</summary>
        public const string ExpiryMinutes = "Jwt:ExpiryMinutes";

        /// <summary>Refresh-token lifetime ("stay signed in"), in days.</summary>
        public const string RefreshTokenDays = "Jwt:RefreshTokenDays";

        /// <summary>Time allowed to enter the 2FA code after the password, in minutes.</summary>
        public const string TwoFactorChallengeMinutes = "Jwt:TwoFactorChallengeMinutes";
    }

    // ==================== Data protection ====================

    /// <summary>
    /// The keys that sign email-confirmation / password-reset tokens and the 2FA challenge.
    /// Bound to <c>Phloem.Infrastructure.Settings.Identity.DataProtectionSettings</c>.
    /// </summary>
    /// <remarks>Kept in a folder next to the app so they survive restarts on shared hosting (where the app pool has
    /// no user profile and keys would otherwise be lost, breaking every link already emailed).</remarks>
    public static class DataProtection
    {
        /// <summary>The whole section.</summary>
        public const string Section = "DataProtection";

        /// <summary>Name that isolates Phloem's keys from other apps on the same server, e.g. "Phloem".</summary>
        public const string ApplicationName = "DataProtection:ApplicationName";

        /// <summary>Folder for the key files, relative to the app folder (e.g. "App_Data/keys"), never in wwwroot.</summary>
        public const string KeysPath = "DataProtection:KeysPath";
    }

    // ==================== Storage ====================

    /// <summary>
    /// Uploaded files (photos, documents). Bound to <c>Phloem.Infrastructure.Settings.Storage.StorageSettings</c>.
    /// </summary>
    public static class Storage
    {
        /// <summary>The whole section.</summary>
        public const string Section = "Storage";

        /// <summary>Which provider keeps files: "Local" (more later).</summary>
        public const string Provider = "Storage:Provider";

        /// <summary>Settings for the Local provider.</summary>
        public static class Local
        {
            /// <summary>The whole sub-section.</summary>
            public const string Section = "Storage:Local";

            /// <summary>Public files folder, inside the web root (e.g. "wwwroot/uploads").</summary>
            public const string PublicRootPath = "Storage:Local:PublicRootPath";

            /// <summary>Private files folder (e.g. "wwwroot/private-uploads"); blocked from direct access.</summary>
            public const string PrivateRootPath = "Storage:Local:PrivateRootPath";

            /// <summary>Web path of the signed private-download endpoint (e.g. "/private-files").</summary>
            public const string PrivateDownloadPath = "Storage:Local:PrivateDownloadPath";

            /// <summary>Key that signs private-file links. SECRET: User Secrets / environment variables only.</summary>
            public const string SigningKey = "Storage:Local:SigningKey";
        }
    }

    // ==================== Images ====================

    /// <summary>
    /// Photo pipeline tuning (limits and quality). Bound to <c>Phloem.Infrastructure.Settings.Images.ImageSettings</c>.
    /// Output sizes are Domain rules (<c>DishPhotoStandard</c>), not settings.
    /// </summary>
    public static class Images
    {
        /// <summary>The whole section.</summary>
        public const string Section = "Images";

        /// <summary>Largest dish-photo upload in bytes.</summary>
        public const string MaxDishPhotoBytes = "Images:MaxDishPhotoBytes";

        /// <summary>Largest profile-photo / logo upload in bytes.</summary>
        public const string MaxSimpleImageBytes = "Images:MaxSimpleImageBytes";

        /// <summary>Longest side for profile photos / logos, in pixels.</summary>
        public const string SimpleImageMaxSide = "Images:SimpleImageMaxSide";

        /// <summary>WebP quality, 1–100.</summary>
        public const string WebpQuality = "Images:WebpQuality";

        /// <summary>Minimum sharpness for dish photos.</summary>
        public const string BlurThreshold = "Images:BlurThreshold";

        /// <summary>Light sharpen on dish photos (true/false).</summary>
        public const string EnhanceDishPhotos = "Images:EnhanceDishPhotos";
    }

    // ==================== Email ====================

    /// <summary>
    /// Outgoing email (SMTP). Bound to <c>Phloem.Infrastructure.Settings.Email.EmailSettings</c>.
    /// </summary>
    public static class Email
    {
        /// <summary>The whole section.</summary>
        public const string Section = "Email";

        /// <summary>Display name on outgoing emails, e.g. "Phloem".</summary>
        public const string FromName = "Email:FromName";

        /// <summary>Sending address, e.g. "no-reply@phloem.app". Replies never go here (see <see cref="ReplyTo"/>).</summary>
        public const string FromAddress = "Email:FromAddress";

        /// <summary>
        /// The team's own inbox: emails TO Phloem staff (e.g. "New shop waiting for approval",
        /// "Seller uploaded a document").
        /// </summary>
        public const string TeamInbox = "Email:TeamInbox";

        /// <summary>SMTP server host.</summary>
        public const string Host = "Email:Host";

        /// <summary>SMTP port (587 StartTls, 465 SslOnConnect, 25 local test inbox).</summary>
        public const string Port = "Email:Port";

        /// <summary>Encryption: StartTls, SslOnConnect, Auto or None.</summary>
        public const string Security = "Email:Security";

        /// <summary>SMTP user name (empty = no sign-in).</summary>
        public const string Username = "Email:Username";

        /// <summary>SMTP password. SECRET: User Secrets / environment variables only.</summary>
        public const string Password = "Email:Password";

        /// <summary>
        /// Where replies go when a customer or seller presses "Reply": one inbox per audience.
        /// All three may be the same address; split them later without code changes.
        /// </summary>
        public static class ReplyTo
        {
            /// <summary>The whole sub-section.</summary>
            public const string Section = "Email:ReplyTo";

            /// <summary>Customers: orders, account, price changes. e.g. "support@phloem.app".</summary>
            public const string Support = "Email:ReplyTo:Support";

            /// <summary>Sellers: shop approval, price ranges, delivery fees. e.g. "sellers@phloem.app".</summary>
            public const string Sellers = "Email:ReplyTo:Sellers";

            /// <summary>Parcels: weighing, payment, delivery. e.g. "shipping@phloem.app".</summary>
            public const string Shipping = "Email:ReplyTo:Shipping";
        }
    }

    // ==================== Hosting ====================

    /// <summary>ASP.NET Core hosting configuration.</summary>
    public static class Hosting
    {
        /// <summary>Host names the API answers to ("*" = any).</summary>
        public const string AllowedHosts = "AllowedHosts";
    }

    // ==================== Logging ====================

    /// <summary>
    /// Serilog: log levels and where logs go (console + daily files in <c>logs/</c>, kept 14 days).
    /// Read by Serilog itself (<c>ReadFrom.Configuration</c>), not bound to a settings class.
    /// </summary>
    /// <remarks>
    /// <para>Change a level without code changes, e.g. <c>Serilog:MinimumLevel:Override:Phloem</c> = <c>Debug</c>
    /// (environment variable <c>Serilog__MinimumLevel__Override__Phloem</c>).</para>
    /// <para>The <c>logs/</c> folder is next to the app, never inside <c>wwwroot</c>, so log files can't be downloaded.</para>
    /// </remarks>
    public static class Serilog
    {
        /// <summary>The whole section.</summary>
        public const string Section = "Serilog";

        /// <summary>The default minimum level, e.g. "Information".</summary>
        public const string MinimumLevelDefault = "Serilog:MinimumLevel:Default";
    }
}