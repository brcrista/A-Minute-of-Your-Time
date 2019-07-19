using System;
using System.Threading.Tasks;
using CommandLine;

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

                    Console.WriteLine(pullRequests);
                },
                errors =>
                {
                    // ParseArguments will already print an error message when failing to parse
                    return Task.CompletedTask;
                });
        }
    }
}