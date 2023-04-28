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
using System.Linq;
using System.Text;
using System.Threading;
using GSF.Diagnostics;

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
        /// Gets the descriptive status of this <see cref="Collection{T}"/>.
        /// </summary>
        public virtual string Status
        {
            get
            {
                StringBuilder status = new();

                // Show collection status
                status.AppendLine($" Total reporting processes: {Count}");

                if (Count > 0)
                {
                    int index = 0;

                    status.AppendLine();
                    status.AppendLine("Status of each reporting process:");
                    status.AppendLine(new string('-', 79));

                    // Show the status of registered components.
                    lock (this)
                    {
                        foreach (IReportingProcess item in this)
                        {
                            status.AppendLine();
                            status.AppendLine($"Status of reporting process [{++index}] \"{item.Name}\":");

                            try
                            {
                                status.Append(item.Status);
                            }
                            catch (Exception ex)
                            {
                                status.AppendLine($"Failed to retrieve status due to exception: {ex.Message}");
                            }
                        }
                    }

                    status.AppendLine();
                    status.AppendLine(new string('-', 79));
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Loads available <see cref="IReportingProcess"/> implementations.
        /// </summary>
        /// <param name="newReportProcessHandler">New report process handler to call when new <see cref="IReportingProcess"/> implementations are loaded.</param>
        /// <param name="exceptionHandler">Exception handler, if any, to call when type creation fails; otherwise, if <c>null</c> any exceptions will be thrown.</param>
        public void LoadImplementations(Action<IReportingProcess> newReportProcessHandler, Action<Exception> exceptionHandler = null)
        {
            // Manually load known reporting processes as an optimization
            Add(new CompletenessReportingProcess());
            Add(new CorrectnessReportingProcess());

            foreach (IReportingProcess reportingProcess in this)
                newReportProcessHandler(reportingProcess);

            // Load any user defined reporting processes on a background thread
            new Thread(() =>
            {
                try
                {
                    foreach (Type reportingProcessType in typeof(IReportingProcess).LoadImplementations())
                    {
                        if (reportingProcessType == typeof(CompletenessReportingProcess) || reportingProcessType == typeof(CorrectnessReportingProcess))
                            continue;

                        IReportingProcess reportingProcess = null;

                        try
                        {
                            // Try to load the reporting process implementation
                            reportingProcess = Activator.CreateInstance(reportingProcessType) as IReportingProcess;
                        }
                        catch (Exception ex)
                        {
                            if (exceptionHandler is not null)
                                exceptionHandler(ex);
                            else
                                throw;
                        }

                        if (reportingProcess is null)
                            continue;

                        Add(reportingProcess);
                        newReportProcessHandler(reportingProcess);
                    }
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex);
                }
            })
            {
                IsBackground = true
            }
            .Start();
        }

        /// <summary>
        /// Finds the <see cref="IReportingProcess"/> for the specified <paramref name="reportType"/> name.
        /// </summary>
        /// <param name="reportType">Name of the report type to find.</param>
        /// <returns>
        /// The <see cref="IReportingProcess"/> for the specified <paramref name="reportType"/> name, if found;
        /// otherwise, <c>null</c> if not found.
        /// </returns>
        public IReportingProcess FindReportType(string reportType) => 
            this.FirstOrDefault(reportingProcess => reportType.Equals(reportingProcess.ReportType, StringComparison.OrdinalIgnoreCase));

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
            item?.LoadSettings();
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
            item?.LoadSettings();
        }

        #endregion
    }
}