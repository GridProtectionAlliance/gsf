//******************************************************************************************************
//  CustomConfigurationEditorAttribute.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  07/29/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Reflection;
using GSF.IO;

namespace GSF.TimeSeries.Adapters
{
    /// <summary>
    /// Marks a parameter or class as having a custom configuration editor page used to configure that parameter or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class CustomConfigurationEditorAttribute : Attribute
    {
        #region [ Members ]

        // Fields
        private string m_assemblyName;
        private string m_typeName;
        private Type m_editorType;
        private string m_connectionString;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="CustomConfigurationEditorAttribute"/> class.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly in which the editor type resides.</param>
        /// <param name="typeName">The full name of the type of the editor.</param>
        /// <param name="connectionString">The connection string for the custom configuration screen.</param>
        public CustomConfigurationEditorAttribute(string assemblyName, string typeName, string connectionString = null)
        {
            m_assemblyName = assemblyName;
            m_typeName = typeName;
            m_connectionString = connectionString;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CustomConfigurationEditorAttribute"/> class.
        /// </summary>
        /// <param name="editorType">The type of the editor.</param>
        /// <param name="connectionString">The connection string for the custom configuration screen.</param>
        public CustomConfigurationEditorAttribute(Type editorType, string connectionString = null)
        {
            m_editorType = editorType;
            m_connectionString = connectionString;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the type of the UI editor used to configure the custom adapter.
        /// </summary>
        public Type EditorType
        {
            get
            {
                Assembly editorAssembly;

                if ((object)m_editorType == null)
                {
                    editorAssembly = Assembly.LoadFrom(FilePath.GetAbsolutePath(m_assemblyName));
                    m_editorType = editorAssembly.GetType(m_typeName);
                }

                return m_editorType;
            }
        }

        /// <summary>
        /// Gets the connection string used to configure the editor.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
        }

        #endregion
    }
}
