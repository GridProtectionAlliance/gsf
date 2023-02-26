//******************************************************************************************************
//  ModuleInitializer.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  02/25/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GSF.Diagnostics;
using GSF.IO;

// ReSharper disable MustUseReturnValue
namespace GSF.Web
{
    // This class is used to establish a module initializer for the GSF.Web assembly
    // See: https://github.com/kzu/InjectModuleInitializer
    internal static class ModuleInitializer
    {
        private const string SourceNamespace = $"{nameof(GSF)}.{nameof(Web)}.";

        internal static void Run()
        {
            const string EventName = $"{nameof(ModuleInitializer)}.{nameof(Run)}";

            LogPublisher log = Logger.CreatePublisher(typeof(ModuleInitializer), MessageClass.Framework);

            try
            {
                (int restorationCount, int validationCount) = RestoreEmbeddedResources(new[]
                {
                    // Using NUglify to remove dependency on AjaxMin which causes version
                    // conflicts with WebGrease, commonly used in IIS applications. Also,
                    // NUglify fixes general minifier issues encountered with AjaxMin.
                    // See: https://github.com/trullock/NUglify
                    "NUglify.dll"
                });

                log.Publish(MessageLevel.Info, EventName, $"Embedded resource file restoration complete. {restorationCount:N0} files restored, {validationCount:N0} existing files validated.");
            }
            catch (Exception ex)
            {
                log.Publish(MessageLevel.Error, EventName, $"Embedded resource file restoration failed: {ex.Message}", exception: ex);
            }
        }

        // Restoring assemblies from an embedded resource allows us to avoid having
        // to roll down another dependency assembly. Note that assemblies cannot be
        // directly loaded from embedded resource since the Razor engine, which is
        // based on Roslyn, can only use assemblies loaded from files.
        private static (int, int) RestoreEmbeddedResources(IEnumerable<string> assemblyNames)
        {
            Assembly entryAssembly = typeof(ModuleInitializer).Assembly;
            string targetPath = FilePath.AddPathSuffix(FilePath.GetAbsolutePath(""));
            int restorationCount = 0, validationCount = 0;
            HashSet<string> targetResources = new(assemblyNames.Select(assemblyName => 
                $"{SourceNamespace}{assemblyName}"), StringComparer.OrdinalIgnoreCase);
            
            // This simple file restoration assumes embedded resources to be restored are in root namespace
            foreach (string name in entryAssembly.GetManifestResourceNames().Where(targetResources.Contains))
            {
                using Stream resourceStream = entryAssembly.GetManifestResourceStream(name);

                if (resourceStream is null)
                    continue;

                string filePath = name;

                // Remove namespace prefix from resource file name
                if (filePath.StartsWith(SourceNamespace))
                    filePath = filePath.Substring(SourceNamespace.Length);

                string targetFileName = Path.Combine(targetPath, filePath);
                bool restoreFile = true;

                if (File.Exists(targetFileName))
                {
                    string resourceMD5 = GetMD5HashFromStream(resourceStream);
                    resourceStream.Seek(0, SeekOrigin.Begin);
                    restoreFile = !resourceMD5.Equals(GetMD5HashFromFile(targetFileName));
                }

                if (!restoreFile)
                {
                    validationCount++;
                    continue;
                }

                if (string.Compare(Path.GetExtension(targetFileName), ".exe", StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.Compare(Path.GetExtension(targetFileName), ".dll", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using FileStream writer = File.Create(targetFileName);
                    resourceStream.CopyTo(writer);
                }
                else
                {
                    byte[] buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, (int)resourceStream.Length);

                    using StreamWriter writer = File.CreateText(targetFileName);
                    writer.Write(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                }

                restorationCount++;
            }

            return (restorationCount, validationCount);
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            using FileStream stream = File.OpenRead(fileName);
            return GetMD5HashFromStream(stream);
        }

        private static string GetMD5HashFromStream(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }
}
