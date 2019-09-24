using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TableReader
{
	public class Reader
	{
		public bool Read(string filePath, List<ManifestXmlReader.Content> manifestContents, ref List<byte> dest, ref UInt32 numRecords)
		{
			numRecords = 0;
			dest.Clear();
			try
			{
				using (var reader = new CsvReader(new StreamReader(filePath, Encoding.UTF8)))
				{
					{
						var config = reader.Configuration;
						config.Encoding = Encoding.UTF8;
					}
					if (!reader.Read()) goto Failed;
					if (!reader.ReadHeader()) goto Failed;

					while (reader.Read())
					{
						List<byte> binary = new List<byte>();

						var bitflagsProcessing = false;
						Int32 bitflags = 0;
						var bitflagsShift = 0;
						foreach (ManifestXmlReader.Content content in manifestContents)
						{
							var field = String.Empty;
							if (content.valueName != string.Empty)
							{
								var index = reader.GetFieldIndex(content.valueName);
								field = reader.GetField(index);
							}

							if (ManifestXmlReader.ValueType.bits32 != content.valueType)
							{
								if (bitflagsProcessing)
								{
									bitflagsProcessing = false;
									binary.AddRange(BitConverter.GetBytes(bitflags));
								}
							}

							switch (content.valueType)
							{
								//---------------------------------------
								// Primal Type
								case ManifestXmlReader.ValueType.s8:
									{
										sbyte value = 0;
										if (content.valueName != string.Empty)
										{
											if (!sbyte.TryParse(field, out value)) goto Failed;
										}
										binary.Add((byte)value);
									}
									break;
								case ManifestXmlReader.ValueType.u8:
									{
										byte value = 0;
										if (content.valueName != string.Empty)
										{
											if (!byte.TryParse(field, out value)) goto Failed;
										}
										binary.Add(value);
									}
									break;
								case ManifestXmlReader.ValueType.s16:
									{
										Int16 value = 0;
										if (content.valueName != string.Empty)
										{
											if (!Int16.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestXmlReader.ValueType.u16:
									{
										UInt16 value = 0;
										if (content.valueName != string.Empty)
										{
											if (!UInt16.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestXmlReader.ValueType.s32:
									{
										Int32 value = 0;
										if (content.valueName != string.Empty)
										{
											if (!Int32.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestXmlReader.ValueType.u32:
									{
										UInt32 value = 0;
										if (content.valueName != string.Empty)
										{
											if (!UInt32.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestXmlReader.ValueType.f32:
									{
										float value = 0;
										if (content.valueName != string.Empty)
										{
											if (!float.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;

								//---------------------------------------
								// String Type
								case ManifestXmlReader.ValueType.utf16:
									{
										var value = field;
										for (var i = 0; i < content.length; ++i)
										{
											if (value.Length > i && i < content.length - 1)
											{
												binary.AddRange(BitConverter.GetBytes(value[i]));
											}
											else
											{
												binary.Add(0);
												binary.Add(0);
											}
										}
									}
									break;
#if false
								case ManifestXmlReader.ValueType.sjis:
									{
										var unicode = Encoding.Unicode;
										var unicodeByte = unicode.GetBytes(field);
										var sjis = Encoding.GetEncoding("shift_jis");
										var value = Encoding.Convert(unicode, sjis, unicodeByte);
										for (var i = 0; i < content.length; ++i)
										{
											if (value.Length > i && i < content.length - 1)
											{
												binary.Add(value[i]);
											}
											else
											{
												binary.Add(0);
											}
										}
									}
									break;
#endif
								//---------------------------------------
								// Bits Type
								case ManifestXmlReader.ValueType.bits32:
									{
										const int size = 32;
										Int32 value = 0;
										if (content.valueName != string.Empty)
										{
											if (!Int32.TryParse(field, out value)) goto Failed;
										}

										if (0 >= content.length)
										{
											goto Failed;
										}

										if (!bitflagsProcessing)
										{
											bitflagsProcessing = true;
											bitflagsShift = 0;
											bitflags = 0;
										}

										var mask = ~(~(0x01 << content.length) + 1);
										bitflags = (bitflags | ((value & mask) << bitflagsShift));

										bitflagsShift += content.length;
										if (size < bitflagsShift) goto Failed;
										if (size == bitflagsShift)
										{
											bitflagsProcessing = false;
											binary.AddRange(BitConverter.GetBytes(bitflags));
										}
									}
									break;
							}
						}

						if (bitflagsProcessing)
						{
							bitflagsProcessing = false;
							binary.AddRange(BitConverter.GetBytes(bitflags));
						}
						dest.AddRange(binary);
						++numRecords;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				goto Failed;
			}

			return true;
			Failed:
			dest.Clear();
			numRecords = 0;
			return false;
		}
	}
}
