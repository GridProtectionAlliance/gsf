//******************************************************************************************************
//  Diff.cs - Gbtc
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

// ReSharper disable NonReadonlyMemberInGetHashCode
namespace GSF.Text
{
    /// <summary>
    /// The data structure representing a diff is a List of Diff objects:
    /// {Diff(Operation.DELETE, "Hello"), Diff(Operation.INSERT, "Goodbye"),
    ///  Diff(Operation.EQUAL, " world.")}
    /// which means: delete "Hello", add "Goodbye" and keep " world."
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// Target deletes text
        /// </summary>
        DELETE,

        /// <summary>
        /// Target inserts text
        /// </summary>
        INSERT,

        /// <summary>
        /// Source and target are the same
        /// </summary>
        EQUAL
    }

    /// <summary>
    /// Class representing one diff operation.
    /// </summary>
    /// <remarks>
    /// This class produces a dynamic hash code, do not use
    /// an instance of this class as key in a dictionary.
    /// </remarks>
    public class Diff
    {
        /// <summary>
        /// One of: INSERT, DELETE or EQUAL.
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>
        /// The text associated with this diff operation.
        /// </summary>
        public string Text;

        /// <summary>
        /// Initializes the diff with the provided values.
        /// </summary>
        /// <param name="operation">One of INSERT, DELETE or EQUAL.</param>
        /// <param name="text">The text being applied.</param>
        public Diff(Operation operation, string text)
        {
            // Construct a diff with the specified operation and text
            Operation = operation;
            Text = text;
        }

        /// <summary>
        /// Display a human-readable version of this Diff.
        /// </summary>
        /// <returns>text version</returns>
        public override string ToString()
        {
            string prettyText = Text.Replace('\n', '\u00b6');
            return "Diff(" + Operation + ",\"" + prettyText + "\")";
        }

        /// <summary>
        /// Is this Diff equivalent to another Diff?
        /// </summary>
        /// <param name="obj">Another Diff to compare against.</param>
        /// <returns>true or false</returns>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // If parameter cannot be cast to Diff return false.
            Diff p = obj as Diff;

            if (p == null)
                return false;

            // Return true if the fields match.
            return p.Operation == Operation && p.Text == Text;
        }

        /// <summary>
        /// Is this Diff equivalent to another Diff?
        /// </summary>
        /// <param name="obj">Another Diff to compare against.</param>
        /// <returns>true or false</returns>
        public bool Equals(Diff obj)
        {
            // If parameter is null return false.
            if (obj == null)
                return false;

            // Return true if the fields match.
            return obj.Operation == Operation && obj.Text == Text;
        }

        /// <summary>
        /// Hash function for the Diff.
        /// </summary>
        /// <returns>The Diff's hash code</returns>
        public override int GetHashCode()
        {
            return Text.GetHashCode() ^ Operation.GetHashCode();
        }
    }
}
