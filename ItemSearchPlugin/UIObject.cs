using System.Numerics;
using System.Runtime.InteropServices;

namespace ItemSearchPlugin {
	
	[StructLayout(LayoutKind.Explicit)]
	public struct UIObject {
		
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
		[FieldOffset(0x8)] public string Name;

		[FieldOffset(0x182)] public byte Flags;
		[FieldOffset(0x1BC)] public short X;
		[FieldOffset(0x1BE)] public short Y;
		[FieldOffset(0x1AC)] public float Scale;
		
		public bool Visible => (Flags & 0b00100000) == 0b00100000;
		public Vector2 Position => new Vector2(X, Y);

	}
}