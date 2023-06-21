using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace GrafanaAdapters.GrafanaFunctions
{
    internal class GrafanaFunction : IFunctionsModel
    {
        private const string ExpressionFormat = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";

        public GrafanaFunction(string functionName, string regexName, string functionDescription, List<IParameter> parameters, Action<List<object>> computeLogic)
        {
            Name = functionName;
            Regex = new Regex(string.Format(ExpressionFormat, regexName), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Description = functionDescription;
            Parameters = parameters;
            ComputeLogic = computeLogic;

        }

        public string Name { get; }
        public Regex Regex { get; }

        public string Description { get; }
        public List<IParameter> Parameters { get; }

        private Action<List<object>> ComputeLogic { get; }

        public void Compute(List<object> values)
        {
            ComputeLogic(values);
        }
    }

    internal class Parameter<T> : IParameter
    {
        public Type Default { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public Type Type { get; } = typeof(T);
    }

    internal class QueryDataHolder
    {
        public Target SourceTarget { get;  }
        public DateTime StartTime { get; }
        public DateTime StopTime { get;  }
        public string Interval { get;  }
        public bool IncludePeaks { get;  }
        public bool DropEmptySeries { get; }

        public CancellationToken CancellationToken { get; }

        public QueryDataHolder(Target sourceTarget, DateTime startTime, DateTime stopTime, string interval, bool includePeaks, bool dropEmptySeries, CancellationToken cancellationToken)
        {
            SourceTarget = sourceTarget;
            StartTime = startTime;
            StopTime = stopTime;
            Interval = interval;
            IncludePeaks = includePeaks;
            DropEmptySeries = dropEmptySeries;
            CancellationToken = cancellationToken;
        }
    }
}
