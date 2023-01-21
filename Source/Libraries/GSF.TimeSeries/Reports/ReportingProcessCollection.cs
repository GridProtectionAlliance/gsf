//******************************************************************************************************
//  ReportingProcessCollection.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/09/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.ObjectModel;
using System.Text;
using GSF.TimeSeries.Adapters;

namespace GSF.TimeSeries.Reports
{
    /// <summary>
    /// Represents a collection of <see cref="IReportingProcess"/> items.
    /// </summary>
    public class ReportingProcessCollection : Collection<IReportingProcess>, IProvideStatus
    {
        #region [ Properties ]

        string IProvideStatus.Name => GetType().Name;

        /// <summary>
        /// Gets the descriptive status of this <see cref="AdapterCollectionBase{T}"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new();

                // Show collection status
                status.AppendFormat(" Total reporting processes: {0}", Count);
                status.AppendLine();

                if (Count > 0)
                {
                    int index = 0;

                    status.AppendLine();
                    status.AppendFormat("Status of each reporting process:");
                    status.AppendLine();
                    status.Append(new string('-', 79));
                    status.AppendLine();

                    // Show the status of registered components.
                    lock (this)
                    {
                        foreach (IReportingProcess item in this)
                        {
                            status.AppendLine();
                            status.AppendFormat("Status of reporting process [{0}] \"{1}\":", ++index, item.Name);
                            status.AppendLine();

                            try
                            {
                                status.Append(item.Status);
                            }
                            catch (Exception ex)
                            {
                                status.AppendFormat("Failed to retrieve status due to exception: {0}", ex.Message);
                                status.AppendLine();
                            }
                        }
                    }

                    status.AppendLine();
                    status.Append(new string('-', 79));
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Finds the <see cref="IReportingProcess"/> for the specified <paramref name="reportType"/> name.
        /// </summary>
        /// <param name="reportType">Name of the report type to find.</param>
        /// <returns>
        /// The <see cref="IReportingProcess"/> for the specified <paramref name="reportType"/> name, if found;
        /// otherwise, <c>null</c> if not found.
        /// </returns>
        public IReportingProcess FindReportType(string reportType)
        {
            foreach (IReportingProcess reportingProcess in this)
            {
                if (reportType.Equals(reportingProcess.ReportType, StringComparison.OrdinalIgnoreCase))
                    return reportingProcess;
            }

            return null;
        }

        /// <summary>
        /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or-
        /// <paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void InsertItem(int index, IReportingProcess item)
        {
            base.InsertItem(index, item);
            item.LoadSettings();
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to replace.</param>
        /// <param name="item">The new value for the element at the specified index. The value can be null for reference types.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or-
        /// <paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.
        /// </exception>
        protected override void SetItem(int index, IReportingProcess item)
        {
            base.SetItem(index, item);
            item.LoadSettings();
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads available <see cref="IReportingProcess"/> implementations.
        /// </summary>
        /// <param name="exceptionHandler">Exception handler, if any, to call when type creation fails; otherwise, if <c>null</c> any exceptions will be thrown.</param>
        /// <returns>New collection of <see cref="IReportingProcess"/> implementations.</returns>
        public static ReportingProcessCollection LoadImplementations(Action<Exception> exceptionHandler = null)
        {
            ReportingProcessCollection reportingProcesses = new();
            IReportingProcess reportingProcess;

            foreach (Type reportingProcessType in typeof(IReportingProcess).LoadImplementations())
            {
                reportingProcess = null;

                try
                {
                    // Try to load the reporting process implementation
                    reportingProcess = Activator.CreateInstance(reportingProcessType) as IReportingProcess;
                }
                catch (Exception ex)
                {
                    if ((object)exceptionHandler is not null)
                        exceptionHandler(ex);
                    else
                        throw;
                }

                if (reportingProcess is not null)
                    reportingProcesses.Add(reportingProcess);
            }

            return reportingProcesses;
        }

        #endregion
    }
}