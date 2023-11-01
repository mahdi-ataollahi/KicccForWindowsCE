using System;

namespace Kiccc.Ing.PcPos
{
    internal enum FakeResponseType
    {
        UnknownError = -999,
        Timeout = -998,
        AsynchException = -997,
        InvalidAcceptor = -203,
        InvalidTerminal = -202,
        InvalidSerial = -201,
        InvalidTransaction = -200,
        InvalidData = -100,
        CanceledByUserBeforeTrans = -1
    }
}