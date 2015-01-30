using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace GSF.ServiceModel.Activation
{
    /// <summary>
    /// WCF Endpoint behavior intended to be attached to REST endpoints.
    /// Attaches <see cref="FormatSpecificationInspector"/> to inspect requests and alter their formats as required.
    /// </summary>
    public class FormatSpecificationBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new FormatSpecificationInspector());
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
