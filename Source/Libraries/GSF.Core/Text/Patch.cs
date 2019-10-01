//******************************************************************************************************
//  Patch.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  09/17/2019 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#region [ Contributor License Agreements ]

/*
 * Diff Match and Patch
 * Copyright 2018 The diff-match-patch Authors.
 * https://github.com/google/diff-match-patch
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GSF.Text
{
    /// <summary>
    /// Class representing one patch operation.
    /// </summary>
    public class Patch
    {
        /// <summary>
        /// List of <see cref="Diff"/>s in the patch
        /// </summary>
        public List<Diff> Diffs { get; set; } = new List<Diff>();

        /// <summary>
        /// Source line number (zero-based index)
        /// </summary>
        public int Start1 { get; set; }

        /// <summary>
        /// Target line number (zero-based index)
        /// </summary>
        public int Start2 { get; set; }

        /// <summary>
        /// Number of lines from source contained in this patch
        /// </summary>
        public int Length1 { get; set; }

        /// <summary>
        /// Number of lines from target contained in this patch
        /// </summary>
        public int Length2 { get; set; }

        /// <summary>
        /// Emulate GNU diff's format.
        /// Header: @@ -382,8 +481,9 @@
        /// Indices are printed as 1-based, not 0-based.
        /// </summary>
        /// <returns>The GNU diff string</returns>
        public override string ToString()
        {
            string coords1, coords2;

            if (Length1 == 0)
                coords1 = Start1 + ",0";
            else if (Length1 == 1)
                coords1 = Convert.ToString(Start1 + 1);
            else
                coords1 = (Start1 + 1) + "," + Length1;

            if (Length2 == 0)
                coords2 = Start2 + ",0";
            else if (Length2 == 1)
                coords2 = Convert.ToString(Start2 + 1);
            else
                coords2 = (Start2 + 1) + "," + Length2;

            StringBuilder text = new StringBuilder()
                .Append("@@ -")
                .Append(coords1)
                .Append(" +")
                .Append(coords2)
                .Append(" @@\n");

            // Escape the body of the patch with %xx notation.
            foreach (Diff aDiff in Diffs)
            {
                switch (aDiff.Operation)
                {
                    case Operation.INSERT:
                        text.Append('+');
                        break;

                    case Operation.DELETE:
                        text.Append('-');
                        break;

                    case Operation.EQUAL:
                        text.Append(' ');
                        break;
                }

                text.Append(DiffMatchPatch.EncodeURI(aDiff.Text)).Append("\n");
            }

            return text.ToString();
        }
    }
}
