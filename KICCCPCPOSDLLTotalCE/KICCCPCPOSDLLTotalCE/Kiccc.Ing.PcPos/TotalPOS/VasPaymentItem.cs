using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos.TotalPOS
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComSourceInterfaces(typeof(IVasPaymentItem))]
	[ComVisible(true)]
	[Guid("94047997-FB15-49B1-96CC-048A675E5107")]
	[ProgId("Kiccc.PcPos.VasPaymentItem")]
	public class VasPaymentItem : IVasPaymentItem
	{
		private string _amount;

		public string Amount
		{
			get
			{
				string str;
				string str1 = this._amount;
				if (str1 != null)
				{
					str = str1.PadLeft(12, '0');
				}
				else
				{
					str = null;
				}
				return str;
			}
			set
			{
				this._amount = value;
			}
		}

		public string Iban
		{
			get;
			set;
		}

		public string Terminal
		{
			get;
			set;
		}

		public VasPaymentItem()
		{
		}
	}
}