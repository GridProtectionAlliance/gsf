//*******************************************************************************************************
//  ISupportLifecycle.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  10/09/2008 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>
    /// Specifies that this object provides support for performing tasks during the key stages of object lifecycle. 
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <listheader>
    ///         <term>Lifecycle Stage</term>
    ///         <description>Equivalent Member</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Birth</term>
    ///         <description><see cref="Initialize()"/></description>
    ///     </item>
    ///     <item>
    ///         <term>Life (Work/Sleep)</term>
    ///         <description><see cref="Enabled"/></description>
    ///     </item>
    ///     <item>
    ///         <term>Death</term>
    ///         <description><see cref="IDisposable.Dispose()"/></description>
    ///     </item>
    /// </list>
    /// </remarks>
    public interface ISupportLifecycle : IDisposable
    {
        /// <summary>
        /// Initializes the state of the object.
        /// </summary>
        /// <remarks>
        /// Typical implementation of <see cref="Initialize()"/> should allow the object state to be initialized only 
        /// once. <see cref="Initialize()"/> should be called automatically from one or more key entry points of the 
        /// object. For example, if the object is a <see cref="System.ComponentModel.Component"/> and it implements 
        /// the <see cref="System.ComponentModel.ISupportInitialize"/> interface then <see cref="Initialize()"/> should 
        /// be called from the <see cref="System.ComponentModel.ISupportInitialize.EndInit()"/> method so that the object 
        /// gets initialized automatically when consumed through the IDE designer surface. In addition to this 
        /// <see cref="Initialize()"/> should also be called from key or mandatory methods of the object, like 'Start()'
        /// or 'Connect()', so that the object gets initialized even when not consumed through the IDE designer surface.
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the object is enabled.
        /// </summary>
        /// <remarks>
        /// Typical implementation of <see cref="Enabled"/> should suspend the internal processing when the object is 
        /// disabled and resume processing when the object is enabled.
        /// </remarks>
        bool Enabled { get; set; }
    }
}
