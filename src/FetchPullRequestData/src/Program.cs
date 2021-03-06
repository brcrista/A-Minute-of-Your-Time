﻿using System;
using System.Linq;
using System.Threading.Tasks;

using CommandLine;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

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

                    var outputDirectory = options.OutputDirectory ?? CommandLineOptions.DefaultOutputDirectory;

                    var dataClient = new DataClient(
                        gitClient: connection.GetClient<GitHttpClient>(),
                        outputFileStore: new DataFileStore(outputDirectory));

                    await FetchDataFilesAsync(
                        dataClient,
                        project: options.Project,
                        repositoryId: options.Repository,
                        count: options.Count);

                    return 0;
                },
                errors =>
                {
                    // ParseArguments will already print an error message when failing to parse
                    return Task.FromResult(1);
                });
        }

        static async Task FetchDataFilesAsync(
            DataClient dataClient,
            string project,
            string repositoryId,
            int count)
        {
            // Fetch the list of PRs
            var pullRequests = await dataClient.FetchPullRequestsAsync(
                project,
                repositoryId,
                count);

            // Fetch additional information for each PR
            foreach (var pr in pullRequests)
            {
                await dataClient.FetchIterationsAsync(
                    project,
                    repositoryId,
                    pr);

                await dataClient.FetchPullRequestChangesAsync(
                    project,
                    repositoryId,
                    pr);
            }
        }
    }
}