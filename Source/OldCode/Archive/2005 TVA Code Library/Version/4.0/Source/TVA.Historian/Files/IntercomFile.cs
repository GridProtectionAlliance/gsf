//*******************************************************************************************************
//  IntercomFile.cs
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
//  03/09/2007 - Pinal C. Patel
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
    /// Represents a file containing <see cref="IntercomRecord"/>s.
    /// </summary>
    /// <seealso cref="IntercomRecord"/>
    [ToolboxBitmap(typeof(IntercomFile))]
    public class IntercomFile : IsamDataFileBase<IntercomRecord>
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="IntercomFile"/> class.
        /// </summary>
        public IntercomFile()
            : base()
        {
            MinimumRecordCount = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntercomFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="IntercomFile"/>.</param>
        public IntercomFile(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the binary size of a <see cref="IntercomRecord"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer.</returns>
        protected override int GetRecordSize()
        {
            return IntercomRecord.ByteCount;
        }

        /// <summary>
        /// Creates a new <see cref="IntercomRecord"/> with the specified <paramref name="recordIndex"/>.
        /// </summary>
        /// <param name="recordIndex">1-based index of the <see cref="IntercomRecord"/>.</param>
        /// <returns>A <see cref="IntercomRecord"/> object.</returns>
        protected override IntercomRecord CreateNewRecord(int recordIndex)
        {
            return new IntercomRecord(recordIndex);
        }

        #endregion
    }
}
