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

		static public string GenerateCode(
			ref Header header,
			ref List<Content> contents)
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

				if (ValueType.bits32 == contents[i].valueType && 0 != contents[i].length/*length=0の場合はビットフィールド強制スプリット*/)
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

							if (0 == contents[j].length/*length=0の場合はビットフィールド強制スプリット*/)
							{
								++j;
								break;
							}
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
		public static bool Read(
			string filePath,
			ref Header header,
			ref List<Content> contents,
			StreamWriter logFile)
		{
			contents.Clear();
			try
			{
				var settings = new XmlReaderSettings();
				settings.Async = true;

				using (var reader = XmlReader.Create(new StreamReader(filePath, Encoding.UTF8), settings))
				{
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
								switch (reader.Name)
								{
									case "header":
										if (!Read_Header(reader, ref header, logFile))
										{
											goto Failed;
										}
										break;
									case "content":
										{
											var content = new Content();
											if (!Read_Content(reader, ref content, logFile))
											{
												goto Failed;
											}
											contents.Add(content);
										}
										break;
									case "root":
										// NOP
										break;
									default:
										if (null != logFile) logFile.Write("Manifest Error(root): \"{0}\" element is unknown\n", reader.Name);
										goto Failed;
								}
								break;
							case XmlNodeType.Text:
								{
									var task = reader.GetValueAsync();
									task.Wait();
								}
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				if (null != logFile) logFile.Write(e.ToString());
				goto Failed;
			}
			return true;
			Failed:
			contents.Clear();
			return false;
		}

		private static bool Read_Header(
			XmlReader reader,
			ref Header header,
			StreamWriter logFile)
		{
			try
			{
				var elementName = string.Empty;
				var isExit = false;
				while (!isExit)
				{
					{
						var task = reader.ReadAsync();
						task.Wait();
						if (!task.Result) break;
					}
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							elementName = reader.Name;
							break;
						case XmlNodeType.EndElement:
							if ("header" == reader.Name)
							{
								isExit = true;
							}
							break;
						case XmlNodeType.Text:
							{
								var task = reader.GetValueAsync();
								task.Wait();
								switch (elementName)
								{
									case "version":
										if (!float.TryParse(task.Result, out header.version))
										{
											if (null != logFile) logFile.Write("Manifest Error(header): \"version\" element is invalid\n");
											goto Failed;
										}
										break;
									case "structName":
										header.structName = task.Result;
										break;
									default:
										if (null != logFile) logFile.Write("Manifest Error(header): \"{0}\" element is unknown\n", elementName);
										goto Failed;
								}
							}
							break;
					}
				}
			}
			catch (Exception e)
			{
				if (null != logFile) logFile.Write("Manifest Error(header): {0}", e.ToString());
				goto Failed;
			}

			if (1.0f != header.version)
			{
				if (null != logFile) logFile.Write("Manifest Error(header): \"version\" element is invalid\n");
				goto Failed;
			}
			if (null == header.structName)
			{
				if (null != logFile) logFile.Write("Manifest Error(header): \"structName\" element must be required\n");
				goto Failed;
			}

			return true;
			Failed:
			return false;
		}

		private static bool Read_Content(
			XmlReader reader,
			ref Content content,
			StreamWriter logFile)
		{
			var isValueTypeSetuped = false;
			var isLengthSetuped = false;
			try
			{
				var elementName = string.Empty;
				var isExit = false;
				while (!isExit)
				{
					{
						var task = reader.ReadAsync();
						task.Wait();
						if (!task.Result) break;
					}
					switch (reader.NodeType)
					{
						case XmlNodeType.Element:
							elementName = reader.Name;
							break;
						case XmlNodeType.EndElement:
							if ("content" == reader.Name)
							{
								isExit = true;
							}
							break;
						case XmlNodeType.Text:
							{
								var task = reader.GetValueAsync();
								task.Wait();
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
											if (valueTypeCount <= i)
											{
												if (null != logFile) logFile.Write("Manifest Error(content): \"valueType\" element \"{0}\" is unknown\n", task.Result);
												goto Failed;
											}
											isValueTypeSetuped = true;
										}
										break;
									case "length":
										if (!Int32.TryParse(task.Result, out content.length))
										{
											if (null != logFile) logFile.Write("Manifest Error(content): \"length\" element \"{0}\" is invalid\n", task.Result);
											goto Failed;
										}
										isLengthSetuped = true;
										break;
									case "structFieldName":
										content.structFieldName = task.Result;
										break;
									case "structBitsName":
										content.structBitsName = task.Result;
										break;
									default:
										if (null != logFile) logFile.Write("Manifest Error(content): \"{0}\" element is unknown\n", elementName);
										goto Failed;

								}
							}
							break;
					}
				}
			}
			catch (Exception e)
			{
				if (null != logFile) logFile.Write("Manifest Error(content): {0}", e.ToString());
				goto Failed;
			}

			if (!isValueTypeSetuped)
			{
				if (null != logFile) logFile.Write("Manifest Error(content): \"valueType\" element must be required\n");
				goto Failed;
			}

			switch (content.valueType)
			{
				case ValueType.utf16:
					if (!isLengthSetuped)
					{
						if (null != logFile) logFile.Write("Manifest Error(content): \"length\" element must be required for valueType \"{0}\"\n", content.valueType.ToString());
						goto Failed;
					}
					if (0 >= content.length)
					{
						if (null != logFile) logFile.Write("Manifest Error(content): \"length\" element \"{0}\"\n is invalid range[0<length] for valueType \"{1}\"\n", content.length, content.valueType.ToString());
						goto Failed;
					}
					break;
				case ValueType.bits32:
					if (!isLengthSetuped)
					{
						if (null != logFile) logFile.Write("Manifest Error(content): \"length\" element must be required for valueType \"{0}\"\n", content.valueType.ToString());
						goto Failed;
					}
					if (0/*0は強制ビットフィールドスプリットで許可*/ > content.length || 15/*BitVector32のSection引数制限*/ < content.length)
					{
						if (null != logFile) logFile.Write("Manifest Error(content): \"length\" element \"{0}\"\n is invalid range[0|1<=length<=15] for valueType \"{1}\"\n", content.length, content.valueType.ToString());
						goto Failed;
					}
					break;
				default:
					if (isLengthSetuped)
					{
						if (null != logFile) logFile.Write("Manifest Error(content): \"length\" element is not supported for valueType \"{0}\"\n", content.valueType.ToString());
						goto Failed;
					}
					break;
			}

			return true;
			Failed:
			return false;
		}
	}
}
