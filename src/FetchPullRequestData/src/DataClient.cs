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
            info.Trace($"Fetching {count} pull requests for repository {repositoryId} ...");
            var pullRequests = await gitClient.GetPullRequestsAsync(
                project,
                repositoryId,
                searchCriteria: new GitPullRequestSearchCriteria
                {
                    Status = PullRequestStatus.Completed,
                    TargetRefName = "refs/heads/master"
                },
                top: count);

            if (pullRequests.Count < count)
            {
                error.Trace($"Requested {count} pull requests, but only {pullRequests.Count} were received.");
            }

            var outputFile = "pull-requests.json";
            info.Trace($"Writing output to {outputFile} ...");
            await outputFileStore.WriteFileAsync(
                filename: outputFile,
                content: JsonConvert.SerializeObject(pullRequests, Formatting.Indented));

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
                info.Trace($"Fetching iterations for pull request {pullRequestId.ToString()} ...");
                var iterations = await gitClient.GetPullRequestIterationsAsync(
                    project,
                    repositoryId,
                    pullRequestId);

                info.Trace($"Writing output to {outputFile} ...");
                await outputFileStore.WriteFileAsync(
                    filename: outputFile,
                    content: JsonConvert.SerializeObject(iterations, Formatting.Indented));
            }
        }

        public async Task FetchPullRequestChangesAsync(
            string project,
            string repositoryId,
            GitPullRequest pullRequest)
        {
            var pullRequestId = pullRequest.PullRequestId;
            var outputFile = $"{pullRequestId.ToString()}-changes.json";
            var mergeCommitId = pullRequest.LastMergeCommit?.CommitId;

            if (mergeCommitId == null)
            {
                // A completed PR won't merge if the commit is already in the target branch
                info.Trace($"{pullRequestId} has no merge commit.");
            }
            else if (outputFileStore.Contains(outputFile))
            {
                info.Trace($"{outputFile} already exists. Skipping call to the API.");
            }
            else
            {
                info.Trace($"Fetching changes for commit {mergeCommitId.ToString()} ...");
                var changes = await gitClient.GetChangesAsync(
                    project,
                    commitId: mergeCommitId,
                    repositoryId: repositoryId);

                info.Trace($"Writing output to {outputFile} ...");
                await outputFileStore.WriteFileAsync(
                    filename: outputFile,
                    content: JsonConvert.SerializeObject(changes, Formatting.Indented));
            }
        }
    }
}