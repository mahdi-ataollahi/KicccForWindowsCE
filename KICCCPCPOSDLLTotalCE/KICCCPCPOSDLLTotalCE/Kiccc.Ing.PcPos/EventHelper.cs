using System;
using System.ComponentModel;

namespace Kiccc.Ing.PcPos
{
    internal class EventHelper
    {
        public EventHelper()
        {
        }

        public void RaiseAsync(Delegate ev, object[] args)
        {
            Delegate[] invocationList = ev.GetInvocationList();
            for (int i = 0; i < (int)invocationList.Length; i++)
            {
                Delegate d = invocationList[i];
                ISynchronizeInvoke syncer = d.Target as ISynchronizeInvoke;
                if (syncer != null)
                {
                    syncer.BeginInvoke(d, args);
                }
                else
                {
                    d.DynamicInvoke(args);
                }
            }
        }
    }
}