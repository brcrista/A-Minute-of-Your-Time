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
            string repositoryId,
            int count)
        {
            // Get the list of all pull requests
            var pullRequests = await info.TraceOperation(
                $"Fetching {count} pull requests for repository {repositoryId} ...",
                () => gitClient.GetPullRequestsAsync(
                    project,
                    repositoryId,
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
            string repositoryId,
            GitPullRequest pullRequest)
        {
            var pullRequestId = pullRequest.PullRequestId;
            var outputFile = $"{pullRequestId.ToString()}-iterations.json";

            if (outputFileStore.Contains(outputFile))
            {
                info.Trace($"{outputFile} already exists. Skipping call to the API.");
            }
            else
            {
                var iterations = await info.TraceOperation(
                    $"Fetching iterations for pull request {pullRequestId.ToString()} ...",
                    () => gitClient.GetPullRequestIterationsAsync(
                        project,
                        repositoryId,
                        pullRequestId));

                await info.TraceOperation(
                    $"Writing output to {outputFile} ...",
                    () => outputFileStore.WriteFileAsync(
                        filename: outputFile,
                        content: JsonConvert.SerializeObject(iterations, Formatting.Indented)));
            }
        }

        public async Task FetchPullRequestChangesAsync(
            string project,
            string repositoryId,
            GitPullRequest pullRequest)
        {
            var pullRequestId = pullRequest.PullRequestId;
            var outputFile = $"{pullRequestId.ToString()}-changes.json";

            if (outputFileStore.Contains(outputFile))
            {
                info.Trace($"{outputFile} already exists. Skipping call to the API.");
            }
            else
            {
                var mergeCommitId = pullRequest.LastMergeCommit.CommitId;
                var changes = await info.TraceOperation(
                    $"Fetching changes for commit {mergeCommitId.ToString()} ...",
                    () => gitClient.GetChangesAsync(
                        project,
                        commitId: mergeCommitId,
                        repositoryId: repositoryId));

                    await info.TraceOperation(
                        $"Writing output to {outputFile} ...",
                        () => outputFileStore.WriteFileAsync(
                            filename: outputFile,
                            content: JsonConvert.SerializeObject(changes, Formatting.Indented)));
            }
        }
    }
}