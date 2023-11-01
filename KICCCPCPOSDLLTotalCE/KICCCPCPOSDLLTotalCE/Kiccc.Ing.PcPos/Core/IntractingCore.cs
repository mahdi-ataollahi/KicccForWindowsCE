using Kiccc.Ing.PcPos;
using Kiccc.Ing.PcPos.Multiplex;
using Kiccc.Ing.PcPos.TotalPOS;
using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Kiccc.Ing.PcPos.Core
{
    public abstract class IntractingCore
    {
        public const string TerminalRegexPattern = "^[0-9]{8}$";

        public const string AcceptorRegexPattern = "^[0-9]{12,15}$";

        public const string SerialNoRegexPattern = "^[0-9]{1}[0-9]{7,11}$";

        public const string ComPortRegexPattern = "^COM[1-9]{1}[0-9]{0,1}$";

        public bool _isDisposed;

        public string _comPort;

        public string _acceptorId;

        public string _terminalId;

        public string _serialNo;

        public Thread _currentOps;

        internal System.IO.Ports.SerialPort SerialPort;

        public int _timeOut;

        public ConnetionTypes _connetionType;

        public ReadyState State
        {
            get;
            set;
        }

        protected IntractingCore()
        {
        }

        public bool BeginBillPayment(string billid, string billpaymentid)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.BillPayment(billid, billpaymentid);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginCreditRemaining()
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = this.CreditRemaining()
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginESpecialPayment(string amount, string paymentId)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.ESpecialPayment(amount, paymentId);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginMultiplexPayment(MultiplexPayment multiplexPayment)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.MultiplexPayment(multiplexPayment);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginMultiplexPaymentWithPaymentId(MultiplexPayment multiplexPayment)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.MultiplexPaymentWithPaymentId(multiplexPayment);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginMultiplexPaymentWithVas(VasMultiplexPayment multiplexPayment)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.MultiplexPaymentWithVas(multiplexPayment);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSale(string amount)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.Sale(amount);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithAsanKharid(string amount, string installmentCount)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithAsanKharid(amount, installmentCount);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithExtraParam(string amount, string extraparam)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithExtraParam(amount, extraparam);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithExtraParamAndPaymentId(string amount, string extraparam, string paymentid)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithExtraParamAndPaymentId(amount, extraparam, paymentid);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithExtraParamAndPrintableInfo(string amount, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithExtraParamAndPrintableInfo(amount, extraparam, printableinfoLine01, printableinfoLine02, printableinfoLine03, printableinfoLine04);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithPaymentId(string amount, string paymentId)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithPaymentId(amount, paymentId);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithPaymentIdAndPrintableInfo(string amount, string paymentid, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithPaymentIdAndPrintableInfo(amount, paymentid, printableinfoLine01, printableinfoLine02, printableinfoLine03, printableinfoLine04);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithPaymentIdExtraParamAndPrintableInfo(string amount, string paymentid, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithPaymentIdExtraParamAndPrintableInfo(amount, paymentid, extraparam, printableinfoLine01, printableinfoLine02, printableinfoLine03, printableinfoLine04);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSaleWithPrintableInfo(string amount, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SaleWithPrintableInfo(amount, printableinfoLine01, printableinfoLine02, printableinfoLine03, printableinfoLine04);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginSpecialPayment(string amount)
        {
            this.CheckObjectDisposeStatus();
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.SpecialPayment(amount);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = Total.ParseMessage(Total.MakeFackeResponse(FakeResponseType.AsynchException))
                    });
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public bool BeginVasDynamicSale(VasPaymentItem[] vasPaymentItems)
        {
            this._currentOps = new Thread(() =>
            {
                try
                {
                    string result = this.VasDynamicSale(vasPaymentItems);
                    this.OnResponseReceived(new ResponseReceivedEventArgs(null)
                    {
                        Response = result
                    });
                }
                catch (ThreadAbortException threadAbortException)
                {
                    this.State = ReadyState.Ready;
                }
                catch (Exception exception)
                {
                    this.State = ReadyState.Ready;
                    throw;
                }
            })
            {
                Priority = ThreadPriority.AboveNormal
            };
            this._currentOps.Start();
            return true;
        }

        public string BillPayment(string billid, string billpaymentid)
        {
            this.CheckObjectDisposeStatus();
            if (!Regex.IsMatch(billid, "^[0-9]{1,20}$"))
            {
                throw new Exception("Invalid BillId");
            }
            if (!Regex.IsMatch(billpaymentid, "^[0-9]{1,20}$"))
            {
                throw new Exception("Invalid BillPaymentId");
            }
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.BillPayment);
            msg.Amount = "0";
            msg.BillId = billid;
            msg.BillPaymentId = billpaymentid;
            msg.Command = 6.ToString();
            return this.Communicate(msg);
        }

        public void CalcelSale()
        {
            this.JustSend(-10.ToString());
        }

        public static string Calculate(VasPaymentItem[] paymentItems)
        {
            int sum = 0;
            VasPaymentItem[] vasPaymentItemArray = paymentItems;
            for (int i = 0; i < (int)vasPaymentItemArray.Length; i++)
            {
                sum += int.Parse(vasPaymentItemArray[i].Amount);
            }
            return sum.ToString();
        }

        private static void CheckAmount(string amount)
        {
            if (string.IsNullOrEmpty(amount) | !Regex.IsMatch(amount, "^[1-9]{1}[0-9]{3,11}$"))
            {
                throw new ArgumentException("amount");
            }
        }

        private static void CheckExtraParam(string extraparam)
        {
            if ((string.IsNullOrEmpty(extraparam) ? true : !Regex.IsMatch(extraparam, "^([a-z|A-Z|0-9]){1,99}$")))
            {
                throw new ArgumentException("extraparam");
            }
        }

        private static void CheckIban(string iban)
        {
            if (!Regex.IsMatch(iban, "^IR[0-9]{24}$"))
            {
                throw new ArgumentException(string.Format("Invalid IBAN ,{0}", iban), "iban");
            }
        }

        private static void Checkinstallment(string installment)
        {
            if ((string.IsNullOrEmpty(installment) ? true : !Regex.IsMatch(installment, "^[0-9]{1,2}$")))
            {
                throw new ArgumentException("installment");
            }
        }

        private static void CheckMultiAmount(string amount)
        {
            if (string.IsNullOrEmpty(amount) | !Regex.IsMatch(amount, "^[0-9]{1,12}$"))
            {
                throw new ArgumentException("amount");
            }
        }

        internal void CheckObjectDisposeStatus()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException("Ingenico", "Please Initiate Service");
            }
        }

        private static void CheckPaymentId(string paymentid)
        {
            if ((string.IsNullOrEmpty(paymentid) ? true : !Regex.IsMatch(paymentid, "^[0-9]{1,18}$")))
            {
                throw new ArgumentException("paymentid");
            }
        }

        private static void CheckPrintableInfo(string printableinfo)
        {
        }

        public abstract string Communicate(Total.MessageStructure msg);

        public abstract string Communicate(string msg);

        public string CreditRemaining()
        {
            this.CheckObjectDisposeStatus();
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.CreditRemaining);
            msg.Amount = "0";
            msg.Command = 3.ToString();
            return this.Communicate(msg);
        }

        public string ESpecialPayment(string amount, string paymentId)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.ESpecialPayment);
            msg.Command = 1.ToString();
            msg.Amount = amount;
            msg.PaymentId = paymentId;
            return this.Communicate(msg);
        }

        public abstract void JustSend(string msg);

        public static string MakeMultiplexAmountForVasDynamic(string prefix, VasPaymentItem[] paymentItems)
        {
            string result = prefix;
            if (prefix == "100")
            {
                result = string.Concat(result, string.Format("#{0}", (int)paymentItems.Length));
            }
            VasPaymentItem[] vasPaymentItemArray = paymentItems;
            for (int i = 0; i < (int)vasPaymentItemArray.Length; i++)
            {
                VasPaymentItem vasPaymentItem = vasPaymentItemArray[i];
                string iban = vasPaymentItem.Iban;
                result = string.Concat(result, string.Format("#{0},{1}", iban, vasPaymentItem.Amount));
            }
            return result;
        }

        public string MultiplexPayment(MultiplexPayment multiplexPayment)
        {
            this.CheckObjectDisposeStatus();
            MultiplexParameterGenerator paramGen = new MultiplexParameterGenerator();
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Multiplex);
            msg.Command = 2.ToString();
            msg.Amount = multiplexPayment.Amounts.TotalAmount;
            msg.BankCode = multiplexPayment.BankCode;
            msg.MultiPlexAmount = multiplexPayment.Amounts.MultiplexAmount;
            msg.MultiplexAmountQty = multiplexPayment.Amounts.Count.ToString();
            msg.PaymentNumber = paramGen.GetDepositIdentifiers(multiplexPayment.PaymentId, multiplexPayment.Organization, multiplexPayment.ServiceCode, multiplexPayment.SequenceCode, multiplexPayment.Amounts.AmountDeduction, multiplexPayment.Financialyear, multiplexPayment.FiscalPeriod, multiplexPayment.BankCode);
            msg.ServiceId = paramGen.GetServiceIdentifiers(multiplexPayment.PaymentId, multiplexPayment.Organization, multiplexPayment.ServiceCode, multiplexPayment.SequenceCode, multiplexPayment.Amounts.AmountDeduction, multiplexPayment.Financialyear, multiplexPayment.FiscalPeriod, multiplexPayment.BankCode);
            return this.Communicate(msg);
        }

        public string MultiplexPaymentWithPaymentId(MultiplexPayment multiplexPayment)
        {
            this.CheckObjectDisposeStatus();
            MultiplexParameterGenerator paramGen = new MultiplexParameterGenerator();
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.MultiplexWithPaymentId);
            msg.Command = 5.ToString();
            msg.Amount = multiplexPayment.Amounts.TotalAmount;
            msg.BankCode = multiplexPayment.BankCode;
            msg.MultiPlexAmount = multiplexPayment.Amounts.MultiplexAmount;
            msg.MultiplexAmountQty = multiplexPayment.Amounts.Count.ToString();
            msg.PaymentNumber = paramGen.GetDepositIdentifiers(multiplexPayment.PaymentId, multiplexPayment.Organization, multiplexPayment.ServiceCode, multiplexPayment.SequenceCode, multiplexPayment.Amounts.AmountDeduction, multiplexPayment.Financialyear, multiplexPayment.FiscalPeriod, multiplexPayment.BankCode);
            msg.ServiceId = paramGen.GetServiceIdentifiers(multiplexPayment.PaymentId, multiplexPayment.Organization, multiplexPayment.ServiceCode, multiplexPayment.SequenceCode, multiplexPayment.Amounts.AmountDeduction, multiplexPayment.Financialyear, multiplexPayment.FiscalPeriod, multiplexPayment.BankCode);
            msg.PaymentId = multiplexPayment.SpecialPaymentId;
            return this.Communicate(msg);
        }

        public string MultiplexPaymentWithVas(VasMultiplexPayment multiplexPayment)
        {
            this.CheckObjectDisposeStatus();
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.VasMultiplex);
            msg.Command = 8.ToString();
            msg.Amount = multiplexPayment.Amounts.TotalAmount;
            msg.MultiPlexAmount = multiplexPayment.Amounts.MultiplexAmount;
            msg.PaymentId = multiplexPayment.PaymentId;
            msg.VasMultiplexIndex = multiplexPayment.Index.ToString();
            return this.Communicate(msg);
        }

        protected virtual void OnResponseReceived(ResponseReceivedEventArgs e)
        {
            (new EventHelper()).RaiseAsync(this.ResponseReceived, new object[] { this, e });
        }

        public string Sale(string amount)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Sale);
            msg.Command = 0.ToString();
            msg.Amount = amount;
            return this.Communicate(msg);
        }

        public string SaleEncrypted(string message)
        {
            return this.Communicate(message);
        }

        public string SaleWithAsanKharid(string amount, string installmentCount)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            IntractingCore.Checkinstallment(installmentCount);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Special);
            msg.Command = 4.ToString();
            msg.Amount = amount;
            msg.Installments = installmentCount;
            return this.Communicate(msg);
        }

        public string SaleWithExtraParam(string amount, string extraparam)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            IntractingCore.CheckExtraParam(extraparam);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Extra);
            msg.Command = 7.ToString();
            msg.Amount = amount;
            msg.ExtraParam = extraparam;
            msg.ExtraParamLenght = extraparam.Length.ToString();
            return this.Communicate(msg);
        }

        public string SaleWithExtraParamAndPaymentId(string amount, string extraparam, string paymentid)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            IntractingCore.CheckExtraParam(extraparam);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Extra);
            msg.Command = 7.ToString();
            msg.Amount = amount;
            msg.PaymentId = paymentid;
            msg.ExtraParam = extraparam;
            msg.ExtraParamLenght = extraparam.Length.ToString();
            return this.Communicate(msg);
        }

        public string SaleWithExtraParamAndPrintableInfo(string amount, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            IntractingCore.CheckPrintableInfo(printableinfoLine01);
            IntractingCore.CheckPrintableInfo(printableinfoLine02);
            IntractingCore.CheckPrintableInfo(printableinfoLine03);
            IntractingCore.CheckPrintableInfo(printableinfoLine04);
            IntractingCore.CheckExtraParam(extraparam);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Extra);
            msg.Command = 7.ToString();
            msg.Amount = amount;
            msg.PrintableInfoLine01Lenght = ((printableinfoLine01 != null ? printableinfoLine01.Length : 0)).ToString();
            msg.PrintableInfoLine01 = printableinfoLine01;
            msg.PrintableInfoLine02Lenght = ((printableinfoLine02 != null ? printableinfoLine02.Length : 0)).ToString();
            msg.PrintableInfoLine02 = printableinfoLine02;
            msg.PrintableInfoLine03Lenght = ((printableinfoLine03 != null ? printableinfoLine03.Length : 0)).ToString();
            msg.PrintableInfoLine03 = printableinfoLine03;
            msg.PrintableInfoLine04Lenght = ((printableinfoLine04 != null ? printableinfoLine04.Length : 0)).ToString();
            msg.PrintableInfoLine04 = printableinfoLine04;
            msg.ExtraParam = extraparam;
            msg.ExtraParamLenght = extraparam.Length.ToString();
            return this.Communicate(msg);
        }

        public string SaleWithPaymentId(string amount, string paymentId)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.SaleWithPaymentId);
            msg.Command = 1.ToString();
            msg.Amount = amount;
            msg.PaymentId = paymentId;
            return this.Communicate(msg);
        }

        public string SaleWithPaymentIdAndPrintableInfo(string amount, string paymentid, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            IntractingCore.CheckPrintableInfo(printableinfoLine01);
            IntractingCore.CheckPrintableInfo(printableinfoLine02);
            IntractingCore.CheckPrintableInfo(printableinfoLine03);
            IntractingCore.CheckPrintableInfo(printableinfoLine04);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Extra);
            msg.Command = 7.ToString();
            msg.Amount = amount;
            msg.PaymentId = paymentid;
            msg.PrintableInfoLine01Lenght = ((printableinfoLine01 != null ? printableinfoLine01.Length : 0)).ToString();
            msg.PrintableInfoLine01 = printableinfoLine01;
            msg.PrintableInfoLine02Lenght = ((printableinfoLine02 != null ? printableinfoLine02.Length : 0)).ToString();
            msg.PrintableInfoLine02 = printableinfoLine02;
            msg.PrintableInfoLine03Lenght = ((printableinfoLine03 != null ? printableinfoLine03.Length : 0)).ToString();
            msg.PrintableInfoLine03 = printableinfoLine03;
            msg.PrintableInfoLine04Lenght = ((printableinfoLine04 != null ? printableinfoLine04.Length : 0)).ToString();
            msg.PrintableInfoLine04 = printableinfoLine04;
            return this.Communicate(msg);
        }

        public string SaleWithPaymentIdExtraParamAndPrintableInfo(string amount, string paymentid, string extraparam, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            IntractingCore.CheckPrintableInfo(printableinfoLine01);
            IntractingCore.CheckPrintableInfo(printableinfoLine02);
            IntractingCore.CheckPrintableInfo(printableinfoLine03);
            IntractingCore.CheckPrintableInfo(printableinfoLine04);
            IntractingCore.CheckExtraParam(extraparam);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Extra);
            msg.Command = 7.ToString();
            msg.Amount = amount;
            msg.PaymentId = paymentid;
            msg.PrintableInfoLine01Lenght = ((printableinfoLine01 != null ? printableinfoLine01.Length : 0)).ToString();
            msg.PrintableInfoLine01 = printableinfoLine01;
            msg.PrintableInfoLine02Lenght = ((printableinfoLine02 != null ? printableinfoLine02.Length : 0)).ToString();
            msg.PrintableInfoLine02 = printableinfoLine02;
            msg.PrintableInfoLine03Lenght = ((printableinfoLine03 != null ? printableinfoLine03.Length : 0)).ToString();
            msg.PrintableInfoLine03 = printableinfoLine03;
            msg.PrintableInfoLine04Lenght = ((printableinfoLine04 != null ? printableinfoLine04.Length : 0)).ToString();
            msg.PrintableInfoLine04 = printableinfoLine04;
            msg.ExtraParam = extraparam;
            msg.ExtraParamLenght = extraparam.Length.ToString();
            return this.Communicate(msg);
        }

        public string SaleWithPrintableInfo(string amount, string printableinfoLine01, string printableinfoLine02, string printableinfoLine03, string printableinfoLine04)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            IntractingCore.CheckPrintableInfo(printableinfoLine01);
            IntractingCore.CheckPrintableInfo(printableinfoLine02);
            IntractingCore.CheckPrintableInfo(printableinfoLine03);
            IntractingCore.CheckPrintableInfo(printableinfoLine04);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Extra);
            msg.Command = 7.ToString();
            msg.Amount = amount;
            msg.PrintableInfoLine01Lenght = ((printableinfoLine01 != null ? printableinfoLine01.Length : 0)).ToString();
            msg.PrintableInfoLine01 = printableinfoLine01;
            msg.PrintableInfoLine02Lenght = ((printableinfoLine02 != null ? printableinfoLine02.Length : 0)).ToString();
            msg.PrintableInfoLine02 = printableinfoLine02;
            msg.PrintableInfoLine03Lenght = ((printableinfoLine03 != null ? printableinfoLine03.Length : 0)).ToString();
            msg.PrintableInfoLine03 = printableinfoLine03;
            msg.PrintableInfoLine04Lenght = ((printableinfoLine04 != null ? printableinfoLine04.Length : 0)).ToString();
            msg.PrintableInfoLine04 = printableinfoLine04;
            return this.Communicate(msg);
        }

        public string SpecialPayment(string amount)
        {
            this.CheckObjectDisposeStatus();
            IntractingCore.CheckAmount(amount);
            Total.MessageStructure msg = Total.MessageStructure.Create(MessageType.Special);
            msg.Amount = amount;
            msg.Command = 4.ToString();
            return this.Communicate(msg);
        }

        public string VasDynamicSale(VasPaymentItem[] vasPaymentItems)
        {
            this.CheckObjectDisposeStatus();
            if (vasPaymentItems == null)
            {
                throw new ArgumentException("Invalid Argument", "vasPaymentItems");
            }
            if ((int)vasPaymentItems.Length > 10)
            {
                throw new ArgumentException("The Array Lenght Exceeded", "vasPaymentItems");
            }
            VasPaymentItem[] vasPaymentItemArray = vasPaymentItems;
            for (int i = 0; i < (int)vasPaymentItemArray.Length; i++)
            {
                VasPaymentItem item = vasPaymentItemArray[i];
                IntractingCore.CheckMultiAmount(item.Amount);
                IntractingCore.CheckIban(item.Iban);
            }
            Total.MessageStructure message = Total.MessageStructure.Create(MessageType.VasDynamic);
            message.Command = 10.ToString();
            message.Amount = IntractingCore.Calculate(vasPaymentItems);
            message.MultiPlexAmount = IntractingCore.MakeMultiplexAmountForVasDynamic("100", vasPaymentItems);
            return this.Communicate(message);
        }

        public event ResponseReceivedEventHandler ResponseReceived;
    }
}