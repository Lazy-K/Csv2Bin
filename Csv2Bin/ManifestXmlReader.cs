//#define ENABLE_DEBUG_LOG

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ManifestXmlReader
{
	public enum ValueType
	{
		s8,
		u8,
		s16,
		u16,
		s32,
		u32,
		f32,
		utf16,
		bits32,
		Length
	}

	public struct Header
	{
		public float version;
		public string structName;
	}

	public struct Content
	{
		public string valueName;
		public ValueType valueType;
		public int length;

		public string structFieldName;
		public string structBitsName;
	}

	public class Util
	{
		static private readonly string[] _codeValueTypes = {
			"sbyte", //s8,
			"byte", //u8,
			"Int16", //s16,
			"UInt16", //u16,
			"Int32", //s32,
			"UInt32", //u32,
			"float", //f32,
			"string", //utf16,
			"BitVector32" //bits32,
		};

		static public string GenerateCode(ref Header header, ref List<Content> contents)
		{
			var contentsCount = contents.Count;
			var isExitsString = false;
			var isExistBitsType = false;
			var toStringCode = string.Empty;
			var body = string.Empty;
			var fieldDummyCount = 0;
			for (var i = 0; i < contentsCount;)
			{
				var next = i + 1;

				var isDummyFieldName = false;
				var fieldName = contents[i].structFieldName;
				if (null == fieldName)
				{
					isDummyFieldName = true;
					fieldName = string.Format("_reserved_{0:00}", fieldDummyCount++);
				}

				if (ValueType.bits32 == contents[i].valueType)
				{
					const int BitsSize = 32;
					isExistBitsType = true;
					if (null != contents[i].structFieldName)
					{
						var bits = contents[i].length;

						var prevSectionName = string.Empty;
						{
							var maxValue = ~(~(0x01 << contents[i].length) + 1);
							if (null == contents[i].structBitsName)
							{
								prevSectionName = string.Format("BitVector32.CreateSection(0x{0:X4})", maxValue);
							}
							else
							{
								var sectionName = string.Format("{0}_{1}", contents[i].structFieldName, contents[i].structBitsName);
								body += string.Format("\tpublic static readonly BitVector32.Section {0} = BitVector32.CreateSection(0x{1:X4});\n",
									sectionName,
									maxValue);
								prevSectionName = sectionName;

								{
									var split = 0 < toStringCode.Length ? ", " : string.Empty;
									toStringCode += string.Format("\t\ts += string.Format(\"{0}{1}[{2}]={{0}}\", {1}[{2}]);\n", split, fieldName, sectionName);
								}
							}
						}
						var j = i + 1;
						for (; j < contentsCount; ++j)
						{
							if (ValueType.bits32 != contents[j].valueType) break;
							bits += contents[j].length;

							{
								var maxValue = ~(~(0x01 << contents[j].length) + 1);
								if (null == contents[j].structBitsName)
								{
									if (BitsSize == bits)
									{
										++j;
										break;
									}
									prevSectionName = string.Format("BitVector32.CreateSection(0x{0:X4}, {1})", maxValue, prevSectionName);
								}
								else
								{
									var sectionName = string.Format("{0}_{1}", contents[i].structFieldName, contents[j].structBitsName);
									body += string.Format("\tpublic static readonly BitVector32.Section {0} = BitVector32.CreateSection(0x{1:X4}, {2});\n",
										sectionName,
										maxValue,
										prevSectionName);
									prevSectionName = sectionName;

									{
										var split = 0 < toStringCode.Length ? ", " : string.Empty;
										toStringCode += string.Format("\t\ts += string.Format(\"{0}{1}[{2}]={{0}}\", {1}[{2}]);\n", split, fieldName, sectionName);
									}
								}
							}

							if (BitsSize == bits)
							{
								++j;
								break;
							}
						}
						next = j;
					}
				}
				else
				{
					if (!isDummyFieldName)
					{
						var split = 0 < toStringCode.Length ? ", " : string.Empty;
						toStringCode += string.Format("\t\ts += string.Format(\"{0}{1}={{0}}\", {1});\n", split, fieldName);
					}
				}


				var scope = isDummyFieldName ? "private" : "public";
				var codeValueType = _codeValueTypes[(int)contents[i].valueType];

				if (ValueType.utf16 == contents[i].valueType)
				{
					isExitsString = true;
					body += string.Format("\t[MarshalAs(UnmanagedType.ByValTStr, SizeConst = {0})]\n", contents[i].length);
				}
				body += string.Format("\t{0} readonly {1} {2};\n", scope, codeValueType, fieldName);

				i = next;
			}

			var code = string.Empty;
			{
				//-------------------------------------------
				// Header code
				code += "using System;\n";
				code += "using System.Runtime.InteropServices;\n";
				if (isExistBitsType)
				{
					code += "using System.Collections.Specialized;\n";
				}
				code += "\n";

				//-------------------------------------------
				// Struct begin code
				if (isExitsString)
				{
					code += "[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]\n";
				}
				else
				{
					code += "[StructLayout(LayoutKind.Sequential, Pack = 1)]\n";
				}
				code += "public readonly struct " + header.structName + "\n";
				code += "{\n";


				code += "\tpublic override string ToString()\n";
				code += "\t{\n";
				code += "\t\tvar s = string.Empty;\n";
				code += toStringCode;
				code += "\t\treturn s;\n";
				code += "\t}\n";
				code += "\n";

				//-------------------------------------------
				// Struct body code
				code += body;

				//-------------------------------------------
				// Struct end code
				code += "}\n";
			}
			return code;
		}
	}

	public class Reader
	{
		public bool Read(string filePath, ref Header header, ref List<Content> contents)
		{
			contents.Clear();
			try
			{
				var settings = new XmlReaderSettings();
				settings.Async = true;

				using (var reader = XmlReader.Create(new StreamReader(filePath, Encoding.UTF8), settings))
				{
					var elementName = string.Empty;
					while (true)
					{
						{
							var task = reader.ReadAsync();
							task.Wait();
							if (!task.Result) break;
						}
						switch (reader.NodeType)
						{
							case XmlNodeType.Element:
#if ENABLE_DEBUG_LOG
								Console.WriteLine("Start Element {0}", reader.Name);
#endif
								switch (reader.Name)
								{
									case "header":
										if (!Read_Header(reader, ref header))
										{
											goto Failed;
										}
										break;
									case "content":
										{
											var content = new Content();
											if (!Read_Content(reader, ref content))
											{
												goto Failed;
											}
											contents.Add(content);
										}
										break;
								}
								break;
							case XmlNodeType.Text:
								{
									var task = reader.GetValueAsync();
									task.Wait();
#if ENABLE_DEBUG_LOG
									Console.WriteLine("Text Node: {0}, {1}", reader.Name, task.Result);
#endif
								}
								break;
							case XmlNodeType.EndElement:
#if ENABLE_DEBUG_LOG
								Console.WriteLine("End Element {0}", reader.Name);
#endif
								break;
							default:
#if ENABLE_DEBUG_LOG
								Console.WriteLine("Other node {0} with value {1}", reader.NodeType, reader.Value);
#endif
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return true;
			Failed:
			contents.Clear();
			return false;
		}

		private bool Read_Header(XmlReader reader, ref Header header)
		{
			var elementName = string.Empty;
			while (true)
			{
				{
					var task = reader.ReadAsync();
					task.Wait();
					if (!task.Result) break;
				}
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
#if ENABLE_DEBUG_LOG
						Console.WriteLine("Start Element {0}", reader.Name);
#endif
						elementName = reader.Name;
						break;
					case XmlNodeType.EndElement:
#if ENABLE_DEBUG_LOG
						Console.WriteLine("End Element {0}", reader.Name);
#endif
						if ("header" == reader.Name)
						{
							return true;
						}
						break;
					case XmlNodeType.Text:
						{
							var task = reader.GetValueAsync();
							task.Wait();
#if ENABLE_DEBUG_LOG
							Console.WriteLine("Text Node: {0}", task.Result);
#endif
							switch (elementName)
							{
								case "version":
									header.version = float.Parse(task.Result);
									break;
								case "structName":
									header.structName = task.Result;
									break;
							}
						}
						break;
					default:
#if ENABLE_DEBUG_LOG
						Console.WriteLine("Other node {0} with value {1}",
										reader.NodeType, reader.Value);
#endif
						break;
				}
			}
			return false;
		}

		private bool Read_Content(XmlReader reader, ref Content content)
		{
			var elementName = string.Empty;
			while (true)
			{
				{
					var task = reader.ReadAsync();
					task.Wait();
					if (!task.Result) break;
				}
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
#if ENABLE_DEBUG_LOG
						Console.WriteLine("Start Element {0}", reader.Name);
#endif
						elementName = reader.Name;
						break;
					case XmlNodeType.EndElement:
#if ENABLE_DEBUG_LOG
						Console.WriteLine("End Element {0}", reader.Name);
#endif
						if ("content" == reader.Name)
						{
							return true;
						}
						break;
					case XmlNodeType.Text:
						{
							var task = reader.GetValueAsync();
							task.Wait();
#if ENABLE_DEBUG_LOG
							Console.WriteLine("Text Node: {0}", task.Result);
#endif
							switch (elementName)
							{
								case "valueName":
									content.valueName = task.Result;
									break;
								case "valueType":
									{
										var valueTypeCount = (int)ValueType.Length;
										var i = 0;
										for (; i < valueTypeCount; ++i)
										{
											content.valueType = (ValueType)i;
											if (content.valueType.ToString() == task.Result) break;
										}
										if (valueTypeCount <= i) return false;
									}
									break;
								case "length":
									content.length = Int32.Parse(task.Result);
									break;

								case "structFieldName":
									content.structFieldName = task.Result;
									break;
								case "structBitsName":
									content.structBitsName = task.Result;
									break;
							}
						}
						break;
					default:
#if ENABLE_DEBUG_LOG
						Console.WriteLine("Other node {0} with value {1}", reader.NodeType, reader.Value);
#endif
						break;
				}
			}
			return false;
		}
	}
}
