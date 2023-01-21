//******************************************************************************************************
//  IReportingProcess.cs - Gbtc
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
using System.Collections.Generic;
using GSF.Configuration;
using GSF.Console;

namespace GSF.TimeSeries.Reports
{
    /// <summary>
    /// Defines an interface for reporting processes.
    /// </summary>
    public interface IReportingProcess : IProvideStatus, IPersistSettings
    {
        /// <summary>
        /// Gets report type (i.e., name) for this reporting process.
        /// </summary>
        string ReportType { get; }

        /// <summary>
        /// Gets or sets the path to the archive file to which the statistics required for reporting are archived.
        /// </summary>
        string ArchiveFilePath { get; set; }

        /// <summary>
        /// Gets or sets the directory to which reports will be written.
        /// </summary>
        string ReportLocation { get; set; }

        /// <summary>
        /// Gets or sets the title to be displayed on reports.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the name of the company to be displayed on reports.
        /// </summary>
        string Company { get; set; }

        /// <summary>
        /// Gets or sets the minimum lifetime of a report
        /// since the last time it was accessed, in days.
        /// </summary>
        double IdleReportLifetime { get; set; }

        /// <summary>
        /// Gets or sets flag to enable e-mailing of reports.
        /// </summary>
        bool EnableReportEmail { get; set; }

        /// <summary>
        /// Gets or sets SMTP server to use when e-mailing reports.
        /// </summary>
        string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the "from" address to use when e-mailing reports.
        /// </summary>
        string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the comma separated "to" addresses to use when e-mailing reports. 
        /// </summary>
        string ToAddresses { get; set; }

        /// <summary>
        /// Returns the list of reports that are available from the report location.
        /// </summary>
        /// <returns>The list of generated reports.</returns>
        List<string> GetReportsList();

        /// <summary>
        /// Returns the list of reports which are in the queue but are yet to be generated.
        /// </summary>
        /// <returns>The list of pending reports.</returns>
        List<string> GetPendingReportsList();

        /// <summary>
        /// Queues up a report to be generated on a separate thread.
        /// </summary>
        /// <param name="reportDate">The date of the report to be generated.</param>
        /// <param name="emailReport">Flag that determines if report should be e-mailed, if enabled.</param>
        void GenerateReport(DateTime reportDate, bool emailReport);

        /// <summary>
        /// Deletes reports from the <see cref="ReportLocation"/> that have been idle for the length of the <see cref="IdleReportLifetime"/>.
        /// </summary>
        void CleanReportLocation();

        /// <summary>
        /// Gets the command line arguments for the reporting process.
        /// </summary>
        string GetArguments();

        /// <summary>
        /// Gets the command line arguments for the reporting process for a given report date.
        /// </summary>
        /// <param name="reportDate">The date of the report to be generated.</param>
        /// <param name="emailReport">Flag that determines if report should be e-mailed, if enabled.</param>
        string GetArguments(DateTime reportDate, bool emailReport);

        /// <summary>
        /// Applies any received command line arguments for the reporting process.
        /// </summary>
        /// <param name="args">Received command line arguments.</param>
        void SetArguments(Arguments args);
    }
}