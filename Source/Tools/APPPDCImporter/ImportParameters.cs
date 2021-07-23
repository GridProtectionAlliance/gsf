//******************************************************************************************************
//  ScanParameters.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  10/10/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using APPPDCImporter.Model;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Data.Model;
using GSF.Diagnostics;
using GSF.Parsing;

namespace APPPDCImporter
{
    public class ImportParameters : IDisposable
    {
        // Config frame parsed from APP PDC config file
        public ConfigurationFrame APPPDCConfigFrame { get; set; }
        
        // Config frame extracted from GSF PDC database config
        public ConfigurationFrame GSFPDCConfigFrame { get; set; }

        // Config frame as edited by user, i.e., target config
        public ConfigurationFrame TargetConfigFrame { get; set; }

        public AdoDataConnection Connection { get; private set; }
        
        public TableOperations<Device> DeviceTable { get; private set; }

        public Guid NodeID { get; private set; }
        
        public string HostConfig { get; set; }

        public string EditedConnectionString { get; set; }

        private bool m_disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                Connection?.Dispose();
            }
            finally
            {
                m_disposed = true;
            }
        }

        public void InitializeConnection(string connectionString, string dataProviderString, Guid nodeID)
        {
            Connection?.Dispose();

            Connection = new AdoDataConnection(connectionString, dataProviderString);
            NodeID = nodeID;
            DeviceTable = new TableOperations<Device>(Connection);
        }

        public IEnumerable<SignalType> LoadSignalTypes(string source)
        {
            TableOperations<SignalType> signalTypeTable = new TableOperations<SignalType>(Connection);
            return signalTypeTable.QueryRecordsWhere("Source = {0}", source);
        }

        public string CreatePointTag(string deviceAcronym, string signalTypeAcronym) =>
            CreatePointTag(CompanyAcronym, deviceAcronym, null, signalTypeAcronym);

        public string CreateIndexedPointTag(string deviceAcronym, string signalTypeAcronym, int signalIndex) => 
            CreatePointTag(CompanyAcronym, deviceAcronym, null, signalTypeAcronym, null, signalIndex);

        public string CreatePhasorPointTag(string deviceAcronym, string signalTypeAcronym, string phasorLabel, string phase, int signalIndex, int baseKV) => 
            CreatePointTag(CompanyAcronym, deviceAcronym, null, signalTypeAcronym, phasorLabel, signalIndex, string.IsNullOrWhiteSpace(phase) ? '_' : phase.Trim()[0], baseKV);

        private const string DefaultPointTagNameExpression = "{CompanyAcronym}_{DeviceAcronym}[?{SignalType.Source}=Phasor[-eval{'{PhasorLabel}'.Trim().ToUpper().Replace(' ','_')}_eval{'{SignalType.Abbreviation}'.Substring(0,1)}eval{'{Phase}'=='+' ? '1' : ('{Phase}'=='-' ? '2' : '{Phase}')}[?{BaseKV}&gt;0[_{BaseKV}]][?{SignalType.Suffix}=PA[:ANG]][?{SignalType.Suffix}=PM[:MAG]]]][?{SignalType.Source}!=Phasor[:{SignalType.Acronym}[?{SignalIndex}!=-1[{SignalIndex}]]]]";

        private static TemplatedExpressionParser s_pointTagExpressionParser;
        private static Dictionary<string, DataRow> s_signalTypes;
        private static int? s_ieeeC37_118ProtocolID;
        private static string s_companyAcronym;
        private static int s_companyID;

        public int IeeeC37_118ProtocolID => s_ieeeC37_118ProtocolID ??= Connection.ExecuteScalar<int>("SELECT ID FROM Protocol WHERE Acronym='IeeeC37_118V1'");

        public string CompanyAcronym
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(s_companyAcronym))
                    return s_companyAcronym;

                try
                {
                    XDocument serviceConfig = XDocument.Load(HostConfig);

                    s_companyAcronym = serviceConfig
                        .Descendants("systemSettings")
                        .SelectMany(systemSettings => systemSettings.Elements("add"))
                        .Where(element => "CompanyAcronym".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                        .Select(element => (string)element.Attribute("value"))
                        .FirstOrDefault() ?? "GPA";

                    if (string.IsNullOrWhiteSpace(s_companyAcronym))
                        s_companyAcronym = "GPA";
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex, "Failed to initialize default company acronym");
                }

                return s_companyAcronym;
            }
        }

        public int CompanyID
        {
            get
            {
                if (s_companyID > 0)
                    return s_companyID;

                try
                {
                    s_companyID = Connection.ExecuteScalar<int>("SELECT ID FROM Company WHERE Acronym = {0}", CompanyAcronym);
                }
                catch (Exception ex)
                {
                    Logger.SwallowException(ex, "Failed to initialize default company ID");
                }

                return s_companyID;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string CreatePointTag(string companyAcronym, string deviceAcronym, string vendorAcronym, string signalTypeAcronym, string phasorLabel = null, int signalIndex = -1, char phase = '_', int baseKV = 0)
        {
            // Initialize point tag expression parser
            if (s_pointTagExpressionParser is null)
                s_pointTagExpressionParser = InitializePointTagExpressionParser();

            // Initialize signal type dictionary
            if (s_signalTypes is null)
                s_signalTypes = InitializeSignalTypes();

            Dictionary<string, string> substitutions;

            if (!s_signalTypes.TryGetValue(signalTypeAcronym, out DataRow signalTypeValues))
                throw new ArgumentOutOfRangeException(nameof(signalTypeAcronym), $"No database definition was found for signal type \"{signalTypeAcronym}\"");

            // Validate key acronyms
            if (companyAcronym is null)
                companyAcronym = "";

            if (deviceAcronym is null)
                deviceAcronym = "";

            if (vendorAcronym is null)
                vendorAcronym = "";

            if (phasorLabel is null)
                phasorLabel = "";

            companyAcronym = companyAcronym.Trim();
            deviceAcronym = deviceAcronym.Trim();
            vendorAcronym = vendorAcronym.Trim();

            // Define fixed parameter replacements
            substitutions = new Dictionary<string, string>
            {
                { "{CompanyAcronym}", companyAcronym },
                { "{DeviceAcronym}", deviceAcronym },
                { "{VendorAcronym}", vendorAcronym },
                { "{PhasorLabel}", phasorLabel },
                { "{SignalIndex}", signalIndex.ToString() },
                { "{Phase}", phase.ToString() },
                { "{BaseKV}", baseKV.ToString() }
            };

            // Define signal type field value replacements
            DataColumnCollection columns = signalTypeValues.Table.Columns;

            for (int i = 0; i < columns.Count; i++)
                substitutions.Add($"{{SignalType.{columns[i].ColumnName}}}", signalTypeValues[i].ToNonNullString());

            return s_pointTagExpressionParser.Execute(substitutions);
        }

        private Dictionary<string, DataRow> InitializeSignalTypes()
        {
            // It is expected that when a point tag is needing to be created that the database will be available
            Dictionary<string, DataRow> signalTypes = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in Connection.RetrieveData("SELECT * FROM SignalType").AsEnumerable())
                signalTypes.AddOrUpdate(row["Acronym"].ToString(), row);

            return signalTypes;
        }

        private TemplatedExpressionParser InitializePointTagExpressionParser()
        {
            TemplatedExpressionParser pointTagExpressionParser;

            // Get point tag name expression from configuration
            try
            {
                XDocument serviceConfig = XDocument.Load(HostConfig);

                string pointTagNameExpression = serviceConfig
                    .Descendants("systemSettings")
                    .SelectMany(systemSettings => systemSettings.Elements("add"))
                    .Where(element => "PointTagNameExpression".Equals((string)element.Attribute("name"), StringComparison.OrdinalIgnoreCase))
                    .Select(element => (string)element.Attribute("value"))
                    .FirstOrDefault() ?? DefaultPointTagNameExpression;

                pointTagExpressionParser = new TemplatedExpressionParser()
                {
                    TemplatedExpression = pointTagNameExpression
                };
            }
            catch
            {
                pointTagExpressionParser = new TemplatedExpressionParser()
                {
                    TemplatedExpression = DefaultPointTagNameExpression
                };
            }

            return pointTagExpressionParser;
        }
    }
}