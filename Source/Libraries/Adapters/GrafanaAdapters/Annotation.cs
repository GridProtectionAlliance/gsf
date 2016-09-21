//******************************************************************************************************
//  Annotation.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/12/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace GrafanaAdapters
{
    /// <summary>
    /// Supported annotation types.
    /// </summary>
    public enum AnnotationType
    {
        /// <summary>
        /// Raised alarm annotation.
        /// </summary>
        RaisedAlarms,

        /// <summary>
        /// Cleared alarm annotation.
        /// </summary>
        ClearedAlarms,

        /// <summary>
        /// Undefined annotation.
        /// </summary>
        Undefined
    }

    /// <summary>
    /// Defines a Grafana annotation.
    /// </summary>
    public class Annotation
    {
        /// <summary>
        /// Annotation name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Annotation data source.
        /// </summary>
        public string datasource { get; set; }

        /// <summary>
        /// Annotation enabled flag.
        /// </summary>
        public bool enable { get; set; }

        /// <summary>
        /// Annotation icon color.
        /// </summary>
        public string iconColor { get; set; }

        /// <summary>
        /// Annotation query.
        /// </summary>
        public string query { get; set; }
    }

    /// <summary>
    /// Grafana <see cref="Annotation"/> extensions class.
    /// </summary>
    public static class AnnotationExtensions
    {
        /// <summary>
        /// Gets table name for specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <returns>Table name for specified annotation <paramref name="type"/>.</returns>
        public static string TableName(this AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                    return "RaisedAlarms";
                case AnnotationType.ClearedAlarms:
                    return "ClearedAlarms";
                default:
                    return "Undefined";
            }
        }

        private static string Translate(this string tableName)
        {
            // Source metadata for raised and cleared alarms is the same
            if (tableName.Equals("RaisedAlarms", StringComparison.OrdinalIgnoreCase))
                return "Alarms";

            if (tableName.Equals("ClearedAlarms", StringComparison.OrdinalIgnoreCase))
                return "Alarms";

            return tableName;
        }

        /// <summary>
        /// Gets the targets, e.g., point IDs, from a set of metadata definitions for specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <param name="definitions">Metadata definitions.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetTargets(this AnnotationType type, DataRow[] definitions)
        {
            string targetFieldName;

            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                case AnnotationType.ClearedAlarms:
                    targetFieldName = "AssociatedMeasurementID";
                    break;
                default:
                    throw new InvalidOperationException("Cannot extract target for specified annotation type.");
            }

            return definitions.Select(row => MeasurementKey.LookUpBySignalID(Guid.Parse(row[targetFieldName].ToString())).ToString());
        }

        /// <summary>
        /// Extracts a Grafana <see cref="QueryRequest"/> from an <see cref="AnnotationRequest"/>.
        /// </summary>
        /// <param name="request">Annotation request.</param>
        /// <param name="targets">List of desired targets.</param>
        /// <param name="maxDataPoints">Maximum points to return.</param>
        /// <returns>Grafana query request object from an annotation <paramref name="request"/>.</returns>
        public static QueryRequest ExtractQueryRequest(this AnnotationRequest request, IEnumerable<string> targets, int maxDataPoints)
        {
            return new QueryRequest
            {
                range = request.range,
                rangeRaw = request.rangeRaw,
                interval = "*",
                targets = targets.Select((target, index) => new Target { refId = $"ID{index}", target = target }).ToList(),
                format = "json",
                maxDataPoints = maxDataPoints
            };
        }

        /// <summary>
        /// Parses query expression from annotation for annotation type.
        /// </summary>
        /// <param name="annotation">Grafana annotation.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed annotation type for query expression from <paramref name="annotation"/>.</returns>
        public static AnnotationType ParseQueryType(this Annotation annotation, out bool useFilterExpression)
        {
            if ((object)annotation == null)
                throw new ArgumentNullException(nameof(annotation));

            string query = annotation.query ?? "";
            AnnotationType type = AnnotationType.Undefined;
            useFilterExpression = false;

            if (query.StartsWith("#RaisedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.RaisedAlarms;
            }
            else if (query.StartsWith("FILTER RaisedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.RaisedAlarms;
                useFilterExpression = true;
            }
            else if (query.StartsWith("#ClearedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.ClearedAlarms;
            }
            else if (query.StartsWith("FILTER ClearedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.ClearedAlarms;
                useFilterExpression = true;
            }

            if (type == AnnotationType.Undefined)
                throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");

            return type;
        }

        /// <summary>
        /// Parses query expression from annotation request for annotation type.
        /// </summary>
        /// <param name="request">Grafana annotation request.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed annotation type for query expression from annotation <paramref name="request"/>.</returns>
        public static AnnotationType ParseQueryType(this AnnotationRequest request, out bool useFilterExpression)
        {
            if ((object)request == null)
                throw new ArgumentNullException(nameof(request));

            return request.annotation.ParseQueryType(out useFilterExpression);
        }

        /// <summary>
        /// Parses source definitions for an annotation query.
        /// </summary>
        /// <param name="annotation">Grafana annotation.</param>
        /// <param name="type">Annotation type.</param>
        /// <param name="source">Metadata of source definitions.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed source definitions from <paramref name="annotation"/>.</returns>
        public static DataRow[] ParseSourceDefinitions(this Annotation annotation, AnnotationType type, DataSet source, bool useFilterExpression)
        {
            if ((object)annotation == null)
                throw new ArgumentNullException(nameof(annotation));

            if ((object)source == null)
                throw new ArgumentNullException(nameof(source));

            if (type == AnnotationType.Undefined)
                throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");

            string query = annotation.query ?? "";
            DataRow[] events;

            if (useFilterExpression)
            {
                string tableName, expression, sortField;
                int takeCount;

                if (AdapterBase.ParseFilterExpression(query, out tableName, out expression, out sortField, out takeCount))
                    events = source.Tables[tableName.Translate()].Select(expression, sortField).Take(takeCount).ToArray();
                else
                    throw new InvalidOperationException("Invalid FILTER syntax for annotation query expression.");
            }
            else
            {
                events = source.Tables[type.TableName().Translate()].Rows.Cast<DataRow>().ToArray();
            }

            return events;
        }

        /// <summary>
        /// Parses source definitions for an annotation query.
        /// </summary>
        /// <param name="request">Grafana annotation request.</param>
        /// <param name="type">Annotation type.</param>
        /// <param name="source">Metadata of source definitions.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed source definitions from annotation <paramref name="request"/>.</returns>
        public static DataRow[] ParseSourceDefinitions(this AnnotationRequest request, AnnotationType type, DataSet source, bool useFilterExpression)
        {
            if ((object)request == null)
                throw new ArgumentNullException(nameof(request));

            return request.annotation.ParseSourceDefinitions(type, source, useFilterExpression);
        }
    }
}
