using System;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
	[ComVisible(true)]
	[Guid("5DB9EEE5-2CA8-43D5-8224-5EA1B6F28B70")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IVasAmountList
	{
		[DispId(1)]
		int Count
		{
			get;
		}

		[DispId(6)]
		bool IsReadOnly
		{
			get;
		}

		[DispId(0)]
		int MaxSize
		{
			get;
		}

		[DispId(2)]
		string MultiplexAmount
		{
			get;
		}

		[DispId(3)]
		string TotalAmount
		{
			get;
		}

		[DispId(4)]
		void Add(long amount, int order);

		[DispId(5)]
		bool Remove(long amount, int order);
	}
}