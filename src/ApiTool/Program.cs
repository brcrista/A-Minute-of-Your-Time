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
                options =>
                {
                    var restClient = new RestClient(
                        organizationUri: new Uri(options.Url, UriKind.Absolute),
                        pat: options.PersonalAccessToken);

                    var outputDirectory = options.OutputDirectory ?? CommandLineOptions.DefaultOutputDirectory;
                    var outputFileStore = new DataFileStore(outputDirectory);

                    return FetchDataFilesAsync(
                        restClient,
                        outputFileStore,
                        project: options.Project,
                        repository: options.Repository);
                },
                errors =>
                {
                    // ParseArguments will already print an error message when failing to parse
                    return Task.CompletedTask;
                });
        }

        private static async Task FetchDataFilesAsync(
            RestClient restClient,
            DataFileStore outputFileStore,
            string project,
            string repository)
        {
            var pullRequests = await restClient.GetPullRequestsAsync(
                project: project,
                repository: repository);

            await outputFileStore.WriteFileAsync(
                filename: "pull-requests.json",
                content: JsonConvert.SerializeObject(pullRequests, Formatting.Indented));
        }
    }
}