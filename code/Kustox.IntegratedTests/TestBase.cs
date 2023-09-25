using Azure.Identity;
using Kustox.Compiler;
using Kustox.KustoState;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        private static readonly KustoxCompiler _compiler = new KustoxCompiler();

        static TestBase()
        {
            var testId = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffff");

            ReadEnvironmentVariables();

            SampleRootUrl = GetEnvironmentVariable("sampleRootUrl");
            StorageHub = new KustoStorageHub(CreateConnectionProvider(false));
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

        private static ClientSecretCredential CreateTestCredentials()
        {
            var tenantId = GetEnvironmentVariable("tenantId");
            var appId = GetEnvironmentVariable("appId");
            var appKey = GetEnvironmentVariable("appKey");
            var credential = new ClientSecretCredential(tenantId, appId, appKey);

            return credential;
        }
        #endregion

        #region IControlFlowList
        protected static IStorageHub StorageHub { get; }
        #endregion

        #region Kusto
        private static ConnectionProvider CreateConnectionProvider(bool isSandbox)
        {
            var kustoCluster = GetEnvironmentVariable("kustoCluster");
            var dbVariable = isSandbox
                ? "kustoDb-sandbox"
                : "kustoDb-state";
            var kustoDb = GetEnvironmentVariable(dbVariable);
            ClientSecretCredential credential = CreateTestCredentials();

            return new ConnectionProvider(new Uri(kustoCluster), kustoDb, credential);
        }
        #endregion

        #region Storage
        protected static string SampleRootUrl { get; }
        #endregion

        protected static async Task<TableResult?> RunInPiecesAsync(
            string script,
            int? maximumNumberOfSteps = 1,
            CancellationToken ct = default(CancellationToken))
        {
            var connectionProvider = CreateConnectionProvider(true);
            var environmentRuntime = new ProcedureEnvironmentRuntime(
                StorageHub.ProcedureRunStore,
                StorageHub.ProcedureRunRegistry,
                connectionProvider);

            await environmentRuntime.StartAsync(false, ct);

            var procedureQueue = (IProcedureQueue)environmentRuntime;
            var procedureDeclaration = _compiler.CompileProcedure(script);

            if(procedureDeclaration == null)
            {
                throw new InvalidOperationException($"Can't compile '{script}'");
            }

            var procedureRunStepStore = await procedureQueue.QueueProcedureAsync(
                procedureDeclaration,
                false,
                ct);

            while (true)
            {
                var runtime = new ProcedureRuntime(
                    _compiler,
                    StorageHub.ProcedureRunStore,
                    procedureRunStepStore,
                    environmentRuntime.RunnableRuntime);
                var result = await runtime.RunAsync(maximumNumberOfSteps);

                if (result.HasCompleteSuccessfully)
                {
                    await environmentRuntime.StopAsync(ct);

                    return result.Result;
                }
            }
        }
    }
}