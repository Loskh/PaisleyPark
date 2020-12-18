using System;
using System.Diagnostics;
using System.Reflection;

namespace PaisleyPark.MemoryCore
{
	public static class ExtensionMethods
	{
		public static string ToHex(this IntPtr p) => string.Format("0x{0:X}", (ulong)p);
		public static string ToOffset(this IntPtr p) {
			//if(p<)
			return string.Format("{0} + 0x{1:X}", MemoryService.Process.MainModule.ModuleName, (ulong)p - (ulong)MemoryService.Process.MainModule.BaseAddress);
		}
	}
}
