using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace GSF.ServiceModel.Activation
{
    /// <summary>
    /// Message inspector intended to be attached to WCF REST endpoints.
    /// Examines incoming requests for "application/json" in the "Accept" header, and sets the response format to Json when present.
    /// </summary>
    public class FormatSpecificationInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            WebOperationContext context = WebOperationContext.Current;
            if (context == null)
            {
                return null;
            }
            var acceptHeader = context.IncomingRequest.Headers["Accept"];
            if (acceptHeader != null && acceptHeader.Contains("application/json"))
            {
                context.OutgoingResponse.Format = WebMessageFormat.Json;
            }
            return null;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
        }
    }
}
