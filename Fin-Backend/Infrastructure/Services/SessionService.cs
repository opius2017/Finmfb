using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FinTech.WebAPI.Infrastructure.Services
{
    public class SessionInfo
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
    }

    public class SessionOptions
    {
        public int SessionTimeoutMinutes { get; set; } = 30;
        public int MaxConcurrentSessions { get; set; } = 5;
        public bool EnforceUniqueSession { get; set; } = false;
        public bool TrackUserActivity { get; set; } = true;
        public bool ValidateIpAddress { get; set; } = true;
        public bool ValidateUserAgent { get; set; } = true;
    }

    public interface ISessionService
    {
        Task<SessionInfo> CreateSessionAsync(string userId, string username, string ipAddress, string userAgent);
        Task<SessionInfo> GetSessionAsync(string sessionId);
        Task<bool> ValidateSessionAsync(string sessionId, string ipAddress, string userAgent);
        Task<bool> ExtendSessionAsync(string sessionId);
        Task<bool> EndSessionAsync(string sessionId);
        Task<IEnumerable<SessionInfo>> GetUserSessionsAsync(string userId);
        Task<bool> EndAllUserSessionsExceptCurrentAsync(string userId, string currentSessionId);
    }

    public class SessionService : ISessionService
    {
        private readonly IDistributedCache _cache;
        private readonly IEncryptionService _encryptionService;
        private readonly ISecurityEventService _securityEventService;
        private readonly SessionOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string SESSION_KEY_PREFIX = "session:";
        private const string USER_SESSIONS_KEY_PREFIX = "user-sessions:";

        public SessionService(
            IDistributedCache cache,
            IEncryptionService encryptionService,
            ISecurityEventService securityEventService,
            IOptions<SessionOptions> options,
            IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _encryptionService = encryptionService;
            _securityEventService = securityEventService;
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SessionInfo> CreateSessionAsync(string userId, string username, string ipAddress, string userAgent)
        {
            // Generate a secure session ID
            string sessionId = _encryptionService.GenerateRandomToken();

            // If enforcing unique sessions, end all other active sessions for this user
            if (_options.EnforceUniqueSession)
            {
                await EndAllUserSessionsAsync(userId);
            }
            // If we have a max number of concurrent sessions, enforce it
            else if (_options.MaxConcurrentSessions > 0)
            {
                await EnforceMaxConcurrentSessionsAsync(userId);
            }

            // Create new session
            var sessionInfo = new SessionInfo
            {
                SessionId = sessionId,
                UserId = userId,
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.SessionTimeoutMinutes),
                IsActive = true
            };

            // Store session in cache
            await StoreSessionAsync(sessionInfo);

            // Add session to user's sessions list
            await AddSessionToUserSessionsAsync(userId, sessionId);

            // Log session creation
            await _securityEventService.LogEventAsync(new SecurityEvent
            {
                EventType = "SessionCreated",
                Description = "New user session created",
                Username = username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Severity = SecurityEventSeverity.Info
            });

            return sessionInfo;
        }

        public async Task<SessionInfo> GetSessionAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            string sessionKey = GetSessionKey(sessionId);
            string sessionJson = await _cache.GetStringAsync(sessionKey);

            if (string.IsNullOrEmpty(sessionJson))
                return null;

            return JsonSerializer.Deserialize<SessionInfo>(sessionJson);
        }

        public async Task<bool> ValidateSessionAsync(string sessionId, string ipAddress, string userAgent)
        {
            var session = await GetSessionAsync(sessionId);

            if (session == null || !session.IsActive || DateTime.UtcNow > session.ExpiresAt)
                return false;

            // Validate IP address if enabled
            if (_options.ValidateIpAddress && session.IpAddress != ipAddress)
            {
                await _securityEventService.LogSuspiciousActivityAsync(
                    session.Username,
                    ipAddress,
                    userAgent,
                    $"Session IP mismatch. Expected: {session.IpAddress}, Actual: {ipAddress}",
                    SecurityEventSeverity.Warning);
                return false;
            }

            // Validate user agent if enabled
            if (_options.ValidateUserAgent && session.UserAgent != userAgent)
            {
                await _securityEventService.LogSuspiciousActivityAsync(
                    session.Username,
                    ipAddress,
                    userAgent,
                    "Session user agent mismatch",
                    SecurityEventSeverity.Warning);
                return false;
            }

            // If we're tracking user activity, update the last activity timestamp
            if (_options.TrackUserActivity)
            {
                session.LastActivityAt = DateTime.UtcNow;
                session.ExpiresAt = DateTime.UtcNow.AddMinutes(_options.SessionTimeoutMinutes);
                await StoreSessionAsync(session);
            }

            return true;
        }

        public async Task<bool> ExtendSessionAsync(string sessionId)
        {
            var session = await GetSessionAsync(sessionId);

            if (session == null || !session.IsActive)
                return false;

            session.LastActivityAt = DateTime.UtcNow;
            session.ExpiresAt = DateTime.UtcNow.AddMinutes(_options.SessionTimeoutMinutes);
            
            await StoreSessionAsync(session);
            return true;
        }

        public async Task<bool> EndSessionAsync(string sessionId)
        {
            var session = await GetSessionAsync(sessionId);

            if (session == null)
                return false;

            // Mark session as inactive
            session.IsActive = false;
            await StoreSessionAsync(session);

            // Log session end
            var context = _httpContextAccessor.HttpContext;
            string ipAddress = context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string userAgent = context?.Request.Headers["User-Agent"].ToString() ?? "unknown";

            await _securityEventService.LogEventAsync(new SecurityEvent
            {
                EventType = "SessionEnded",
                Description = "User session ended",
                Username = session.Username,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Severity = SecurityEventSeverity.Info
            });

            return true;
        }

        public async Task<IEnumerable<SessionInfo>> GetUserSessionsAsync(string userId)
        {
            List<SessionInfo> sessions = new List<SessionInfo>();
            string userSessionsKey = GetUserSessionsKey(userId);
            string sessionsListJson = await _cache.GetStringAsync(userSessionsKey);

            if (string.IsNullOrEmpty(sessionsListJson))
                return sessions;

            List<string> sessionIds = JsonSerializer.Deserialize<List<string>>(sessionsListJson);

            foreach (var sessionId in sessionIds)
            {
                var session = await GetSessionAsync(sessionId);
                if (session != null && session.IsActive && DateTime.UtcNow <= session.ExpiresAt)
                {
                    sessions.Add(session);
                }
            }

            return sessions;
        }

        public async Task<bool> EndAllUserSessionsExceptCurrentAsync(string userId, string currentSessionId)
        {
            string userSessionsKey = GetUserSessionsKey(userId);
            string sessionsListJson = await _cache.GetStringAsync(userSessionsKey);

            if (string.IsNullOrEmpty(sessionsListJson))
                return true;

            List<string> sessionIds = JsonSerializer.Deserialize<List<string>>(sessionsListJson);
            
            foreach (var sessionId in sessionIds)
            {
                if (sessionId != currentSessionId)
                {
                    await EndSessionAsync(sessionId);
                }
            }

            return true;
        }

        private async Task EndAllUserSessionsAsync(string userId)
        {
            string userSessionsKey = GetUserSessionsKey(userId);
            string sessionsListJson = await _cache.GetStringAsync(userSessionsKey);

            if (string.IsNullOrEmpty(sessionsListJson))
                return;

            List<string> sessionIds = JsonSerializer.Deserialize<List<string>>(sessionsListJson);
            
            foreach (var sessionId in sessionIds)
            {
                await EndSessionAsync(sessionId);
            }

            // Clear the user's sessions list
            await _cache.RemoveAsync(userSessionsKey);
        }

        private async Task EnforceMaxConcurrentSessionsAsync(string userId)
        {
            var activeSessions = await GetUserSessionsAsync(userId);
            int activeCount = activeSessions.Count();

            if (activeCount >= _options.MaxConcurrentSessions)
            {
                // End the oldest sessions to make room for the new one
                var sessionsToEnd = activeSessions
                    .OrderBy(s => s.LastActivityAt)
                    .Take(activeCount - _options.MaxConcurrentSessions + 1);

                foreach (var session in sessionsToEnd)
                {
                    await EndSessionAsync(session.SessionId);
                }
            }
        }

        private async Task StoreSessionAsync(SessionInfo session)
        {
            string sessionKey = GetSessionKey(session.SessionId);
            string sessionJson = JsonSerializer.Serialize(session);
            
            // Store with a slightly longer timeout than the session expiration
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = session.ExpiresAt.AddMinutes(5) // Add some buffer for cleanup
            };
            
            await _cache.SetStringAsync(sessionKey, sessionJson, cacheOptions);
        }

        private async Task AddSessionToUserSessionsAsync(string userId, string sessionId)
        {
            string userSessionsKey = GetUserSessionsKey(userId);
            string sessionsListJson = await _cache.GetStringAsync(userSessionsKey);
            
            List<string> sessionIds;
            if (string.IsNullOrEmpty(sessionsListJson))
            {
                sessionIds = new List<string>();
            }
            else
            {
                sessionIds = JsonSerializer.Deserialize<List<string>>(sessionsListJson);
            }
            
            if (!sessionIds.Contains(sessionId))
            {
                sessionIds.Add(sessionId);
            }
            
            var cacheOptions = new DistributedCacheEntryOptions
            {
                // Set a long expiration for the user's sessions list
                AbsoluteExpiration = DateTime.UtcNow.AddDays(30)
            };
            
            await _cache.SetStringAsync(userSessionsKey, JsonSerializer.Serialize(sessionIds), cacheOptions);
        }

        private string GetSessionKey(string sessionId) => $"{SESSION_KEY_PREFIX}{sessionId}";
        private string GetUserSessionsKey(string userId) => $"{USER_SESSIONS_KEY_PREFIX}{userId}";
    }
}
