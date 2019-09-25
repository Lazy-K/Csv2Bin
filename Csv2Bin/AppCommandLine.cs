using CommandLine;

namespace AppCommandLine
{
	public class Option
	{
		[Option("table", Required = true, HelpText = "Set input table file path.")]
		public string tableFilePath { get; set; }
		[Option("manifest", Required = true, HelpText = "Set manifest file path.")]
		public string manifestFilePath { get; set; }
		[Option("out", Required = true, HelpText = "Set output binary file path.")]
		public string outputBinaryFilePath { get; set; }
		[Option("appendSummary", Required = false, HelpText = "Set append summary enabled.")]
		public bool isAppendSummary { get; set; }
		[Option("outcs", Required = false, HelpText = "Set output cs file path.")]
		public string outputCsFilePath { get; set; }
		[Option("log", Required = false, HelpText = "Set output log file path.")]
		public string outputLogFilePath { get; set; }
	}

	public class Parser
	{
		public static bool Parse(string[] args, ref Option option)
		{
			var _option = new Option();
			var result = CommandLine.Parser.Default.ParseArguments<Option>(args)
				.WithParsed<Option>(o => { _option = o; });
			option = _option;
			return ParserResultType.Parsed == result.Tag;
		}
	}
}
