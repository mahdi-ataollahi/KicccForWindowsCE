using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Kiccc.Ing.PcPos
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("DE154DC2-1265-47C3-8F9A-EC92692D6EFF")]
    [ProgId("Kiccc.Ing.PcPos.MultiplexPayment")]
    [Serializable]
    public class MultiplexPayment : IMultiplexPayment
    {
        private string _paymentId;

        private string _organization;

        private string _serviceCode;

        private string _sequenceCode;

        private string _financialyear;

        private string _fiscalPeriod;

        private string _bankCode;

        private string _specialPaymentId;

        public AmountList Amounts { get; set; } = new AmountList();

        public string BankCode
        {
            get
            {
                return this._bankCode;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9]{1}[1-9]{1}$"))
                {
                    throw new Exception("Invalid BankCode");
                }
                this._bankCode = value;
            }
        }

        public string Financialyear
        {
            get
            {
                return this._financialyear;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[1-9]{1}[0-9]{1}$"))
                {
                    throw new Exception("Invalid Financialyear");
                }
                this._financialyear = value;
            }
        }

        public string FiscalPeriod
        {
            get
            {
                return this._fiscalPeriod;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[1-9]{1}[0-9]{1,2}$"))
                {
                    throw new Exception("Invalid FiscalPeriod");
                }
                this._fiscalPeriod = value;
            }
        }

        public string Organization
        {
            get
            {
                return this._organization;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9]{1}[0-9]{1}$"))
                {
                    throw new Exception("Invalid OrganizationId");
                }
                this._organization = value;
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
                if (!Regex.IsMatch(value, "^[0-9]{1,10}$"))
                {
                    throw new Exception("Invalid PaymentId");
                }
                this._paymentId = value;
            }
        }

        public string SequenceCode
        {
            get
            {
                return this._sequenceCode;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9]{0,2}[1-9]{1}$"))
                {
                    throw new Exception("Invalid SequenceId");
                }
                this._sequenceCode = value;
            }
        }

        public string ServiceCode
        {
            get
            {
                return this._serviceCode;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9]{1,2}$"))
                {
                    throw new Exception("Invalid ServiceId");
                }
                this._serviceCode = value;
            }
        }

        public string SpecialPaymentId
        {
            get
            {
                return this._specialPaymentId;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9]{1,18}$"))
                {
                    throw new Exception("Invalid SpecialPaymentId");
                }
                this._specialPaymentId = value;
            }
        }

        public MultiplexPayment()
        {
        }
    }
}