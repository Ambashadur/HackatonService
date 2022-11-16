using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HackatonService.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HackatonService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _pathToFile;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _pathToFile = configuration["File"];
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var pcs = Process.GetProcesses();
                foreach (var pc in pcs)
                {
                    if (pc.ProcessName.ToLower() == "idle") continue;
                    
                    var process = new ProcessEntity
                    {
                        Id = pc.Id,
                        Name = pc.ProcessName,
                        MachineName = pc.MachineName,
                        // StartTime = pc.StartTime,
                        // ExitTime = pc.ExitTime,
                        MainWindowTitle = pc.MainWindowTitle,
                        TotalProcessorTime = pc.TotalProcessorTime
                    };

                    WriteToFile(process);
                }
            
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void WriteToFile<T>(T entity) where T : class
        {
            if (!File.Exists(_pathToFile))
            {
                File.Create(_pathToFile);
            }
            
            var streamOptions = new FileStreamOptions
            {
                Mode = FileMode.Append,
                Access = FileAccess.Write
            };

            using var streamWriter = new StreamWriter(_pathToFile, streamOptions);
            streamWriter.Write(JsonSerializer.Serialize(entity, _jsonOptions));
        }
    }
}