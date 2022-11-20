using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dnYara;
using HackatonService.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Net.NetworkInformation;

namespace HackatonService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private string _macAddress;

        private string[] _samples = new[]
        {
            @"C:\Users\Home\Documents\hakaton\MalwareDirectory" 
        };

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _macAddress = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault();

            _logger.LogInformation($"Mac address: {_macAddress}");

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

        private async void SentTCP<T>(T entity) where T : class
        {
            using var client = new TcpClient();
            try
            {
                await client.ConnectAsync(IPAddress.Parse("192.168.0.103"), 8888);
                var stream = client.GetStream();
                var stringEntity = JsonSerializer.Serialize(entity, _jsonOptions);
                await stream.WriteAsync(Encoding.Default.GetBytes(stringEntity));

                client.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
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
                        MacAddress = _macAddress,
                    });
                }
            }

            SentTCP(yaraMatches);
            _logger.LogInformation($"Find {scanResults.Count()} results");
        }
    }
}