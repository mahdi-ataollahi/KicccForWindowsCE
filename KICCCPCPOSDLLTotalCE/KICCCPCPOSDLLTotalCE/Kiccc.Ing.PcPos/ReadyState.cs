using System;

namespace Kiccc.Ing.PcPos
{
	public enum ReadyState
	{
		Ready,
		Busy,
		Fault,
		Disposed,
		InitializeRequired
	}
}