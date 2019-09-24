﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BinReadTest
{
	class Program
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
		public readonly struct Cont
		{
			public override string ToString()
			{
				var s = string.Empty;
				s += string.Format("code1={0}", code1);
				s += ", ";
				s += string.Format("code2={0}", code2);
				s += ", ";
				s += string.Format("code3={0}", code3);
				s += ", ";
				s += string.Format("code6={0}", code6);
				s += ", ";
				s += string.Format("flags1[flags1_bits1]={0}", flags1[flags1_bits1]);
				s += ", ";
				s += string.Format("flags1[flags1_bits2]={0}", flags1[flags1_bits2]);
				return s;
			}

			public readonly UInt32 code1;
			public readonly Int32 code2;
			public readonly float code3;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
			public readonly string code6;
			public static readonly BitVector32.Section flags1_bits1 = BitVector32.CreateSection(1);
			public static readonly BitVector32.Section flags1_bits2 = BitVector32.CreateSection(2, flags1_bits1);
			public readonly BitVector32 flags1;
		}

#if false
		// UnicodeとAnsi文字列の混合構造体はbyte配列で受け取って変換するしかない
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct Cont
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4/*chars*/*2/*bytes*/)] public byte[] utf16;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8/*chars*/)] public byte[] sjis;
		/*
				s += string.Format("{0}, ", Encoding.Unicode.GetString(utf16, 0, utf16.Length));
				s += string.Format("{0}, ", Encoding.GetEncoding("Shift_JIS").GetString(sjis, 0, sjis.Length)); // 2bytes文字を含む場合
				s += string.Format("{0}, ", Encoding.ASCII.GetString(sjis, 0, sjis.Length)); // 2bytes文字を含まない場合ASCIIでOK
		*/
		}

#elif false
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

        static int Test1()
        {
            try
            {
                var filePath = "../../../Csv2Bin/make/table1.bin";
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    // Console.WriteLine("fileSize={0}", fs.Length);
                    var buff = new byte[256];
                    var pos = fs.Seek(-4 * 2, SeekOrigin.End);
                    //Console.WriteLine("pos={0}", pos);
                    if (4 * 2 != fs.Read(buff, 0, 4 * 2))
                    {
                        //Console.WriteLine("Error1");
                        goto Failed;
                    }
                    var size = BitConverter.ToUInt32(buff, 0);
                    //Console.WriteLine("size={0}", size);
                    var len = BitConverter.ToUInt32(buff, 4);
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
            }
            return 0;
            Failed:
            return 1;
        }

        static int Test2()
        {
            {
                var filePath = "../../../Csv2Bin/make/table2.bin";
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
            return 0;
            Failed:
            return 1;
        }

		static int Main(string[] args)
		{
            int ret = 0;
#if true
            ret = Test1();
#else
            ret = Test2();
#endif
            for (var i = 0; i < _cont.Length; ++i)
            {
                Console.WriteLine(_cont[i].ToString());
            }
            return ret;
        }
    }
}
