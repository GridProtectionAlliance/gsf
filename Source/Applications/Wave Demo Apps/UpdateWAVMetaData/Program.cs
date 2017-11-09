//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/28/2011 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;
using GSF.Media;
using GSF.Units;

namespace UpdateWAVMetaData
{
    class Program
    {
        static int Main(string[] args)
        {
            // System settings
            ConfigurationFile configFile = ConfigurationFile.Current;
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
            systemSettings.Add("NodeID", Guid.NewGuid().ToString(), "Unique Node ID");
            Guid nodeID = systemSettings["NodeID"].ValueAs<Guid>();
            bool useMemoryCache = systemSettings["UseMemoryCache"].ValueAsBoolean(false);
            string connectionString = systemSettings["ConnectionString"].Value;
            string nodeIDQueryString = null;
            string parameterizedQuery;
            int protocolID, signalTypePMID, signalTypePAID;

            // Define guid with query string delimiters according to database needs
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
            string setting;

            if (settings.TryGetValue("Provider", out setting))
            {
                // Check if provider is for Access since it uses braces as Guid delimiters
                if (setting.StartsWith("Microsoft.Jet.OLEDB", StringComparison.OrdinalIgnoreCase))
                    nodeIDQueryString = "{" + nodeID + "}";
            }

            if (string.IsNullOrWhiteSpace(nodeIDQueryString))
                nodeIDQueryString = "'" + nodeID + "'";

            using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
            {
                IDbConnection connection = database.Connection;

                if (Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM Protocol WHERE Acronym='WAV'")) == 0)
                {
                    if (database.IsSQLServer || database.IsJetEngine)
                        connection.ExecuteNonQuery("INSERT INTO Protocol(Acronym, Name, [Type], Category, AssemblyName, TypeName) VALUES('WAV', 'Wave Form Input Adapter', 'Frame', 'Audio', 'WavInputAdapter.dll', 'WavInputAdapter.WavInputAdapter')");
                    else
                        connection.ExecuteNonQuery("INSERT INTO Protocol(Acronym, Name, Type, Category, AssemblyName, TypeName) VALUES('WAV', 'Wave Form Input Adapter', 'Frame', 'Audio', 'WavInputAdapter.dll', 'WavInputAdapter.WavInputAdapter')");
                }

                protocolID = Convert.ToInt32(connection.ExecuteScalar("SELECT ID FROM Protocol WHERE Acronym='WAV'"));

                // Typically these values should be defined as analogs, however, we use a voltage magnitude signal type
                // since these types of values can be better graphed with auto-scaling in the visualization tools 
                signalTypePMID = Convert.ToInt32(connection.ExecuteScalar("SELECT ID FROM SignalType WHERE Acronym='VPHM'"));
                signalTypePAID = Convert.ToInt32(connection.ExecuteScalar("SELECT ID FROM SignalType WHERE Acronym='VPHA'"));

                string pathRoot = FilePath.GetDirectoryName((args.Length > 0) ? args[0] : systemSettings["MusicDirectory"].Value);
                string sourcePath = Path.Combine(pathRoot, "*" + Path.DirectorySeparatorChar + "*.wav");

                foreach (string sourceFileName in FilePath.GetFileList(sourcePath))
                {
                    WaveFile sourceWave;
                    string fileName = FilePath.GetFileName(sourceFileName);
                    char[] invalidChars = { '\'', '[', ']', '(', ')', ',', '-', '.' };

                    Console.WriteLine("Loading metadata for \"{0}\"...\r\n", fileName);
                    sourceWave = WaveFile.Load(sourceFileName, false);

                    fileName = FilePath.GetFileNameWithoutExtension(fileName).RemoveDuplicateWhiteSpace().RemoveCharacters(invalidChars.Contains).Trim();
                    string acronym = fileName.Replace(' ', '_').ToUpper() + "_" + (int)(sourceWave.SampleRate / SI.Kilo) + "KHZ";
                    string name = GenerateSongName(sourceWave, fileName);

                    Console.WriteLine("   Acronym = {0}", acronym);
                    Console.WriteLine("      Name = {0}", name);
                    Console.WriteLine("");

                    // Check to see if device exists
                    if (Convert.ToInt32(connection.ExecuteScalar(database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE Acronym = {0}", "acronym"), acronym)) == 0)
                    {
                        parameterizedQuery = database.ParameterizedQueryString("INSERT INTO Device(NodeID, Acronym, Name, ProtocolID, FramesPerSecond, " +
                            "MeasurementReportingInterval, ConnectionString, Enabled) VALUES(" + nodeIDQueryString + ", {0}, {1}, {2}, {3}, {4}, {5}, {6})",
                            "acronym", "name", "protocolID", "framesPerSecond", "measurementReportingInterval",
                            "connectionString", "enabled");

                        // Insert new device record
                        connection.ExecuteNonQuery(parameterizedQuery, acronym, name, protocolID, sourceWave.SampleRate, 1000000, $"wavFileName={FilePath.GetAbsolutePath(sourceFileName)}; connectOnDemand=true; outputSourceIDs={acronym}; memoryCache={useMemoryCache}", database.Bool(true));
                        int deviceID = Convert.ToInt32(connection.ExecuteScalar(database.ParameterizedQueryString("SELECT ID FROM Device WHERE Acronym = {0}", "acronym"), acronym));
                        string pointTag;
                        int lastPhasorIndex = 0;
                        int phasorIndex = 1;

                        // Add a measurement for each defined wave channel
                        for (int i = 0; i < sourceWave.Channels; i++)
                        {
                            int index = i + 1;
                            int signalTypeID = index % 2 == 0 ? signalTypePAID : signalTypePMID;

                            if (i > 0 && i % 2 == 0)
                                phasorIndex++;

                            if (lastPhasorIndex != phasorIndex)
                            {
                                lastPhasorIndex = phasorIndex;

                                parameterizedQuery = database.ParameterizedQueryString("INSERT INTO Phasor(DeviceID, Label, Type, Phase, SourceIndex) VALUES ({0}, {1}, 'V', '+', {2})", "deviceID", "label", "sourceIndex");

                                // Insert new phasor record
                                connection.ExecuteNonQuery(parameterizedQuery, (object)deviceID, acronym, phasorIndex);
                            }

                            string signalSuffix = index % 2 == 0 ? "-PA" : "-PM";
                            pointTag = acronym + ":WAVA" + index;

                            parameterizedQuery = database.ParameterizedQueryString("INSERT INTO Measurement(DeviceID, PointTag, SignalTypeID, PhasorSourceIndex, SignalReference, Description, " +
                                "Enabled) VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6})", "deviceID", "pointTag", "signalTypeID", "phasorSourceIndex", "signalReference", "description", "enabled");

                            // Insert new measurement record
                            connection.ExecuteNonQuery(parameterizedQuery, (object)deviceID, pointTag, signalTypeID, phasorIndex, acronym + signalSuffix + phasorIndex, name + " - channel " + phasorIndex, database.Bool(true));
                            //Convert.ToInt32(connection.ExecuteScalar(database.ParameterizedQueryString("SELECT PointID FROM Measurement WHERE PointTag = {0}", "pointTag"), pointTag));
                        }

                        // Disable all non analog measurements that may be associated with this device
                        connection.ExecuteNonQuery(database.ParameterizedQueryString("UPDATE Measurement SET Enabled = {0} WHERE DeviceID = {1} AND NOT SignalTypeID IN ({2}, {3})", "enabled", "deviceID", "signalTypePAID", "signalTypePMID"), database.Bool(false), deviceID, signalTypePAID, signalTypePMID);
                    }
                }
            }

            return 0;
        }

        private static string GenerateSongName(WaveFile song, string fileName)
        {
            Dictionary<string, string> infoStrings = song.InfoStrings;

            string title = GetInfoProperty(infoStrings, "INAM", fileName).Trim();
            string artist = GetInfoProperty(infoStrings, "IART", "").Trim();
            string track = GetInfoProperty(infoStrings, "ITRK", "").Trim();
            string length = " " + song.AudioLength.ToString("hh\\:mm\\:ss");
            string sampleRate = " @ " + (song.SampleRate / SI.Kilo).ToString("0.00kHz");

            if (!string.IsNullOrEmpty(artist))
            {
                if (string.Compare(artist, "Unknown artist", true) == 0)
                    artist = fileName;

                artist = ", " + artist;
            }

            if (!string.IsNullOrEmpty(track))
                track = " #" + track + ",";

            string suffix;

            if (string.IsNullOrEmpty(track) && string.IsNullOrEmpty(artist))
                suffix = sampleRate;
            else
                suffix = " -" + track + length + sampleRate;

            if (title.Length + suffix.Length > 200)
                return title.TruncateRight(199 - suffix.Length) + " " + suffix;

            if (title.Length + artist.Length + suffix.Length > 200)
                return title + artist.TruncateRight(200 - title.Length - suffix.Length) + " " + suffix;

            return title + artist + suffix;
        }

        private static string GetInfoProperty(Dictionary<string, string> infoStrings, string key, string defaultValue)
        {
            string value;

            if (infoStrings.TryGetValue(key, out value))
            {
                value = value.Trim().RemoveDuplicateWhiteSpace();

                if (string.IsNullOrEmpty(value))
                    return defaultValue;

                return value;
            }

            return defaultValue;
        }
    }
}
