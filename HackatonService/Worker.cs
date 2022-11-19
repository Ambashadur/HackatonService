using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dnYara;
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

        private string[] _samples = new[]
        {
            @"C:\Users\Home\Documents\hakaton\MalwareDirectory" 
        };

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
                _logger.LogInformation("Start scanning...");

                string[] ruleFiles = Directory.GetFiles(@".\yara-rules\", "*.yara", SearchOption.AllDirectories).ToArray();

                using (var context = new YaraContext())
                {
                    CompiledRules rules = null;
                    using (var compiler = new Compiler())
                    {
                        foreach (var yara in ruleFiles)
                        {
                            compiler.AddRuleFile(yara);
                        }

                        rules = compiler.Compile();
                    }

                    if (rules != null)
                    {
                        var scanner = new Scanner();

                        foreach (var sample in _samples)
                        {
                            if (File.Exists(sample))
                            {
                                ScanFile(scanner, sample, rules);
                            }
                            else
                            {
                                if (Directory.Exists(sample))
                                {
                                    DirectoryInfo dirInfo = new DirectoryInfo(sample);

                                    foreach (FileInfo fi in dirInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                                    {
                                        ScanFile(scanner, fi.FullName, rules);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No rules was provided!");
                    }

                    await Task.Delay(10000);
                }
            }
        }

        private void WriteToFile<T>(T entity, FileMode mode = FileMode.OpenOrCreate) where T : class
        {
            if (!File.Exists(_pathToFile))
            {
                File.Create(_pathToFile);
            }
            
            var streamOptions = new FileStreamOptions
            {
                Mode = mode,
                Access = FileAccess.Write
            };

            using var streamWriter = new StreamWriter(_pathToFile, streamOptions);
            streamWriter.Write(JsonSerializer.Serialize(entity, _jsonOptions));
        }

        private void ScanFile(Scanner scanner, string filename, CompiledRules rules)
        {
            List<ScanResult> scanResults = scanner.ScanFile(filename, rules);
            var yaraMatches = new List<YaraMatchEntity>();

            foreach (ScanResult scanResult in scanResults)
            {
                string id = scanResult.MatchingRule.Identifier;

                foreach (var match in scanResult.Matches)
                {
                    yaraMatches.Add(new YaraMatchEntity 
                    {
                        PathToFile = filename,
                        Rule = id,
                        RuleKey = match.Key,
                    });
                }
            }

            WriteToFile(yaraMatches, FileMode.Append);
            _logger.LogInformation($"Find {scanResults.Count()} results");
        }
    }
}