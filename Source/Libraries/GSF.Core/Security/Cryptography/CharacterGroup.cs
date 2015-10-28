//******************************************************************************************************
//  CharacterGroup.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  10/27/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;

namespace GSF.Security.Cryptography
{
    /// <summary>
    /// Defines a group of characters that can
    /// appear in an automatically generated password.
    /// </summary>
    public class CharacterGroup
    {
        #region [ Members ]

        // Fields
        private string m_characters;
        private int m_minOccurrence;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CharacterGroup"/> class.
        /// </summary>
        public CharacterGroup()
            : this(" ")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CharacterGroup"/> class.
        /// </summary>
        /// <param name="characters">String representing the characters in the character group.</param>
        public CharacterGroup(string characters)
            : this(characters, 0)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CharacterGroup"/> class.
        /// </summary>
        /// <param name="characters">String representing the characters in the character group.</param>
        /// <param name="minOccurrence">The minimum number of occurrences of this character group in the password.</param>
        public CharacterGroup(string characters, int minOccurrence)
        {
            Characters = characters;
            MinOccurrence = minOccurrence;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// The collection of characters in the character group.
        /// </summary>
        public string Characters
        {
            get
            {
                return m_characters;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Character group must have at least one character.", nameof(value));

                m_characters = value;
            }
        }

        /// <summary>
        /// The minimum number of occurrences of any of the
        /// characters in this character group in the password.
        /// </summary>
        public int MinOccurrence
        {
            get
            {
                return m_minOccurrence;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                m_minOccurrence = value;
            }
        }

        #endregion
    }
}
