using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    internal interface IFunctionsModel
    {
        Regex Regex { get; }
        string Name { get; }
        string Description { get; }
        List<IParameter> Parameters { get; }
        void Compute(List<object> values);
    }

    internal interface IParameter
    {
        Type Default { get; set; }
        string Description { get; set; }
        bool Required { get; set; }
        public Type Type { get; }
    }
}
