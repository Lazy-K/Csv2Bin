using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Csv2Bin
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

	public struct ManifestHeader
	{
		public float version;
		public string structName;
	}

	public struct ManifestContent
	{
		public string valueName;
		public ValueType valueType;
		public int length;

		public string structFieldName;
		public string structBitsName;
	}

	public partial class Manifest
	{
		public static bool Parse(
			string filePath,
			ref ManifestHeader header,
			ref List<ManifestContent> contents)
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
										if (!Read_Header(reader, ref header))
										{
											goto Failed;
										}
										break;
									case "content":
										{
											var content = new ManifestContent();
											if (!Read_Content(reader, ref content))
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
										Console.WriteLine("Manifest Error(root): \"{0}\" element is unknown", reader.Name);
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
				Console.WriteLine(e.ToString());
				goto Failed;
			}
			return true;
			Failed:
			contents.Clear();
			return false;
		}

		private static bool Read_Header(
			XmlReader reader,
			ref ManifestHeader header)
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
											Console.WriteLine("Manifest Error(header): \"version\" element is invalid");
											goto Failed;
										}
										break;
									case "structName":
										header.structName = task.Result;
										break;
									default:
										Console.WriteLine("Manifest Error(header): \"{0}\" element is unknown", elementName);
										goto Failed;
								}
							}
							break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Manifest Error(header): {0}", e.ToString());
				goto Failed;
			}

			if (1.0f != header.version)
			{
				Console.WriteLine("Manifest Error(header): \"version\" element is invalid\n");
				goto Failed;
			}
			if (null == header.structName)
			{
				Console.WriteLine("Manifest Error(header): \"structName\" element must be required\n");
				goto Failed;
			}

			return true;
			Failed:
			return false;
		}

		private static bool Read_Content(
			XmlReader reader,
			ref ManifestContent content)
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
												Console.WriteLine("Manifest Error(content): \"valueType\" element \"{0}\" is unknown", task.Result);
												goto Failed;
											}
											isValueTypeSetuped = true;
										}
										break;
									case "length":
										if (!Int32.TryParse(task.Result, out content.length))
										{
											Console.WriteLine("Manifest Error(content): \"length\" element \"{0}\" is invalid", task.Result);
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
										Console.WriteLine("Manifest Error(content): \"{0}\" element is unknown", elementName);
										goto Failed;

								}
							}
							break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("Manifest Error(content): {0}", e.ToString());
				goto Failed;
			}

			if (!isValueTypeSetuped)
			{
				Console.WriteLine("Manifest Error(content): \"valueType\" element must be required");
				goto Failed;
			}

			switch (content.valueType)
			{
				case ValueType.utf16:
					if (!isLengthSetuped)
					{
						Console.WriteLine("Manifest Error(content): \"length\" element must be required for valueType \"{0}\"", content.valueType.ToString());
						goto Failed;
					}
					if (0 >= content.length)
					{
						Console.WriteLine("Manifest Error(content): \"length\" element \"{0}\"\n is invalid range[0<length] for valueType \"{1}\"", content.length, content.valueType.ToString());
						goto Failed;
					}
					break;
				case ValueType.bits32:
					if (!isLengthSetuped)
					{
						Console.WriteLine("Manifest Error(content): \"length\" element must be required for valueType \"{0}\"", content.valueType.ToString());
						goto Failed;
					}
					if (0/*0は強制ビットフィールドスプリットで許可*/ > content.length || 15/*BitVector32のSection引数制限*/ < content.length)
					{
						Console.WriteLine("Manifest Error(content): \"length\" element \"{0}\"\n is invalid range[0|1<=length<=15] for valueType \"{1}\"", content.length, content.valueType.ToString());
						goto Failed;
					}
					break;
				default:
					if (isLengthSetuped)
					{
						Console.WriteLine("Manifest Error(content): \"length\" element is not supported for valueType \"{0}\"", content.valueType.ToString());
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
