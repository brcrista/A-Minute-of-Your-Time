using System;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;

namespace ApiTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);

            await parserResult.MapResult(
                async options =>
                {
                    var client = new RestClient(
                        organizationUri: new Uri(options.Url, UriKind.Absolute),
                        pat: options.PersonalAccessToken);

                    var pullRequests = await client.GetPullRequestsAsync(
                        project: options.Project,
                        repository: options.Repository);

                    var outputDirectory = options.OutputDirectory ?? CommandLineOptions.DefaultOutputDirectory;
                    var outputFileStore = new DataFileStore(outputDirectory);
                    outputFileStore.WriteFile(
                        filename: "pull-requests/list.json",
                        content: JsonConvert.SerializeObject(pullRequests, Formatting.Indented));

                    var pullRequestIds = from pr in pullRequests
                        where !outputFileStore.Contains(pr.PullRequestId.ToString())
                        select pr.PullRequestId;

                    foreach (var pullRequestId in pullRequestIds)
                    {
                        // TODO
                        Console.WriteLine($"Fetched new PR {pullRequestId}");
                    }
                },
                errors =>
                {
                    // ParseArguments will already print an error message when failing to parse
                    return Task.CompletedTask;
                });
        }
    }
}