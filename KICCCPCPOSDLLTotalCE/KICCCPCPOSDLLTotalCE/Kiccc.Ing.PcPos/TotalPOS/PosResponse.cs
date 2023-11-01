using System;
using System.Runtime.CompilerServices;

namespace Kiccc.Ing.PcPos.TotalPOS
{
	public struct PosResponse
	{
		public string Amount
		{
			get;
			set;
		}

		public string CRCResponse
		{
			get;
			set;
		}

		public string Pan
		{
			get;
			set;
		}

		public string PaymentlId
		{
			get;
			set;
		}

		public string RespCode
		{
			get;
			set;
		}

		public string RRN
		{
			get;
			set;
		}

		public string SerialNo
		{
			get;
			set;
		}

		public string TerminalId
		{
			get;
			set;
		}

		public string TraceNo
		{
			get;
			set;
		}

		public string TransactionDate
		{
			get;
			set;
		}

		public string TransactionTime
		{
			get;
			set;
		}
	}
}