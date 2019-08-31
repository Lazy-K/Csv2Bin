using System;
using System.Collections.Generic;
using System.IO;

namespace Csv2Bin
{
	class Program
	{
		private static AppCommandLine.Option _commandLineOption;

		static int Main(string[] args)
		{
			if (!AppCommandLine.Parser.Parse(args, ref _commandLineOption))
			{
				return 1;
			}

			var manifestAtts = new List<ManifestReader.Reader.Attribute>();
			{ // Read manifest file
				var manifestReader = new ManifestReader.Reader();
				if (!manifestReader.Read(_commandLineOption.manifestFilePath, ref manifestAtts))
				{
					return 1;
				}
			}

			var binary = new List<byte>();
            UInt32 numRecords = 0;
			{ // Read table file and convert binary by manifest
				var tableReader = new TableReader.Reader();
				if (!tableReader.Read(_commandLineOption.tableFilePath, manifestAtts, ref binary, ref numRecords))
				{
					return 1;
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
					Console.WriteLine(e);
					return 1;
				}
			}

			return 0;
		}
	}
}
