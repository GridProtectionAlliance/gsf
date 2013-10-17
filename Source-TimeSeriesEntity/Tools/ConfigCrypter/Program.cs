//******************************************************************************************************
//  Program.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/05/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GSF.Security.Cryptography;

namespace ConfigCrypter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string password;
            int? keySize;
            string keyIVText;

            GetCryptoArgs(out password, out keySize, out keyIVText);

            if ((object)password != null && (object)keySize != null)
            {
                try
                {
                    if ((object)keyIVText == null)
                        Cipher.ExportKeyIV(password, keySize.Value);
                    else
                        Cipher.ImportKeyIV(password, keySize.Value, keyIVText);

                    Cipher.FlushCache();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to update crypto cache: " + ex.Message);
                    Environment.ExitCode = 1;
                }
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Main());
            }
        }

        private static void GetCryptoArgs(out string password, out int? keySize, out string keyIVText)
        {
            List<string> args = Environment.GetCommandLineArgs().ToList();
            int passwordIndex = args.IndexOf("-password");
            int keySizeIndex = args.IndexOf("-keySize");
            int keyIVTextIndex = args.IndexOf("-keyIVText");
            int tempKeySize;

            password = null;
            keySize = null;
            keyIVText = null;

            if (passwordIndex >= 0 && passwordIndex < args.Count - 1)
                password = args[passwordIndex + 1];

            if (keySizeIndex >= 0 && keySizeIndex < args.Count - 1)
            {
                if (int.TryParse(args[keySizeIndex + 1], out tempKeySize))
                    keySize = tempKeySize;
            }

            if (keyIVTextIndex >= 0 && keyIVTextIndex < args.Count - 1)
                keyIVText = args[keyIVTextIndex + 1];
        }
    }
}
