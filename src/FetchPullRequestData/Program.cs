﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace FetchPullRequestData
{
    /// <example>
    /// Run like
    /// <code>
    /// dotnet FetchPullRequestData.dll --url https://dev.azure.com/my-org --project MyProject --pat ***** --repository RepositoryName --count 1000
    /// </code>
    /// </example>
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);

            return await parserResult.MapResult(
                async options =>
                {
                    var restClient = new RestClient(
                        organizationUri: new Uri(options.Url, UriKind.Absolute),
                        pat: options.PersonalAccessToken);

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
            RestClient restClient,
            DataFileStore outputFileStore,
            string project,
            string repository,
            int count)
        {
            Console.WriteLine($"Fetching {count} pull requests for repository {repository} ...");
            var pullRequests = await restClient.GetPullRequestsAsync(
                project,
                repository,
                count);
            Console.WriteLine("Done.");
            Console.WriteLine();

            var outputFile = "pull-requests.json";
            Console.WriteLine($"Writing output to {outputFile} ...");
            await outputFileStore.WriteFileAsync(
                filename: outputFile,
                content: JsonConvert.SerializeObject(pullRequests, Formatting.Indented));
            Console.WriteLine("Done.");
            Console.WriteLine();
        }
    }
}