using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TableReader
{
	public class Reader
	{
		public bool Read(string filePath, List<ManifestReader.Reader.Attribute> manifestAttributes, ref List<byte> dest, ref UInt32 numRecords)
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
						byte bitflags = 0;
						var bitflagsShift = 0;
						foreach (ManifestReader.Reader.Attribute attr in manifestAttributes)
						{
							var field = String.Empty;
							if (attr.fieldName != string.Empty)
							{
								var index = reader.GetFieldIndex(attr.fieldName);
								field = reader.GetField(index);
							}

							switch (attr.type)
							{
								case ManifestReader.Reader.Type.u8:
									{
										byte value = 0;
										if (attr.fieldName != string.Empty)
										{
											if (!byte.TryParse(field, out value)) goto Failed;
										}

										if (0 < attr.argument)
										{
											if (!bitflagsProcessing)
											{
												bitflagsProcessing = true;
												bitflagsShift = 0;
												bitflags = 0;
											}

											var mask = ~(~(0x01 << attr.argument) + 1);
											bitflags = (byte)((int)bitflags | (((int)value & mask) << bitflagsShift));

											bitflagsShift += attr.argument;
											if (8 < bitflagsShift) goto Failed;
											if (8 == bitflagsShift)
											{
												bitflagsProcessing = false;
												binary.Add(bitflags);
											}
										}
										else
										{
											if (bitflagsProcessing) goto Failed;
											binary.Add(value);
										}
									}
									break;
								case ManifestReader.Reader.Type.u16:
									{
										UInt16 value = 0;
										if (attr.fieldName != string.Empty)
										{
											if (!UInt16.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestReader.Reader.Type.u32:
									{
										UInt32 value = 0;
										if (attr.fieldName != string.Empty)
										{
											if (!UInt32.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestReader.Reader.Type.s32:
									{
										Int32 value = 0;
										if (attr.fieldName != string.Empty)
										{
											if (!Int32.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestReader.Reader.Type.f32:
									{
										float value = 0;
										if (attr.fieldName != string.Empty)
										{
											if (!float.TryParse(field, out value)) goto Failed;
										}
										binary.AddRange(BitConverter.GetBytes(value));
									}
									break;
								case ManifestReader.Reader.Type.utf16:
									{
										var value = field;
										for (var i = 0; i < attr.argument; ++i)
										{
											if (value.Length > i && i < attr.argument - 1)
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
								case ManifestReader.Reader.Type.sjis:
									{
										var unicode = Encoding.Unicode;
										var unicodeByte = unicode.GetBytes(field);
										var sjis = Encoding.GetEncoding("shift_jis");
										var value = Encoding.Convert(unicode, sjis, unicodeByte);
										for (var i = 0; i < attr.argument; ++i)
										{
											if (value.Length > i && i < attr.argument - 1)
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
							}
						}
                        dest.AddRange(binary);
                        ++numRecords;
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
            dest.Clear();
            numRecords = 0;
            return false;
		}
	}
}
