#define ENABLE_DEBUG_LOG

using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
			var code = string.Empty;

			code += "using System;\n";
			code += "using System.Runtime.InteropServices;\n";
			code += "using System.Collections.Specialized;\n";
			code += "\n";

			var contentsCount = contents.Count;
			{
				var isExitsString = false;
				for (var i = 0; i < contentsCount; ++i)
				{
					if (ValueType.utf16 == contents[i].valueType)
					{
						isExitsString = true;
					}
				}
				if (isExitsString)
				{
					code += "[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]\n";
				}
				else
				{
					code += "[StructLayout(LayoutKind.Sequential, Pack = 1)]\n";
				}
			}
			code += "public readonly struct " + header.structName + "\n";
			code += "{\n";

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
					if (null != contents[i].structFieldName)
					{
						var bitsDummyCount = 0;
						var bits = contents[i].length;

						var prevSectionName = string.Empty;
						{
							var isDummyBitsName = false;
							var bitsName = contents[i].structBitsName;
							if (null == bitsName)
							{
								isDummyBitsName = true;
								bitsName = string.Format("reserved_{0:00}", bitsDummyCount++);
							}
							var sectionName = string.Format("{0}_{1}", contents[i].structFieldName, bitsName);
							body += string.Format("\tpublic static readonly BitVector32.Section {0} = BitVector32.CreateSection({1});\n",
								sectionName,
								contents[i].length);
							prevSectionName = sectionName;

							if (!isDummyBitsName)
							{
								if (0 < toStringCode.Length) toStringCode += "\t\ts += \", \";\n";
								toStringCode += string.Format("\t\ts += string.Format(\"{0}[{1}]={{0}}\", {0}[{1}]);\n", fieldName, sectionName);
							}
						}
						var j = i + 1;
						for (; j < contentsCount; ++j)
						{
							if (ValueType.bits32 != contents[j].valueType) break;
							bits += contents[j].length;

							{
								var isDummyBitsName = false;
								var bitsName = contents[j].structBitsName;
								if (null == bitsName)
								{
									isDummyBitsName = true;
									if (32 == bits)
									{
										++j;
										break;
									}
									bitsName = string.Format("reserved_{0:00}", bitsDummyCount++);
								}
								var sectionName = string.Format("{0}_{1}", contents[i].structFieldName, bitsName);
								body += string.Format("\tpublic static readonly BitVector32.Section {0} = BitVector32.CreateSection({1}, {2});\n",
									sectionName,
									contents[j].length,
									prevSectionName);
								prevSectionName = sectionName;

								if (!isDummyBitsName)
								{
									if (0 < toStringCode.Length) toStringCode += "\t\ts += \", \";\n";
									toStringCode += string.Format("\t\ts += string.Format(\"{0}[{1}]={{0}}\", {0}[{1}]);\n", fieldName, sectionName);
								}
							}

							if (32 == bits)
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
						if (0 < toStringCode.Length) toStringCode += "\t\ts += \", \";\n";
						toStringCode += string.Format("\t\ts += string.Format(\"{0}={{0}}\", {0});\n", fieldName);
					}
				}


				var scope = isDummyFieldName ? "private" : "public";
				var codeValueType = _codeValueTypes[(int)contents[i].valueType];

				if (ValueType.utf16 == contents[i].valueType)
				{
					body += string.Format("\t[MarshalAs(UnmanagedType.ByValTStr, SizeConst = {0})]\n", contents[i].length);
				}
				body += string.Format("\t{0} readonly {1} {2};\n", scope, codeValueType, fieldName);

				i = next;
			}

			{
				code += "\tpublic override string ToString()\n";
				code += "\t{\n";
				code += "\t\tvar s = string.Empty;\n";
				code += toStringCode;
				code += "\t\treturn s;\n";
				code += "\t}\n";
				code += "\n";
			}
			code += body;
			code += "}\n";
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
