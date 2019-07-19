using System;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

using Newtonsoft.Json;

namespace ApiTool
{
    class RestClient
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

        public async Task<string> GetPullRequestsAsync(string project, string repository)
        {
            var searchCriteria = new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Completed
            };

            var httpClient = connection.GetClient<GitHttpClient>();
            var pullRequests = await httpClient.GetPullRequestsAsync(
                project,
                repository,
                searchCriteria,
                top: 1000);

            return JsonConvert.SerializeObject(pullRequests, Formatting.Indented);
        }
    }
}
