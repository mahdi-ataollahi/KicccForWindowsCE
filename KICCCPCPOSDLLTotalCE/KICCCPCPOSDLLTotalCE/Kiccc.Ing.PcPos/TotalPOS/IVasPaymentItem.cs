using System;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos.TotalPOS
{
    [ComVisible(true)]
    [Guid("F4B58E4E-4ACD-4FE7-A49B-C6A8FDEC4719")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IVasPaymentItem
    {
        string Amount
        {
            get;
            set;
        }

        string Terminal
        {
            get;
            set;
        }
    }
}