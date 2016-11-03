//******************************************************************************************************
//  PathHelpers.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  10/24/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//       
//
//******************************************************************************************************

using System;
using System.IO;

namespace GSF.IO
{
    /// <summary>
    /// Helper methods for path strings.
    /// </summary>
    public static class PathHelpers
    {
        
        /// <summary>
        /// Ensures the supplied path name is valid.
        /// </summary>
        /// <param name="pathName">any path.</param>
        /// <remarks>
        /// throws a series of exceptions if the <see param="pathName"/> is invalid.
        /// </remarks>
        public static void ValidatePathName(string pathName)
        {
            if (pathName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException("pathName", "Extension cannot be null or empty space");
            }

            if (pathName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("filename has invalid characters.", "value");
        }

     
    }
}
