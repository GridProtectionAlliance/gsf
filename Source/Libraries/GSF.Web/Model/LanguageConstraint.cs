//******************************************************************************************************
//  LanguageConstraint.cs - Gbtc
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
//  05/05/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using RazorEngine;

namespace GSF.Web.Model
{
    /// <summary>
    /// Defines a language constraint for a <see cref="RazorEngine{TLanguage}"/>.
    /// </summary>
    public abstract class LanguageConstraint
    {
        /// <summary>
        /// Gets target language for the <see cref="LanguageConstraint"/> implementation.
        /// </summary>
        public abstract Language TargetLanguage
        {
            get;
        }

        /// <summary>
        /// Gets resolution mode for <see cref="RazorEngine{TLanguage}"/> views.
        /// </summary>
        public virtual RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.ResolvePath;
    }

    /// <summary>
    /// Defines a C# based language constraint.
    /// </summary>
    public class CSharp : LanguageConstraint
    {
        /// <summary>
        /// Gets target language for the <see cref="LanguageConstraint"/> implementation - C#.
        /// </summary>
        public override Language TargetLanguage => Language.CSharp;
    }

    /// <summary>
    /// Defines a C# based language constraint in debug mode.
    /// </summary>
    public class CSharpDebug : CSharp
    {
        /// <summary>
        /// Gets resolution mode for <see cref="RazorEngine{TLanguage}"/> views - watching resolve path for debugging operations.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.WatchingResolvePath;
    }

    /// <summary>
    /// Defines a C# based language constraint for embedded resources.
    /// </summary>
    public class CSharpEmbeddedResource : CSharp
    {
        /// <summary>
        /// Gets resolution mode for <see cref="RazorEngine{TLanguage}"/> views - embedded resources.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.EmbeddedResource;
    }

    /// <summary>
    /// Defines a Visual Basic based language constraint.
    /// </summary>
    public class VisualBasic : LanguageConstraint
    {
        /// <summary>
        /// Gets target language for the <see cref="LanguageConstraint"/> implementation - Visual Basic.
        /// </summary>
        public override Language TargetLanguage => Language.VisualBasic;
    }

    /// <summary>
    /// Defines a Visual Basic based language constraint in debug mode.
    /// </summary>
    public class VisualBasicDebug : VisualBasic
    {
        /// <summary>
        /// Gets resolution mode for <see cref="RazorEngine{TLanguage}"/> views - watching resolve path for debugging operations.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.WatchingResolvePath;
    }

    /// <summary>
    /// Defines a Visual Basic based language constraint for embedded resources.
    /// </summary>
    public class VisualBasicEmbeddedResource : VisualBasic
    {
        /// <summary>
        /// Gets resolution mode for <see cref="RazorEngine{TLanguage}"/> views - embedded resources.
        /// </summary>
        public override RazorViewResolutionMode ResolutionMode => RazorViewResolutionMode.EmbeddedResource;
    }
}