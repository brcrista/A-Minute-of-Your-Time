using System;
using System.Linq;
using System.Threading.Tasks;

using CommandLine;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;

namespace FetchPullRequestData
{
    /// <example>
    /// Run like
    /// <code>
    /// dotnet FetchPullRequestData.dll --url https://dev.azure.com/my-org --project MyProject --pat ***** --repository RepositoryName --count 1000
    /// </code>
    /// </example>
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);

            return await parserResult.MapResult(
                async options =>
                {
                    var connection = new VssConnection(
                        baseUrl: new Uri(options.Url, UriKind.Absolute),
                        credentials: new VssBasicCredential(
                            userName: string.Empty,
                            password: options.PersonalAccessToken ?? string.Empty));

                    var restClient = connection.GetClient<GitHttpClient>();

                    var outputDirectory = options.OutputDirectory ?? CommandLineOptions.DefaultOutputDirectory;
                    var outputFileStore = new DataFileStore(outputDirectory);

                    await FetchDataFilesAsync(
                        restClient,
                        outputFileStore,
                        project: options.Project,
                        repository: options.Repository,
                        count: options.Count);

                    return 0;
                },
                errors =>
                {
                    // ParseArguments will already print an error message when failing to parse
                    return Task.FromResult(1);
                });
        }

        private static async Task FetchDataFilesAsync(
            GitHttpClient restClient,
            DataFileStore outputFileStore,
            string project,
            string repository,
            int count)
        {
            var info = new Tracer(Console.WriteLine);
            var error = new Tracer(Console.Error.WriteLine);

            // Get the list of all pull requests
            var pullRequests = await info.TraceOperation(
                $"Fetching {count} pull requests for repository {repository} ...",
                () => restClient.GetPullRequestsAsync(
                    project,
                    repository,
                    searchCriteria: new GitPullRequestSearchCriteria
                    {
                        Status = PullRequestStatus.Completed,
                        TargetRefName = "refs/heads/master"
                    },
                    top: count));

            if (pullRequests.Count < count)
            {
                error.Trace($"Requested {count} pull requests, but only {pullRequests.Count} were received.");
            }

            var outputFile = "pull-requests.json";
            await info.TraceOperation(
                $"Writing output to {outputFile} ...",
                () => outputFileStore.WriteFileAsync(
                    filename: outputFile,
                    content: JsonConvert.SerializeObject(pullRequests, Formatting.Indented)));

            // Get info for each pull request
            foreach (var id in pullRequests.Select(x => x.PullRequestId))
            {
                outputFile = $"{id.ToString()}-iterations.json";
                if (outputFileStore.Contains(outputFile))
                {
                    info.Trace($"{outputFile} already exists. Skipping call to the API.");
                }
                else
                {
                    var iterations = await info.TraceOperation(
                        $"Fetching pull request {id.ToString()} ...",
                        () => restClient.GetPullRequestIterationsAsync(
                            project,
                            repository,
                            id));

                    await info.TraceOperation(
                        $"Writing output to {outputFile} ...",
                        () => outputFileStore.WriteFileAsync(
                            filename: outputFile,
                            content: JsonConvert.SerializeObject(iterations, Formatting.Indented)));
                }
            }
        }
    }
}