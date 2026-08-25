using Microsoft.Extensions.Configuration;
using Serilog;

namespace LanguageCourseManagement.Infrastructure.Logging;

public class FileLogger : LoggerServiceBase
{
    public FileLogger(IConfiguration configuration)
    {
        FileLogConfiguration logConfig =
            configuration.GetSection("SeriLogConfigurations:FileLogConfiguration").Get<FileLogConfiguration>()
            ?? new FileLogConfiguration { FolderPath = "/logs/gym-manager" };

        string logFilePath = string.Format("{0}{1}",
            Directory.GetCurrentDirectory() + logConfig.FolderPath, ".txt");

        Logger = new LoggerConfiguration()
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31, // Yaklaşık 1 aylık (31 günlük) log geçmişi tutulur; eski dosyalar silinir
                fileSizeLimitBytes: 5000000, // 5 MB - tek bir log dosyasının disk kullanımını sınırla
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
    }
}
