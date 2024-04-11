using CopyFolderWorker;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Copy Folder Worker Service";
    })
    .ConfigureServices(services =>
    {
        services.AddLogging(logging =>
        {
            logging.AddProvider(new FileLoggerProvider("Logging"));
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();