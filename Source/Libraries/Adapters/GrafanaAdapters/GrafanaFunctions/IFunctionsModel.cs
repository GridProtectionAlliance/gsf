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
        IEnumerable<DataSourceValue> Compute(string[] values, IEnumerable<DataSourceValue> dataSourceValues);
    }

    internal interface IParameter
    {
        string Description { get; set; }
        bool Required { get; set; }
    }

    internal interface IParameter<T> : IParameter
    {
        T Default { get; set; }
    }
}
