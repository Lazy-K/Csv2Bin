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
			}

			var manifestHeader = new ManifestXmlReader.Header();
			var manifestContents = new List<ManifestXmlReader.Content>();
			{ // Read manifest file
				if (!ManifestXmlReader.Reader.Read(
					_commandLineOption.manifestFilePath,
					ref manifestHeader,
					ref manifestContents,
					_logFile))
				{
					goto Failed;
				}
			}

			if (null != _commandLineOption.tableFilePath && null != _commandLineOption.outputBinaryFilePath)
			{
				var binary = new List<byte>();
				UInt32 numRecords = 0;
				{ // Read table file and convert binary by manifest
					if (!TableReader.Reader.Read(
						_commandLineOption.tableFilePath,
						manifestContents,
						ref binary,
						ref numRecords,
						_logFile))
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
						if (null != _logFile) _logFile.Write("Output Binary File Error: \"{0}\"\n", e.ToString());
						goto Failed;
					}
				}
			}

			if(null != _commandLineOption.outputCsFilePath)
			{
				// Generate code file
				try
				{
					var code = ManifestXmlReader.Util.GenerateCode(
						ref manifestHeader,
						ref manifestContents);
					File.WriteAllText(_commandLineOption.outputCsFilePath, code);
				}
				catch (Exception e)
				{
					if (null != _logFile) _logFile.Write("Output CShape File Error: \"{0}\"\n", e.ToString());
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
			_logFile.Close();
			if (!File.Exists(_commandLineOption.outputLogFilePath)) return;
			var fileInfo = new FileInfo(_commandLineOption.outputLogFilePath);
			if (0 != fileInfo.Length) return;
			File.Delete(_commandLineOption.outputLogFilePath);
		}
	}
}
