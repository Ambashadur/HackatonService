using HackatonService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = "HackatonService";
    })
    .Build();

await host.RunAsync();