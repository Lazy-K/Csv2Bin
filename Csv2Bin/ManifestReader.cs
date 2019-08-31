using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ManifestReader
{
	public class Reader
	{
		public enum Type
		{
			u8,
			u16,
			u32,
			s32,
			f32,
			utf16,
			sjis,
			Length
		}

		public struct Attribute
		{
			public string fieldName;
			public Type type;
			public int argument;
		}

		public bool Read(string filePath, ref List<Attribute> attributes)
		{
			attributes.Clear();
			try
			{
				using (var reader = new CsvReader(new StreamReader(filePath, Encoding.UTF8)))
				{
					{
						var config = reader.Configuration;
						config.Encoding = Encoding.UTF8;
					}
					if (!reader.Read()) goto Failed; // Read header
					while (true)
					{
						var list = CsvHelperUtil.Util.Read<string>(reader);
						if (0 >= list.Count) break;
						if (2 != list.Count) goto Failed;

						var attr = new Attribute();
						attr.fieldName = list[0];

						{
							var type = 0;
							var typeLength = (int)Type.Length;
							var typeName1 = list[1];
							for (; type < typeLength; ++type)
							{
								var typeName2 = ((Type)type).ToString();
								if (0 != typeName1.IndexOf(typeName2)) continue;
								attr.type = (Type)type;

								if (typeName1.Length != typeName2.Length)
								{
									switch (attr.type)
									{
										case Type.u8:
											{
												if (':' != typeName1[typeName2.Length]) goto Failed;
												var start = typeName2.Length + 1;
												var s = typeName1.Substring(start);
												attr.argument = Int32.Parse(s);
												if (0 >= attr.argument) goto Failed;
												if (9 <= attr.argument) goto Failed;
											}
											break;
										case Type.utf16:
										case Type.sjis:
											{
												if (':' != typeName1[typeName2.Length]) goto Failed;
												var start = typeName2.Length + 1;
												var s = typeName1.Substring(start);
												attr.argument = Int32.Parse(s);
												if (0 >= attr.argument) goto Failed;
											}
											break;
										default:
											goto Failed;
									}
								}
								else
								{
									switch (attr.type)
									{
										case Type.utf16:
										case Type.sjis:
											goto Failed;
										default:
											break;
									}
								}
								break;
							}
							if (typeLength <= type) goto Failed;
						}

						attributes.Add(attr);
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				goto Failed;
			}

			return true;
			Failed:
			attributes.Clear();
			return false;
		}
	}
}
