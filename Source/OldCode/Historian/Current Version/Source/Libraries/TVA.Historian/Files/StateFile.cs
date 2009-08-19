//*******************************************************************************************************
//  StateFile.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/08/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System.ComponentModel;
using System.Drawing;
using TVA.IO;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Represents a file containing <see cref="StateRecord"/>s.
    /// </summary>
    /// <seealso cref="StateRecord"/>
    [ToolboxBitmap(typeof(StateFile))]
    public class StateFile : IsamDataFileBase<StateRecord>
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="StateFile"/> class.
        /// </summary>
        public StateFile()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="StateFile"/>.</param>
        public StateFile(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the binary size of a <see cref="StateRecord"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer.</returns>
        protected override int GetRecordSize()
        {
            return StateRecord.ByteCount;
        }

        /// <summary>
        /// Creates a new <see cref="StateRecord"/> with the specified <paramref name="recordIndex"/>.
        /// </summary>
        /// <param name="recordIndex">1-based index of the <see cref="StateRecord"/>.</param>
        /// <returns>A <see cref="StateRecord"/> object.</returns>
        protected override StateRecord CreateNewRecord(int recordIndex)
        {
            return new StateRecord(recordIndex);
        }

        #endregion
    }
}
