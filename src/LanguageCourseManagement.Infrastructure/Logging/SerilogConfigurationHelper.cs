using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace LanguageCourseManagement.Infrastructure.Logging;

/// <summary>
/// Serilog konfigürasyonı için yardımcı sınıf.
/// ASP.NET Core host için ortak Serilog logger kurulumunu sağlar.
/// </summary>
public static class SerilogConfigurationHelper
{
    public static void Configure(IConfiguration configuration, string applicationName = "LanguageCourseManagement")
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // EF Core ve ASP.NET iç loglarını bastır; gereksiz gürültüyü azaltır
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information) // Uygulama başlatma/durdurma mesajlarını geri aç
            .WriteTo.File(
                path: Path.Combine(Directory.GetCurrentDirectory(), "logs", applicationName + "-.txt"),
                rollingInterval: RollingInterval.Day, // Her gün yeni log dosyası oluşturulur
                retainedFileCountLimit: 31, // 31 günlük (yaklaşık 1 aylık) döngüsel dosya saklama
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
