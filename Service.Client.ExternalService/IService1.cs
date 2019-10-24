using System.Runtime.Serialization;
using System.ServiceModel;

namespace Service.Client.ExternalService
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(
        ConfigurationName = "ExternalService.MyService")]
    public interface MyService
    {
        [System.ServiceModel.OperationContractAttribute(
             Action = "http://tempuri.org/MyService/DoWork",
             ReplyAction = "http://tempuri.org/MyService/DoWorkResponse")]
        string DoWork(string left, string right);

        [System.ServiceModel.OperationContractAttribute(
            Action = "http://tempuri.org/MyService/DoWork",
            ReplyAction = "http://tempuri.org/MyService/DoWorkResponse")]
        System.Threading.Tasks.Task<string> DoWorkAsync(string left, string right);
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MyServiceClient :
        System.ServiceModel.ClientBase<Service.Client.ExternalService.MyService>,
        Service.Client.ExternalService.MyService
    {
        public MyServiceClient()
        { }

        public MyServiceClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        { }

        public MyServiceClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        { }

        public MyServiceClient(string endpointConfigurationName,
            System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        { }

        public MyServiceClient(System.ServiceModel.Channels.Binding binding,
            System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        { }

        public string DoWork(string left, string right)
        {
            return base.Channel.DoWork(left, right);
        }

        public System.Threading.Tasks.Task<string> DoWorkAsync(
            string left,
            string right)
        {
            return base.Channel.DoWorkAsync(left, right);
        }
    }
}
