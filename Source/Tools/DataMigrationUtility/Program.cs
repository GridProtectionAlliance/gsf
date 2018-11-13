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
//  08/15/2010 - Mihir Brahmbhatt
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GSF;
using GSF.Data;
using GSF.IO;

namespace DataMigrationUtility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Any() && args.All(arg => arg != "-install"))
            {
                // Code added for automation of schema serialization
                SerializeSchema(args[0]);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DataMigrationUtilityScreen());
        }

        static void SerializeSchema(string connectionString)
        {
            Schema schema = new Schema(connectionString, TableType.Table);

            using (FileStream stream = new FileStream(FilePath.GetAbsolutePath("SerializedSchema.bin"), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] schemaData = Serialization.Serialize(schema, SerializationFormat.Binary);
                stream.Write(schemaData, 0, schemaData.Length);
            }
        }
    }
}
