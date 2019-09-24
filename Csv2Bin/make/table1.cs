using System;
using System.Runtime.InteropServices;
using System.Collections.Specialized;

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
		s += ", ";
		s += string.Format("flags1[flags1_bits3]={0}", flags1[flags1_bits3]);
		s += ", ";
		s += string.Format("flags2[flags2_bits1]={0}", flags2[flags2_bits1]);
		return s;
	}

	public readonly UInt32 code1;
	public readonly Int32 code2;
	public readonly float code3;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
	public readonly string code6;
	public static readonly BitVector32.Section flags1_bits1 = BitVector32.CreateSection(1);
	public static readonly BitVector32.Section flags1_bits2 = BitVector32.CreateSection(2, flags1_bits1);
	public static readonly BitVector32.Section flags1_reserved_00 = BitVector32.CreateSection(3, flags1_bits2);
	public static readonly BitVector32.Section flags1_bits3 = BitVector32.CreateSection(5, flags1_reserved_00);
	public readonly BitVector32 flags1;
	public static readonly BitVector32.Section flags2_reserved_00 = BitVector32.CreateSection(24);
	public static readonly BitVector32.Section flags2_bits1 = BitVector32.CreateSection(1, flags2_reserved_00);
	public readonly BitVector32 flags2;
	private readonly sbyte _reserved_00;
}
