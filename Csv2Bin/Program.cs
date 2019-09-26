using System;
using System.Collections.Generic;
using System.IO;

namespace Csv2Bin
{
	class Program
	{
		private static AppCommandLine.Option _commandLineOption;
		private static StreamWriter _logFile = null;

		static int Main(string[] args)
		{
			if (!AppCommandLine.Parser.Parse(args, ref _commandLineOption))
			{
				return 1;
			}

			if (null != _commandLineOption.outputLogFilePath)
			{
				if (File.Exists(_commandLineOption.outputLogFilePath))
				{
					File.Delete(_commandLineOption.outputLogFilePath);
				}
				_logFile = File.CreateText(_commandLineOption.outputLogFilePath);
				Console.SetOut(_logFile);
			}

			var manifestHeader = new ManifestHeader();
			var manifestContents = new List<ManifestContent>();
			{ // Read manifest file
				if (!Manifest.Read(
					_commandLineOption.manifestFilePath,
					ref manifestHeader,
					ref manifestContents))
				{
					goto Failed;
				}
			}

#if false // TEST
			{ // Write manifest file
				if (!Manifest.Write(
					_commandLineOption.manifestFilePath + "_test.xml",
					manifestHeader,
					manifestContents))
				{
					goto Failed;
				}

			}
			return 0;
#endif

			if (null != _commandLineOption.tableFilePath && null != _commandLineOption.outputBinaryFilePath)
			{
				List<byte> binary;
				UInt32 numRecords;
				{ // Read table file and convert binary by manifest
					if (!Manifest.GenerateBinary(
						_commandLineOption.tableFilePath,
						manifestContents,
						out binary,
						out numRecords))
					{
						goto Failed;
					}
				}

				{ // Write binary file
					try
					{
						using (var writer = new BinaryWriter(new FileStream(_commandLineOption.outputBinaryFilePath, FileMode.Create)))
						{
							writer.Write(binary.ToArray());
							if (_commandLineOption.isAppendSummary)
							{ // Append summary
								UInt32 size = (UInt32)binary.Count / numRecords;
								writer.Write(size);
								writer.Write(numRecords);
							}
						}
					}
					catch (Exception e)
					{
						Console.WriteLine("Output Binary File Error: \"{0}\"", e.ToString());
						goto Failed;
					}
				}
			}

			if(null != _commandLineOption.outputCsFilePath)
			{
				// Generate code file
				try
				{
					var code = Manifest.GenerateCode(
						manifestHeader,
						manifestContents);
					File.WriteAllText(_commandLineOption.outputCsFilePath, code);
				}
				catch (Exception e)
				{
					Console.WriteLine("Output CShape File Error: \"{0}\"", e.ToString());
					goto Failed;
				}
			}

			FinalizeLogFile();
			return 0;
			Failed:
			FinalizeLogFile();
			return 1;
		}

		private static void FinalizeLogFile()
		{
			if (null == _logFile) return;
			_logFile.Dispose();
			if (!File.Exists(_commandLineOption.outputLogFilePath)) return;
			var fileInfo = new FileInfo(_commandLineOption.outputLogFilePath);
			if (0 != fileInfo.Length) return;
			File.Delete(_commandLineOption.outputLogFilePath);
		}
	}
}
