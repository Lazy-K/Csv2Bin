using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Csv2Bin
{
	public partial class Manifest
	{
		public static bool Write(
			string filePath,
			in ManifestHeader header,
			in List<ManifestContent> contents)
		{
			try
			{
				var settings = new XmlWriterSettings();
				settings.Async = true;
				settings.Encoding = System.Text.Encoding.UTF8;
				settings.Indent = true;
				settings.IndentChars = "  ";

				using (var streamWriter = new StreamWriter(filePath, false, Encoding.UTF8))
				{
					using (var writer = XmlWriter.Create(streamWriter, settings))
					{
						writer.WriteStartDocument(true);
						writer.WriteStartElement("root");

						{ // header
							writer.WriteStartElement("header");
							writer.WriteElementString("version", string.Format("{0:F1}", header.version));
							writer.WriteElementString("structName", header.structName);
							writer.WriteEndElement();
						}

						{ // content
							var contentsCount = contents.Count;
							for (var i = 0; i < contentsCount; ++i)
							{
								writer.WriteStartElement("content");
								if (null != contents[i].valueName)
								{
									writer.WriteElementString("valueName", contents[i].valueName);
								}
								writer.WriteElementString("valueType", contents[i].valueType.ToString());
								if (ValueType.utf16 == contents[i].valueType || ValueType.bits32 == contents[i].valueType)
								{
									writer.WriteElementString("length", contents[i].length.ToString());
								}
								if (null != contents[i].structFieldName)
								{
									writer.WriteElementString("structFieldName", contents[i].structFieldName);
								}
								if (null != contents[i].structBitsName)
								{
									writer.WriteElementString("structBitsName", contents[i].structBitsName);
								}
								writer.WriteEndElement();
							}
							writer.WriteEndElement();
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
			return false;
		}
	}
}
