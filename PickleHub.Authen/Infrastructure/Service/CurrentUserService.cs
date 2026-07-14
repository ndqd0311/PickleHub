using PickleHub.Common.Exceptions;
using PickleHub.Common.Interfaces;
using System.Security.Claims;

namespace PickleHub.Authen.Infrastructure.Service
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                // Ưu tiên header từ API Gateway
                var headerValue = _httpContextAccessor.HttpContext?
                    .Request.Headers["X-User-Id"]
                    .FirstOrDefault();

                if (Guid.TryParse(headerValue, out var headerId))
                    return headerId;

                // Fallback: Đọc từ JWT claims (khi test trực tiếp)
                var claimValue = _httpContextAccessor.HttpContext?.User
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(claimValue, out var claimId))
                    return claimId;

                throw new UnauthorizedException("Không xác định được người dùng.");
            }
        }

        public string Role
        {
            get
            {
                // Ưu tiên header từ API Gateway
                var headerValue = _httpContextAccessor.HttpContext?
                    .Request.Headers["X-User-Role"]
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(headerValue))
                    return headerValue;

                // Fallback: Đọc từ JWT claims (khi test trực tiếp)
                return _httpContextAccessor.HttpContext?.User
                    .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                // Kiểm tra header từ Gateway
                var headerUserId = _httpContextAccessor.HttpContext?
                    .Request.Headers["X-User-Id"]
                    .FirstOrDefault();
                
                var hasHeader = Guid.TryParse(headerUserId, out _);
                
                if (hasHeader)
                {
                    Console.WriteLine($"✓ Authenticated via Header: X-User-Id={headerUserId}");
                    return true;
                }

                // Fallback: Kiểm tra JWT claims
                var claimUserId = _httpContextAccessor.HttpContext?.User
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                var hasClaim = Guid.TryParse(claimUserId, out _);
                
                Console.WriteLine($"JWT Claim 'NameIdentifier': {claimUserId ?? "NULL"}");
                Console.WriteLine($"User Claims: {string.Join(", ", _httpContextAccessor.HttpContext?.User.Claims.Select(c => $"{c.Type}={c.Value}") ?? new[]{"NONE"})}");
                
                return hasClaim;
            }
        }
    }
}
