//*******************************************************************************************************
//  MetadataFile.cs
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
    /// Represents a file containing <see cref="MetadataRecord"/>s.
    /// </summary>
    /// <seealso cref="MetadataRecord"/>
    [ToolboxBitmap(typeof(MetadataFile))]
    public class MetadataFile : IsamDataFileBase<MetadataRecord>
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataFile"/> class.
        /// </summary>
        public MetadataFile()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataFile"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="MetadataFile"/>.</param>
        public MetadataFile(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }

        #endregion

        #region [ Methods ]

        //public MetadataRecord Read(string name)
        //{
        //    if (IsOpen)
        //    {
        //        List<MetadataRecord> records = Read();
        //        for (int i = 0; i < records.Count; i++)
        //        {
        //            if (string.Compare(name, records[i].Name) == 0 || string.Compare(name, records[i].Synonym1) == 0 || string.Compare(name, records[i].Synonym2) == 0)
        //            {
        //                return records[i];
        //            }
        //        }

        //        return null;
        //    }
        //    else
        //    {
        //        throw (new InvalidOperationException(string.Format("{0} \"{1}\" is not open.", this.GetType().Name, name)));
        //    }
        //}

        /// <summary>
        /// Gets the binary size of a <see cref="MetadataRecord"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer.</returns>
        protected override int GetRecordSize()
        {
            return MetadataRecord.ByteCount;
        }

        /// <summary>
        /// Creates a new <see cref="MetadataRecord"/> with the specified <paramref name="recordIndex"/>.
        /// </summary>
        /// <param name="recordIndex">1-based index of the <see cref="MetadataRecord"/>.</param>
        /// <returns>A <see cref="MetadataRecord"/> object.</returns>
        protected override MetadataRecord CreateNewRecord(int recordIndex)
        {
            return new MetadataRecord(recordIndex);
        }

        #endregion
    }
}
