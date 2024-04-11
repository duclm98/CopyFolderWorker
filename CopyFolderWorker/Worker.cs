namespace CopyFolderWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting copy operation at: {time}", DateTimeOffset.Now);

            var sourceDirectory = _configuration["Path:SourceDirectory"];
            var destinationDirectory = _configuration["Path:DestinationDirectory"];
            if (sourceDirectory != null && destinationDirectory != null)
                await CheckAndUpdateDirectoryAsync(sourceDirectory, destinationDirectory);

            _logger.LogInformation("Copy operation completed at: {time}", DateTimeOffset.Now);

            await Task.Delay(int.Parse(_configuration["Delay"] ?? "600") * 1000, stoppingToken);
        }
    }

    private async Task CheckAndUpdateDirectoryAsync(string sourceDir, string targetDir)
    {
        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(targetDir, Path.GetFileName(file));
            var destFileInfo = new FileInfo(destFile);

            if (!File.Exists(destFile) || new FileInfo(file).LastWriteTime > destFileInfo.LastWriteTime)
            {
                await using var sourceStream = File.Open(file, FileMode.Open);
                await using var destinationStream = File.Create(destFile);
                await sourceStream.CopyToAsync(destinationStream);
            }
        }

        // Đệ quy copy thư mục con
        foreach (var subdir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(targetDir, Path.GetFileName(subdir));
            await CheckAndUpdateDirectoryAsync(subdir, destSubDir);
        }
    }
}