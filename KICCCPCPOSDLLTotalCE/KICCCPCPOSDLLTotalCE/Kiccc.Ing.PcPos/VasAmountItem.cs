using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	[Guid("90A4690A-AB63-458C-8F6C-F8E5A322203B")]
	//[ProgId("Kiccc.Ing.PcPos.VasAmountItem")]
	[Serializable]
	public class VasAmountItem : IVasAmountItem
	{
        long _amount;
		public long Amount
		{
            get { return _amount; }
            set { _amount = value; }
		}

		int _order;
		public int Order
		{
            get { return _order; }
            set { _order = value; }
		}

		public VasAmountItem()
		{
		}
	}
}