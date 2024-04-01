using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Acme.BookStore
{
    public class AmazonAwsFeatureDefinitionProvider : IFeatureDefinitionProvider
    {
        readonly IConfiguration _configuration;

        public AmazonAwsFeatureDefinitionProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async IAsyncEnumerable<FeatureDefinition> GetAllFeatureDefinitionsAsync()
        {
            foreach (var section in _configuration.GetChildren().Where(section => section.Key.StartsWith("featuremanagement_")))
            {
                yield return new FeatureDefinition()
                {
                    Name = section.Key.Replace("featuremanagement_", ""),
                    EnabledFor = Convert.ToBoolean(section["enabled"]) ? new List<FeatureFilterConfiguration>() { new FeatureFilterConfiguration() { Name = "AlwaysOn" } } : new List<FeatureFilterConfiguration>(),
                    RequirementType = RequirementType.All
                };
            }
        }

        public async Task<FeatureDefinition> GetFeatureDefinitionAsync(string featureName)
        {
            if (!string.IsNullOrWhiteSpace(_configuration[$"featuremanagement_{featureName}:enabled"]))
            {
                return new FeatureDefinition()
                {
                    Name = featureName,
                    EnabledFor = Convert.ToBoolean(_configuration[$"featuremanagement_{featureName}:enabled"]) ? new List<FeatureFilterConfiguration>() { new FeatureFilterConfiguration() { Name = "AlwaysOn" } } : new List<FeatureFilterConfiguration>(),
                    RequirementType = RequirementType.All
                };
            }
            return null;
        }
    }
}
