using System;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
	[ComVisible(true)]
	[Guid("C30D73E9-A5BA-4B3F-9FA9-8351FD2519DD")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IVasMultiplexPayment
	{
		[DispId(2)]
		VasAmountList Amounts
		{
			get;
			set;
		}

		[DispId(0)]
		int Index
		{
			get;
			set;
		}

		[DispId(1)]
		string PaymentId
		{
			get;
			set;
		}
	}
}