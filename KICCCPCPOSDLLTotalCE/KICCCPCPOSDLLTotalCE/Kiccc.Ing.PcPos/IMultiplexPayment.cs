using System;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
	[ComVisible(true)]
	[Guid("428BC97C-62CD-4149-8023-90F241BED966")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IMultiplexPayment
	{
		[DispId(8)]
		AmountList Amounts
		{
			get;
			set;
		}

		[DispId(7)]
		string BankCode
		{
			get;
			set;
		}

		[DispId(5)]
		string Financialyear
		{
			get;
			set;
		}

		[DispId(6)]
		string FiscalPeriod
		{
			get;
			set;
		}

		[DispId(2)]
		string Organization
		{
			get;
			set;
		}

		[DispId(0)]
		string PaymentId
		{
			get;
			set;
		}

		[DispId(4)]
		string SequenceCode
		{
			get;
			set;
		}

		[DispId(3)]
		string ServiceCode
		{
			get;
			set;
		}

		[DispId(1)]
		string SpecialPaymentId
		{
			get;
			set;
		}
	}
}