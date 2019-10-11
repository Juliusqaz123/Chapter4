using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Chapter4
{
    [ServiceContract]
    public class MyService
    {
        [OperationContract]
        public string DoWork(string left, string right)
        {
            return left + right;
        }
    }
}
