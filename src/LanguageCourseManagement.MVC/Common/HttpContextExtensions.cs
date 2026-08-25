using System.Net;

namespace LanguageCourseManagement.MVC.Common;

/// <summary>
/// HttpContext için paylaşılan yardımcı uzantı metotları.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// İstek yapan istemcinin IP adresini döndürür.
    /// X-Forwarded-For başlığını önceliklendirir; yoksa uzak IP kullanır.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',');
            if (ips.Length > 0)
            {
                var originalIp = ips[0].Trim();
                if (IPAddress.TryParse(originalIp, out IPAddress? parsedIp)
                    && parsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return originalIp;
                }
            }
        }

        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }
}
