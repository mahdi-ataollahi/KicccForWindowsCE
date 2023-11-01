using System;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
    [ComVisible(true)]
    [Guid("722395d6-beb8-4aec-98a4-3821053ce0bb")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IResponseReceivedEventArgs
    {
        string Response
        {
            get;
        }
    }
}