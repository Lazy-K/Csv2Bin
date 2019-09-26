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
		public static bool IsValid(
			ref ManifestHeader header,
			ref List<ManifestContent> contents)
		{
			if (1.0f != header.version)
			{
				Console.WriteLine("Manifest Error(header): version \"{0:F1}\" is invalid", header.version);
				return false;
			}
			if (null == header.structName || string.Empty == header.structName)
			{
				Console.WriteLine("Manifest Error(header): structName is invalid");
				return false;
			}

			var contentsCount = contents.Count;
			for (var i = 0; i < contentsCount; ++i)
			{
				{
					var isValid = true;
					if (ValueType.utf16 == contents[i].valueType)
					{
						if (0 >= contents[i].length)
						{
							isValid = false;
						}
					}
					else if (ValueType.bits32 == contents[i].valueType)
					{
						if (0 > contents[i].length/*0はビットフィールド強制スプリットで許可*/ || 32 < contents[i].length)
						{
							isValid = false;
						}
					}
					else
					{
						if (0 != contents[i].length)
						{
							isValid = false;
						}
					}

					if (!isValid)
					{
						Console.WriteLine("Manifest Error(content No.{0}): length \"{1}\" is invalid", i + 1, contents[i].length);
						return false;
					}
				}

				{
					if (ValueType.bits32 != contents[i].valueType)
					{
						if (null != contents[i].structBitsName && contents[i].structBitsName != string.Empty)
						{
							Console.WriteLine("Manifest Error(content No.{0}): structBitsName \"{1}\" must be empty for bits32 type", i + 1, contents[i].structBitsName);
							return false;
						}
						if (null != contents[i].structFieldName && contents[i].structFieldName != string.Empty)
						{
							Console.WriteLine("Manifest Error(content No.{0}): structFieldName \"{1}\" must be empty for bits32 type", i + 1, contents[i].structFieldName);
							return false;
						}
					}
				}
			}
			return true;
		}
	}
}
