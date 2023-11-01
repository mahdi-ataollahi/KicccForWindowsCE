using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kiccc.Ing.PcPos
{
    //[ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("5d9817cb-177d-46d2-9cbe-07834eee6a3d")]
    //[ProgId("Kiccc.Ing.PcPos.ResponseReceivedEventArgs")]
    public class ResponseReceivedEventArgs : EventArgs, IResponseReceivedEventArgs
    {
        public string Response
        {
            get
            {
                return JustDecompileGenerated_get_Response();
            }
            set
            {
                JustDecompileGenerated_set_Response(value);
            }
        }

        private string JustDecompileGenerated_Response_k__BackingField;

        public string JustDecompileGenerated_get_Response()
        {
            return this.JustDecompileGenerated_Response_k__BackingField;
        }

        internal void JustDecompileGenerated_set_Response(string value)
        {
            this.JustDecompileGenerated_Response_k__BackingField = value;
        }

        public ResponseReceivedEventArgs()
            : this(null)
        {
        }

        public ResponseReceivedEventArgs(string response)
        {
            this.Response = response;
        }
    }
}