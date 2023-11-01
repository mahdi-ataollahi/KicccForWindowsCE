using System;

namespace Kiccc.Ing.PcPos
{
	public enum MessageType
	{
		Sale,
		SaleWithPaymentId,
		Multiplex,
		CreditRemaining,
		Special,
		MultiplexWithPaymentId,
		BillPayment,
		Extra,
		VasMultiplex,
		ESpecialPayment,
		VasDynamic
	}
}