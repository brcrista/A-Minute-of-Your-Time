using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json;

namespace FetchPullRequestData
{
    sealed class DataClient
    {
        private readonly GitHttpClient gitClient;
        private readonly DataFileStore outputFileStore;

        private readonly Tracer info;
        private readonly Tracer error;

        public DataClient(
            GitHttpClient gitClient,
            DataFileStore outputFileStore)
        {
            this.gitClient = gitClient;
            this.outputFileStore = outputFileStore;

            info = new Tracer(Console.WriteLine);
            error = new Tracer(Console.Error.WriteLine);
        }

        public async Task<List<GitPullRequest>> FetchPullRequestsAsync(
            string project,
            string repository,
            int count)
        {
            // Get the list of all pull requests
            var pullRequests = await info.TraceOperation(
                $"Fetching {count} pull requests for repository {repository} ...",
                () => gitClient.GetPullRequestsAsync(
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

            return pullRequests;
        }

        public async Task FetchIterationsAsync(
            string project,
            string repository,
            IEnumerable<int> pullRequestIds)
        {
            // Get info for each pull request
            foreach (var id in pullRequestIds)
            {
                var outputFile = $"{id.ToString()}-iterations.json";
                if (outputFileStore.Contains(outputFile))
                {
                    info.Trace($"{outputFile} already exists. Skipping call to the API.");
                }
                else
                {
                    var iterations = await info.TraceOperation(
                        $"Fetching pull request {id.ToString()} ...",
                        () => gitClient.GetPullRequestIterationsAsync(
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