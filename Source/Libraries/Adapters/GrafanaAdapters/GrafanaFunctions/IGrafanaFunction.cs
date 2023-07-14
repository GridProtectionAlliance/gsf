using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrafanaAdapters.GrafanaFunctions
{
    /// <summary>
    /// Defines a common interface for Grafana functions.
    /// </summary>
    public interface IGrafanaFunction
    {
        /// <summary>
        /// Gets the name of the Grafana function.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the Grafana function.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the type of the Grafana function.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the regular expression for matching the Grafana function.
        /// </summary>
        Regex Regex { get; }

        /// <summary>
        /// Gets the list of parameters of the Grafana function.
        /// </summary>
        List<IParameter> Parameters { get; }

        /// <summary>
        /// Performs the computation of the Grafana function.
        /// </summary>
        /// <param name="parameters">The input parameters for the computation.</param>
        /// <returns>A sequence of computed data source parameters.</returns>
        DataSourceValueGroup<DataSourceValue> Compute(List<IParameter> parameters);
        /// <summary>
        /// Performs the computation for phasor of the Grafana function.
        /// </summary>
        /// <param name="parameters">The input parameters for the computation.</param>
        /// <returns>A sequence of computed data source parameters.</returns>
        DataSourceValueGroup<PhasorValue> ComputePhasor(List<IParameter> parameters);
    }

    /// <summary>
    /// Defines a common interface for parameters of Grafana functions.
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Gets or sets the description of the parameter.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the parameter is required.
        /// </summary>
        bool Required { get; set; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        Type ParameterType { get; }

        /// <summary>
        /// Gets or sets the type name of the parameter.
        /// </summary>
        string ParameterTypeName { get; set; }
        /// <summary>
        /// Sets the value of the parameter.
        /// </summary>
        void SetValue(GrafanaDataSourceBase dataSourceBase, object value, string target, Dictionary<string, string> metadata);
    }

    /// <summary>
    /// Defines a common interface for parameters of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    public interface IParameter<T> : IParameter
    {
        /// <summary>
        /// Gets or sets the default value of the parameter.
        /// </summary>
        T Default { get; set; }

        /// <summary>
        /// Gets or sets the actual value of the parameter.
        /// </summary>
        T Value { get; set;  }
        
    }

}
