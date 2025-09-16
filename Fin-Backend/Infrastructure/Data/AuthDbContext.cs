using Fin_Backend.Domain.Entities.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fin_Backend.Infrastructure.Data
{
    /// <summary>
    /// Application identity database context
    /// </summary>
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class
        /// </summary>
        /// <param name="options">The options</param>
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the refresh tokens
        /// </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Gets or sets the MFA settings
        /// </summary>
        public DbSet<MfaSettings> MfaSettings { get; set; }

        /// <summary>
        /// Gets or sets the backup codes
        /// </summary>
        public DbSet<BackupCode> BackupCodes { get; set; }

        /// <summary>
        /// Gets or sets the MFA challenges
        /// </summary>
        public DbSet<MfaChallenge> MfaChallenges { get; set; }

        /// <summary>
        /// Gets or sets the trusted devices
        /// </summary>
        public DbSet<TrustedDevice> TrustedDevices { get; set; }

        /// <summary>
        /// Gets or sets the login attempts
        /// </summary>
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        /// <summary>
        /// Gets or sets the social login profiles
        /// </summary>
        public DbSet<SocialLoginProfile> SocialLoginProfiles { get; set; }

        /// <summary>
        /// Gets or sets the security alerts
        /// </summary>
        public DbSet<SecurityAlert> SecurityAlerts { get; set; }

        /// <summary>
        /// Gets or sets the user security preferences
        /// </summary>
        public DbSet<UserSecurityPreferences> UserSecurityPreferences { get; set; }

        /// <summary>
        /// Configure the model
        /// </summary>
        /// <param name="builder">The model builder</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships and indexes
            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            builder.Entity<MfaSettings>()
                .HasIndex(ms => ms.UserId)
                .IsUnique();

            builder.Entity<BackupCode>()
                .HasIndex(bc => bc.UserId);

            builder.Entity<BackupCode>()
                .HasIndex(bc => bc.Code);

            builder.Entity<MfaChallenge>()
                .HasIndex(mc => mc.UserId);

            builder.Entity<MfaChallenge>()
                .HasIndex(mc => mc.VerificationCode);

            builder.Entity<TrustedDevice>()
                .HasIndex(td => td.UserId);

            builder.Entity<TrustedDevice>()
                .HasIndex(td => td.DeviceId);

            builder.Entity<LoginAttempt>()
                .HasIndex(la => la.UserId);

            builder.Entity<LoginAttempt>()
                .HasIndex(la => la.Username);

            builder.Entity<LoginAttempt>()
                .HasIndex(la => la.AttemptTime);

            builder.Entity<SocialLoginProfile>()
                .HasIndex(slp => slp.UserId);

            builder.Entity<SocialLoginProfile>()
                .HasIndex(slp => new { slp.Provider, slp.ProviderKey })
                .IsUnique();

            builder.Entity<SecurityAlert>()
                .HasIndex(sa => sa.UserId);

            builder.Entity<SecurityAlert>()
                .HasIndex(sa => sa.CreatedAt);

            builder.Entity<UserSecurityPreferences>()
                .HasIndex(usp => usp.UserId)
                .IsUnique();
        }
    }
}