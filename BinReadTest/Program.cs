using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BinReadTest
{
	class Program
	{
#if false
		// UnicodeとAnsi文字列の混合構造体はbyte配列で受け取って変換するしかない
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct Cont
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4/*chars*/*2/*bytes*/)] public byte[] utf16;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8/*chars*/)] public byte[] sjis;
			/*
				Decode例
				s += string.Format("{0}, ", Encoding.Unicode.GetString(utf16, 0, utf16.Length));
				s += string.Format("{0}, ", Encoding.GetEncoding("Shift_JIS").GetString(sjis, 0, sjis.Length)); // 2bytes文字を含む場合
				s += string.Format("{0}, ", Encoding.ASCII.GetString(sjis, 0, sjis.Length)); // 2bytes文字を含まない場合ASCIIでOK
			*/
		}

		// UnicodeとAnsi文字列を混合して持っていない構造体はStructLayoutで指示すればstringで受け取れる
		[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
		//[StructLayout(LayoutKind.Sequential, Pack =1, CharSet = CharSet.Ansi)]
		private struct Cont
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst =4)] public string utf16; // CharSet = CharSet.Unicode
			//[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2)] public string sjis; // CharSet = CharSet.Ansi
		}
#endif

		static private Cont[] _cont;

		/**
		 * バイナリファイルに要約情報(ファイル末尾に構造体サイズとデータ数の情報)がある場合
		 */
		static int Test1(in string filePath)
		{
			try
			{
				using (var fs = new FileStream(filePath, FileMode.Open))
				{
					// Console.WriteLine("fileSize={0}", fs.Length);
					var buff = new byte[256];
					var pos = fs.Seek(-sizeof(UInt32) * 2, SeekOrigin.End);
					//Console.WriteLine("pos={0}", pos);
					if (sizeof(UInt32) * 2 != fs.Read(buff, 0, sizeof(UInt32) * 2))
					{
						//Console.WriteLine("Error1");
						goto Failed;
					}
					var size = BitConverter.ToUInt32(buff, sizeof(UInt32) * 0);
					//Console.WriteLine("size={0}", size);
					var len = BitConverter.ToUInt32(buff, sizeof(UInt32) * 1);
					//Console.WriteLine("size={0}", len);
					if (size != Marshal.SizeOf<Cont>())
					{
						//Console.WriteLine("Error2");
						goto Failed;
					}

					_cont = new Cont[len];
					{
						pos = fs.Seek(0, SeekOrigin.Begin);
						//Console.WriteLine("pos={0}", pos);
						for (var i = 0; i < len; ++i)
						{
							if (size != fs.Read(buff, 0, (int)size))
							{
								//Console.WriteLine("Error3");
								goto Failed;
							}
							//Console.WriteLine("pos={0}", pos);
							var gch = GCHandle.Alloc(buff, GCHandleType.Pinned);
							_cont[i] = Marshal.PtrToStructure<Cont>(gch.AddrOfPinnedObject());
							gch.Free();
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				goto Failed;
			}
			return 0;
			Failed:
			return 1;
		}

		/**
		 * バイナリファイルに要約情報(ファイル末尾に構造体サイズとデータ数の情報)がない場合
		 */
		static int Test2(in string filePath)
		{
			try
			{
				var binary = System.IO.File.ReadAllBytes(filePath);
				var size = Marshal.SizeOf<Cont>();
				if (0 != (binary.Length % size))
				{
					//Console.WriteLine("Error1");
					goto Failed;
				}
				var len = binary.Length / size;
				_cont = new Cont[len];
				{
					var gch = GCHandle.Alloc(binary, GCHandleType.Pinned);
					var ptr = gch.AddrOfPinnedObject();
					for (var i = 0; i < len; ++i)
					{
						var ins = new IntPtr(ptr.ToInt64() + i * size);
						_cont[i] = Marshal.PtrToStructure<Cont>(ins);
					}
					gch.Free();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				goto Failed;
			}

			return 0;
			Failed:
			return 1;
		}

		static int Main(string[] args)
		{
#if true
			var ret = Test1("../../../Csv2Bin/make/table1.bin");
#else
			var ret = Test2("../../../Csv2Bin/make/table1.bin");
#endif
			Console.WriteLine("ret={0}", ret);
			if (null != _cont)
			{
				for (var i = 0; i < _cont.Length; ++i)
				{
					Console.WriteLine(_cont[i].ToString());
				}
			}
			return ret;
		}
	}
}
