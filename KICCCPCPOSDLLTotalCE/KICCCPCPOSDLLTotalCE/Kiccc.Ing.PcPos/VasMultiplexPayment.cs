using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Kiccc.Ing.PcPos
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ComVisible(true)]
	[Guid("608B2F1F-5DB2-4597-8303-817E2F034F9B")]
	[ProgId("Kiccc.Ing.PcPos.VasMultiplexPayment")]
	[Serializable]
	public class VasMultiplexPayment : IVasMultiplexPayment
	{
		private string _paymentId = string.Empty;

		private int _index;

		public VasAmountList Amounts { get; set; } = new VasAmountList();

		public int Index
		{
			get
			{
				return this._index;
			}
			set
			{
				if ((value <= 0 ? true : value > 5))
				{
					throw new Exception("Invalid Index");
				}
				this._index = value;
			}
		}

		public string PaymentId
		{
			get
			{
				return this._paymentId;
			}
			set
			{
				if ((string.IsNullOrEmpty(value) ? true : !Regex.IsMatch(value, "^[0-9]{1,18}$")))
				{
					throw new Exception("Invalid PaymentId");
				}
				this._paymentId = value;
			}
		}

		public VasMultiplexPayment()
		{
		}
	}
}