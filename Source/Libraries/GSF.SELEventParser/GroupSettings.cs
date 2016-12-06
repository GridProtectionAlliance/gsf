//******************************************************************************************************
//  GroupSettings.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  12/06/2016 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSF.SELEventParser
{
    public class Settings
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Properties ]


        #endregion

        #region [ Static ]

        // Static Methods

        public static Dictionary<string, string> Parse(string[] lines, ref int index)
        {
            EventFile.SkipBlanks(lines,ref index);

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            while (lines[index] !="" && index < lines.Length)
            {
                if (lines[index].Contains("="))
                {
                    string line = new string(lines[index].ToCharArray());
                    int count = line.Count(s => s == '=');

                    for (int x = 0; x < count; ++x)
                    {
                        string key = line.Split(new[] { '=' }, 2)[0].Trim();
                        line = line.Split(new[] { '=' }, 2)[1].TrimStart();
                        string value = line.Split(new[] { ' ' }, 2)[0];
                        line = line.Split(new[] { ' ' }, 2)[line.Split(new[] { ' ' }, 2).Length - 1].TrimStart();
                        dictionary.Add(key, value);
                    }
                }
                ++index;
            }


            return dictionary;
        }
        #endregion

    }

    public class ControlEquation
    {
        #region [ Members ]

        // Fields

        #endregion

        #region [ Properties ]


        #endregion

        #region [ Static ]

        // Static Methods

        public static Dictionary<string, string> Parse(string[] lines, ref int index)
        {
            EventFile.SkipBlanks(lines, ref index);

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            while (lines[index] != "" && index < lines.Length)
            {
                if (lines[index].Contains("="))
                {
                    string line = lines[index];
                    dictionary.Add(line.Split('=')[0].Trim(), line.Split('=')[1].Trim());
                }
                ++index;
            }
            return dictionary;
        }
        #endregion

    }

}
