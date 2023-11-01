using System;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
	[ComVisible(true)]
	[Guid("AC012AA7-1BD9-4768-A1A4-CA13C24C1E88")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IVasAmountItem
	{
		[DispId(0)]
		long Amount
		{
			get;
			set;
		}

		[DispId(1)]
		int Order
		{
			get;
			set;
		}
	}
}