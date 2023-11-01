using System;

namespace Kiccc.Ing.PcPos.TotalPOSClient
{
	internal enum SocketState
	{
		Waiting,
		Aborted,
		Accepting,
		Reading,
		Sending,
		DropingConnecction,
		Connected
	}
}