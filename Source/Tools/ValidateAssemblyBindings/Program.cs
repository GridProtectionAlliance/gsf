//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  02/07/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using GSF;
using GSF.IO;

namespace ValidateAssemblyBindings
{
    // NOTE: The build output folder for this application is intentionally the "Libraries" location
    // so that the tool will be available for GSF.Web dependent downstream applications as part of
    // the nightly build roll-down process.
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length is < 1 or > 2)
            {
                Console.Error.WriteLine("USAGE: ValidateAssemblyBindings <appConfigFileName> <leaveExistingBindings=false;exceptions=name1:newVersion1,name2:newVersion2,...>");
                Console.Error.WriteLine();
                Console.Error.WriteLine("    Example: ValidateAssemblyBindings openPDC.exe.config");
                Console.Error.WriteLine("    Example: ValidateAssemblyBindings openHistorian.exe.config exceptions=System.Diagnostics.DiagnosticSource:4.0.4.0");
                Console.Error.WriteLine("    Example: ValidateAssemblyBindings SEBrowser.dll.config leaveExistingBindings=true;exceptions=System.Diagnostics.DiagnosticSource:4.0.4.0");
                Console.Error.WriteLine();
                return 1;
            }

            try
            {
                string appConfigFileName = FilePath.GetAbsolutePath(args[0]);
                Dictionary<string, string> exceptions = new(StringComparer.OrdinalIgnoreCase);
                bool leaveExistingBindings = false;

                if (!File.Exists(appConfigFileName))
                    throw new FileNotFoundException($"Application configuration file name \"{appConfigFileName}\" not found.");

                if (args.Length == 2)
                {
                    Dictionary<string, string> parameters = args[1].ParseKeyValuePairs();

                    leaveExistingBindings = parameters.TryGetValue("leaveExistingBindings", out string value) && value.ParseBoolean();

                    if (parameters.TryGetValue("exceptions", out value))
                    {
                        string[] exceptionArgs = value.Split(',');

                        foreach (string exceptionArg in exceptionArgs)
                        {
                            string[] parts = exceptionArg.Split(':');

                            if (parts.Length != 2)
                                throw new ArgumentException($"Invalid exception argument \"{exceptionArg}\".");

                            exceptions.Add(parts[0], parts[1]);
                        }
                    }
                }

                Console.WriteLine($"Validating assembly bindings for \"{appConfigFileName}\"");
                
                if (ValidateAssemblyBindings(appConfigFileName, leaveExistingBindings, exceptions))
                {
                    Console.WriteLine("Assembly bindings validation succeeded.");
                    return 0;
                }
                else
                {
                    Console.Error.WriteLine("Assembly bindings validation failed.");
                    return 3;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: Failed to validate assembly bindings: {ex.Message}");
                Console.Error.WriteLine();
                return 2;
            }
        }

        // Validates the assembly bindings for the specified application <paramref name="configFileName"/>.
        private static bool ValidateAssemblyBindings(string configFileName, bool leaveExistingBindings, Dictionary<string, string> exceptions)
        {
            if (!File.Exists(configFileName))
                return false;

            XmlDocument configFile = new();
            configFile.Load(configFileName);

            XmlNode runTime = configFile.SelectSingleNode("configuration/runtime");

            if (runTime == null)
            {
                XmlNode config = configFile.SelectSingleNode("configuration");

                // This is expected to already exist...
                if (config == null)
                    return false;

                runTime = configFile.CreateElement("runtime");
                config.AppendChild(runTime);

                XmlElement gcServer = configFile.CreateElement("gcServer");
                XmlAttribute enabled = configFile.CreateAttribute("enabled");
                enabled.Value = "true";

                gcServer.Attributes.Append(enabled);
                runTime.AppendChild(gcServer);

                XmlElement gcConcurrent = configFile.CreateElement("gcConcurrent");
                enabled = configFile.CreateAttribute("enabled");
                enabled.Value = "true";

                gcConcurrent.Attributes.Append(enabled);
                runTime.AppendChild(gcConcurrent);

                XmlElement generatePublisherEvidence = configFile.CreateElement("generatePublisherEvidence");
                enabled = configFile.CreateAttribute("enabled");
                enabled.Value = "false";

                generatePublisherEvidence.Attributes.Append(enabled);
                runTime.AppendChild(generatePublisherEvidence);
            }

            const string xmlns = "urn:schemas-microsoft-com:asm.v1";
            XmlNamespaceManager nsmgr = new(configFile.NameTable);
            nsmgr.AddNamespace("s", xmlns);

            if (!leaveExistingBindings)
            {
                // Remove any existing assembly bindings
                XmlElement[] existingAssemblyBindings = runTime.ChildNodes.Cast<XmlElement>().Where(node => node.Name == "assemblyBinding").ToArray();

                foreach (XmlElement element in existingAssemblyBindings)
                    runTime.RemoveChild(element);
            }

            XmlDocument assemblyBindingsXml = new();
            XmlElement assemblyBinding = assemblyBindingsXml.CreateElement("assemblyBinding", xmlns);
            string[] assemblyFileNames = FilePath.GetFileList(FilePath.GetAbsolutePath("*.dll"));

            foreach (string assemblyFileName in assemblyFileNames)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(assemblyFileName);
                    AssemblyName assemblyName = assembly.GetName();
                    string version = assemblyName.Version?.ToString();

                    if (version is null or "0.0.0.0" or "1.0.0.0")
                        continue;

                    StringBuilder keyTokenImage = new();
                    byte[] keyTokenBytes = assemblyName.GetPublicKeyToken();

                    if (keyTokenBytes is not null)
                    {
                        foreach (byte keyToken in keyTokenBytes)
                            keyTokenImage.Append($"{keyToken:x2}");
                    }

                    XmlElement dependentAssembly = assemblyBindingsXml.CreateElement("dependentAssembly", xmlns);

                    // Load assembly identity element
                    XmlElement assemblyIdentity = assemblyBindingsXml.CreateElement("assemblyIdentity", xmlns);

                    XmlAttribute name = assemblyBindingsXml.CreateAttribute("name");
                    name.Value = assemblyName.Name;
                    assemblyIdentity.Attributes.Append(name);

                    if (keyTokenImage.Length > 0)
                    {
                        XmlAttribute publicKeyToken = assemblyBindingsXml.CreateAttribute("publicKeyToken");
                        publicKeyToken.Value = keyTokenImage.ToString();
                        assemblyIdentity.Attributes.Append(publicKeyToken);
                    }

                    string cultureName = assemblyName.CultureName;

                    if (string.IsNullOrWhiteSpace(cultureName))
                        cultureName = "neutral";

                    XmlAttribute culture = assemblyBindingsXml.CreateAttribute("culture");
                    culture.Value = cultureName;
                    assemblyIdentity.Attributes.Append(culture);

                    dependentAssembly.AppendChild(assemblyIdentity);

                    // Load binding redirect element
                    XmlElement bindingRedirect = assemblyBindingsXml.CreateElement("bindingRedirect", xmlns);

                    XmlAttribute oldVersion = assemblyBindingsXml.CreateAttribute("oldVersion");
                    oldVersion.Value = $"1.0.0.0-{version}";
                    bindingRedirect.Attributes.Append(oldVersion);

                    XmlAttribute newVersion = assemblyBindingsXml.CreateAttribute("newVersion");

                    newVersion.Value = exceptions.TryGetValue(assemblyName.Name, out string exceptionVersion) ? exceptionVersion : version;

                    bindingRedirect.Attributes.Append(newVersion);

                    dependentAssembly.AppendChild(bindingRedirect);

                    // Add new dependent assembly to binding redirect
                    assemblyBinding.AppendChild(dependentAssembly);
                }
                catch (Exception ex)
                {
                    if (ex is not BadImageFormatException)
                        Console.WriteLine($"Skipping \"{assemblyFileName}\": {ex.Message}");
                }
            }
            
            assemblyBindingsXml.AppendChild(assemblyBinding);

            XmlDocumentFragment assemblyBindings = configFile.CreateDocumentFragment();
            assemblyBindings.InnerXml = assemblyBindingsXml.InnerXml;

            XmlNode oldAssemblyBindings = configFile.SelectSingleNode("configuration/runtime/s:assemblyBinding", nsmgr);

            if (oldAssemblyBindings is null)
                runTime.AppendChild(assemblyBindings);
            else
                runTime.ReplaceChild(assemblyBindings, oldAssemblyBindings);

            configFile.Save(configFileName);

            return true;
        }
    }
}