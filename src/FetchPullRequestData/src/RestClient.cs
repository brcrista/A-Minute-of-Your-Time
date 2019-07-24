using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace FetchPullRequestData
{
    /// <summary>
    /// Manages calls to the Azure DevOps REST API.
    /// </summary>
    sealed class RestClient
    {
        private readonly VssConnection connection;

        public RestClient(Uri organizationUri, string pat = null)
        {
            this.connection = new VssConnection(
                baseUrl: organizationUri,
                credentials: new VssBasicCredential(
                    userName: string.Empty,
                    password: pat ?? string.Empty));
        }

        public Task<List<GitPullRequest>> GetPullRequestsAsync(string project, string repository, int count)
        {
            var searchCriteria = new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Completed,
                TargetRefName = "refs/heads/master"
            };

            var httpClient = connection.GetClient<GitHttpClient>();
            return httpClient.GetPullRequestsAsync(
                project,
                repository,
                searchCriteria,
                top: count);
        }

        public Task<List<GitPullRequestIteration>> GetPullRequestIterationsAsync(string project, string repository, int pullRequestId)
        {
            var httpClient = connection.GetClient<GitHttpClient>();
            return httpClient.GetPullRequestIterationsAsync(
                project,
                repository,
                pullRequestId);
        }
    }
}
