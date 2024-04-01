using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Amazon;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Linq;

namespace Acme.BookStore
{
    public class AmazonAwsFeatureFlagConfigurationSource : IConfigurationSource
    {
        public string Environment { get; init; }

        public bool ReloadPeriodically { get; init; }

        public int PeriodInSeconds { get; init; } = 5;

        public IConfigurationProvider Build(IConfigurationBuilder builder) =>
            new AmazonAwsFeatureFlagConfigurationProvider(this);
    }

    public class AmazonAwsFeatureFlagConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private AmazonAwsFeatureFlagConfigurationSource Source { get; }
        private readonly Timer? _timer;
        private Amazon.AppConfigData.AmazonAppConfigDataClient? client;
        private string token;

        public AmazonAwsFeatureFlagConfigurationProvider(AmazonAwsFeatureFlagConfigurationSource source)
        {
            Source = source;

            if (Source.ReloadPeriodically)
            {
                _timer = new Timer
                (
                    callback: ReloadSettings,
                    dueTime: TimeSpan.FromSeconds(5),
                    period: TimeSpan.FromSeconds(Source.PeriodInSeconds),
                    state: null
                );
            }
        }

        public override void Load()
        {
            if (client == null)
            {
                client = new Amazon.AppConfigData.AmazonAppConfigDataClient(RegionEndpoint.EUWest3);
                var response = Task.Run(() => client.StartConfigurationSessionAsync(new Amazon.AppConfigData.Model.StartConfigurationSessionRequest()
                {
                    ApplicationIdentifier = "Acme.BookStore",
                    ConfigurationProfileIdentifier = "Acme.BookStore",
                    EnvironmentIdentifier = Source.Environment
                })).Result;
                token = response.InitialConfigurationToken;
            }
                
            var config = Task.Run(() => client.GetLatestConfigurationAsync(new Amazon.AppConfigData.Model.GetLatestConfigurationRequest() { ConfigurationToken = token })).Result;
            if (config.ContentLength > 0)
            {
                token = config.NextPollConfigurationToken;
                StreamReader sr = new StreamReader(config.Configuration);
                var configStr = sr.ReadToEnd();
                var jsonConfig = JsonSerializer.Deserialize<Dictionary<string, JsonNode>>(configStr);
                Data = jsonConfig.ToDictionary(k => "FeatureManagement:" + k.Key, v => v.Value["enabled"].ToString());
            }
        }

        private void ReloadSettings(object? state)
        {
            Load();
            OnReload();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
