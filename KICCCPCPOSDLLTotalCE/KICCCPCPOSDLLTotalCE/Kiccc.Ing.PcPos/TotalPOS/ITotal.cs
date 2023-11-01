using Kiccc.Ing.PcPos;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos.TotalPOS
{
	[ComVisible(true)]
	[Guid("407B946E-0739-4B18-8331-7FECD8F02C31")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface ITotal
	{
		[DispId(22)]
		string AcceptorId
		{
			get;
			set;
		}

		[DispId(25)]
		ConnetionTypes ConnetionType
		{
			get;
			set;
		}

		[DispId(31)]
		bool GenerateLog
		{
			get;
			set;
		}

		[DispId(17)]
		string IpAddress
		{
			get;
			set;
		}

		[DispId(32)]
		string LogPath
		{
			get;
			set;
		}

		[DispId(18)]
		int Port
		{
			get;
			set;
		}

		[DispId(24)]
		string SerialNo
		{
			get;
			set;
		}

		[DispId(15)]
		ReadyState State
		{
			get;
			set;
		}

		[DispId(23)]
		string TerminalId
		{
			get;
			set;
		}

		[DispId(16)]
		int TimeOut
		{
			get;
			set;
		}

		[DispId(13)]
		bool BeginBillPayment(string billid, string billpaymentid);

		[DispId(12)]
		bool BeginCreditRemaining();

		[DispId(48)]
		bool BeginESpecialPayment(string amount, string paymentId);

		[DispId(9)]
		bool BeginMultiplexPayment(MultiplexPayment multiplexPayment);

		[DispId(10)]
		bool BeginMultiplexPaymentWithPaymentId(MultiplexPayment multiplexPayment);

		[DispId(46)]
		bool BeginMultiplexPaymentWithVas(VasMultiplexPayment multiplexPayment);

		[DispId(7)]
		bool BeginSale(string amount);

		[DispId(50)]
		bool BeginSaleWithAsanKharid(string amount, string installmentCount);

		[DispId(38)]
		bool BeginSaleWithExtraParam(string amount, string extraparam);

		[DispId(39)]
		bool BeginSaleWithExtraParamAndPaymentId(string amount, string extraparam, string paymentid);

		[DispId(44)]
		bool BeginSaleWithExtraParamAndPrintableInfo(string amount, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(8)]
		bool BeginSaleWithPaymentId(string amount, string paymentId);

		[DispId(41)]
		bool BeginSaleWithPaymentIdAndPrintableInfo(string amount, string paymentid, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(42)]
		bool BeginSaleWithPaymentIdExtraParamAndPrintableInfo(string amount, string paymentid, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(40)]
		bool BeginSaleWithPrintableInfo(string amount, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(11)]
		bool BeginSpecialPayment(string amount);

		[DispId(55)]
		bool BeginVasDynamicSale(VasPaymentItem[] vasPaymentItems);

		[DispId(6)]
		string BillPayment(string billid, string billpaymentid);

		[DispId(5)]
		string CreditRemaining();

		[DispId(30)]
		void Dispose();

		[DispId(47)]
		string ESpecialPayment(string amount, string paymentId);

		[DispId(26)]
		void InitiateService();

		[DispId(27)]
		void InitiateService(string serialNo, string acceptorId, string terminalId, string ipAddress, int port, int timeOut = 200);

		[DispId(52)]
		void InitiateService(string serialNo, string acceptorId, string terminalId, string comPort, int baudRate, int dataBits, SerialPortStopBit stopBit, SerialPortParity parity, int timeOut = 200);

		[DispId(53)]
		void InitiateService(string serialNo, string acceptorId, string terminalId, string comPort, int baudRate, int dataBits, int stopBit, int parity, int timeOut = 200);

		[DispId(2)]
		string MultiplexPayment(MultiplexPayment multiplexPayment);

		[DispId(3)]
		string MultiplexPaymentWithPaymentId(MultiplexPayment multiplexPayment);

		[DispId(45)]
		string MultiplexPaymentWithVas(VasMultiplexPayment multiplexPayment);

		[DispId(29)]
		void ResetService();

		[DispId(0)]
		string Sale(string amount);

		[DispId(51)]
		string SaleEncrypted(string message);

		[DispId(49)]
		string SaleWithAsanKharid(string amount, string installmentCount);

		[DispId(33)]
		string SaleWithExtraParam(string amount, string extraparam);

		[DispId(34)]
		string SaleWithExtraParamAndPaymentId(string amount, string extraparam, string paymentid);

		[DispId(43)]
		string SaleWithExtraParamAndPrintableInfo(string amount, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(1)]
		string SaleWithPaymentId(string amount, string paymentId);

		[DispId(36)]
		string SaleWithPaymentIdAndPrintableInfo(string amount, string paymentid, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(37)]
		string SaleWithPaymentIdExtraParamAndPrintableInfo(string amount, string paymentid, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(35)]
		string SaleWithPrintableInfo(string amount, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04);

		[DispId(4)]
		string SpecialPayment(string amount);

		[DispId(28)]
		void TerminateService();

		[DispId(54)]
		string VasDynamicSale(VasPaymentItem[] vasPaymentItems);

		[DispId(14)]
		event ResponseReceivedEventHandler ResponseReceived;
	}
}