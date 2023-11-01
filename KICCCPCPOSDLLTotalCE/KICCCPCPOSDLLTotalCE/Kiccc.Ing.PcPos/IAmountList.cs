using System;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
	[ComVisible(true)]
	[Guid("205EF9F4-A1E0-4BDE-94BB-88B668D3CE7F")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IAmountList
	{
		string AmountDeduction
		{
			get;
		}

		int Count
		{
			get;
		}

		bool IsReadOnly
		{
			get;
		}

		int MaxSize
		{
			get;
		}

		string MultiplexAmount
		{
			get;
		}

		string TotalAmount
		{
			get;
		}

		void Add(string amount);

		string Remove();
	}
}