using CommandLine;

namespace ApiTool
{
    class CommandLineOptions
    {
        [Option("url", Required = true, HelpText = "The URL for the Azure DevOps organization. For example, https://dev.azure.com/mseng.")]
        public string Url { get; set; }

        [Option("project", Required = true, HelpText = "The name of the project where the definition exists.")]
        public string Project { get; set; }

        [Option("repository", Required = true, HelpText = "The name of the repository.")]
        public string Repository { get; set; }

        [Option("pat", HelpText = "A personal access token with the appropriate scopes for the operation (if the project is not public).")]
        public string PersonalAccessToken { get; set; }

        [Option("outdir", HelpText = "The directory where output files will be written. Defaults to 'output'.")]
        public string OutputDirectory { get; set; }

        public static readonly string DefaultOutputDirectory = "output";
    }
}
