using System;
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

                    var json = JsonConvert.SerializeObject(pullRequests, Formatting.Indented);
                    Console.WriteLine(json);
                },
                errors =>
                {
                    // ParseArguments will already print an error message when failing to parse
                    return Task.CompletedTask;
                });
        }
    }
}