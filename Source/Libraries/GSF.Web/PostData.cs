//******************************************************************************************************
//  PostData.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  07/30/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;

namespace GSF.Web
{
    /// <summary>
    /// Represents parsed HTTP post data from multi-part form data.
    /// </summary>
    public class PostData
    {
        #region [ Members ]

        // Fields
        private readonly NameValueCollection m_formData;
        private readonly List<HttpContent> m_fileData;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PostData"/> instance.
        /// </summary>
        public PostData() : this(new NameValueCollection())
        {
        }

        internal PostData(NameValueCollection formData)
        {
            m_formData = formData;
            m_fileData = new List<HttpContent>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets any form data passed as part of the multi-part form data.
        /// </summary>
        public NameValueCollection FormData => m_formData;

        /// <summary>
        /// Gets any uploaded file data passed as part of the multi-part form data.
        /// </summary>
        public List<HttpContent> FileData => m_fileData;

        #endregion
    }
}
