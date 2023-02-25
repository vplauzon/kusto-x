using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kustox.CosmosDbState;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kustox.IntegratedTests
{
    public abstract class TestBase
    {
        #region Inner types
        private class ProjectSetting
        {
            public IDictionary<string, string>? EnvironmentVariables { get; set; }
        }

        private class MainSettings
        {
            public IDictionary<string, ProjectSetting>? Profiles { get; set; }

            public IDictionary<string, string> GetEnvironmentVariables()
            {
                if (Profiles == null)
                {
                    throw new InvalidOperationException("'profiles' element isn't present in 'launchSettings.json'");
                }
                if (Profiles.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No profile is configured within 'profiles' element isn't present "
                        + "in 'launchSettings.json'");
                }
                var profile = Profiles.First().Value;

                if (profile.EnvironmentVariables == null)
                {
                    throw new InvalidOperationException("'environmentVariables' element isn't present in 'launchSettings.json'");
                }

                return profile.EnvironmentVariables;
            }
        }
        #endregion

        private static readonly Random _random = new Random();

        static TestBase()
        {
            ReadEnvironmentVariables();

            var builder = CreateKustoBuilder();

            ControlFlowList = ControlFlowPersistencyFactory.FromEnvironmentVariables();
            QueryProvider = KustoClientFactory.CreateCslQueryProvider(builder);
            CommandProvider = KustoClientFactory.CreateCslAdminProvider(builder);
        }

        #region Environment variables
        private static void ReadEnvironmentVariables()
        {
            const string PATH = "Properties\\launchSettings.json";

            if (File.Exists(PATH))
            {
                var settingContent = File.ReadAllText(PATH);
                var mainSetting = JsonSerializer.Deserialize<MainSettings>(
                    settingContent,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (mainSetting == null)
                {
                    throw new InvalidOperationException("Can't read 'launchSettings.json'");
                }

                var variables = mainSetting.GetEnvironmentVariables();

                foreach (var variable in variables)
                {
                    Environment.SetEnvironmentVariable(variable.Key, variable.Value);
                }
            }
        }

        private static string GetEnvironmentVariable(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);

            if (value == null)
            {
                throw new InvalidOperationException(
                    $"Environment variable '{variableName}' missing");
            }

            return value;
        }
        #endregion

        #region IControlFlowList
        protected static IControlFlowList ControlFlowList { get; }

        protected static IControlFlowInstance CreateControlFlowInstance()
        {
            var i1 = _random.Next();
            var i2 = _random.Next();
            var jobId = ((long)i1 << 32) | (long)i2;

            return ControlFlowList.GetInstance(jobId);
        }
        #endregion

        #region Kusto
        protected static ICslQueryProvider QueryProvider { get; }

        protected static ICslAdminProvider CommandProvider { get; }

        private static KustoConnectionStringBuilder CreateKustoBuilder()
        {
            var kustoCluster = GetEnvironmentVariable("kustoCluster");
            var kustoDb = GetEnvironmentVariable("kustoDb");
            var kustoTenantId = GetEnvironmentVariable("kustoTenantId");
            var kustoClientId = GetEnvironmentVariable("kustoClientId");
            var kustoClientKey = GetEnvironmentVariable("kustoClientKey");

            return new KustoConnectionStringBuilder(kustoCluster, kustoDb)
                .WithAadApplicationKeyAuthentication(kustoClientId, kustoClientKey, kustoTenantId);
        }
        #endregion

        protected static async Task RunInPiecesAsync(
            IControlFlowInstance flowInstance,
            int? maximumNumberOfSteps = 1)
        {
            var done = false;

            while (!done)
            {
                var runtime =
                    new ControlFlowRuntime(flowInstance, QueryProvider, CommandProvider);

                done = await runtime.RunAsync(maximumNumberOfSteps);
            }
        }
    }
}