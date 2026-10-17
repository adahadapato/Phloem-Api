namespace Phloem.Application.Common.Settings;

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
/// <para><b>Secrets</b> (passwords, keys, connection strings with passwords) never go in appsettings.json:
/// User Secrets in development, environment variables / a key vault in production.</para>
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
}