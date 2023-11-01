using System;
using System.Runtime.CompilerServices;

namespace Kiccc.Ing.PcPos
{
    [Serializable]
    public class PaymentRequest
    {
        public string AcceptorId
        {
            get;
            set;
        }

        public string Amount
        {
            get;
            set;
        }

        public string BankCode
        {
            get;
            set;
        }

        public string BaudRate
        {
            get;
            set;
        }

        public string BillId
        {
            get;
            set;
        }

        public string BillPaymentId
        {
            get;
            set;
        }

        public string ComPort
        {
            get;
            set;
        }

        public string DataBits
        {
            get;
            set;
        }

        public string Financialyear
        {
            get;
            set;
        }

        public string FiscalPeriod
        {
            get;
            set;
        }

        public string[] MultiplexAmounts
        {
            get;
            set;
        }

        public string Organization
        {
            get;
            set;
        }

        public string Parity
        {
            get;
            set;
        }

        public string PaymentId
        {
            get;
            set;
        }

        public string SequenceCode
        {
            get;
            set;
        }

        public string SerialNo
        {
            get;
            set;
        }

        public string ServiceCode
        {
            get;
            set;
        }

        public string SpecialPaymentId
        {
            get;
            set;
        }

        public string StopBit
        {
            get;
            set;
        }

        public string TerminalId
        {
            get;
            set;
        }

        public string TimeOut
        {
            get;
            set;
        }

        public string TransactionType
        {
            get;
            set;
        }

        public PaymentRequest()
        {
        }
    }
}