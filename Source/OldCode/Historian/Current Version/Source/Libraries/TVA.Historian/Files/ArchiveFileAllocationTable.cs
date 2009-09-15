//*******************************************************************************************************
//  ArchiveFileAllocationTable.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, TVA.
//  01/23/2008 - Pinal C. Patel
//       Added thread safety to all FindDataBlock() methods.
//       Recoded RequestDataBlock() method to include the logic to use previously used partially filled 
//       data blocks first.
//  03/31/2008 - Pinal C. Patel
//       Removed intervaled persisting of FAT since FAT is persisted when new block is requested.
//       Recoded RequestDataBlock() method to speed up the block request process based on the block index 
//       suggestion provided from the state information of the point.
//  07/14/2008 - Pinal C. Patel
//       Added overload to GetDataBlock() method that takes a block index.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//  9/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TVA.Interop;
using TVA.Parsing;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Represents the File Allocation Table of an <see cref="ArchiveFile"/>.
    /// </summary>
    /// <seealso cref="ArchiveFile"/>.
    /// <seealso cref="ArchiveDataBlock"/>
    /// <seealso cref="ArchiveDataBlockPointer"/>
    public class ArchiveFileAllocationTable : ISupportBinaryImage
    {
        #region [ Members ]

        // Constants
        private const int ArrayDescriptorLength = 10;

        // Fields
        private TimeTag m_fileStartTime;
        private TimeTag m_fileEndTime;
        private int m_dataPointsReceived;
        private int m_dataPointsArchived;
        private int m_dataBlockSize;
        private int m_dataBlockCount;
        private List<ArchiveDataBlockPointer> m_dataBlockPointers;
        private ArchiveFile m_parent;
        private int m_searchHistorianID;     // <=|
        private TimeTag m_searchStartTime;  // <=| Used for finding data block pointer in m_dataBlockPointers
        private TimeTag m_searchEndTime;    // <=|

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFileAllocationTable"/> class.
        /// </summary>
        /// <param name="parent">An <see cref="ArchiveFile"/> object.</param>
        internal ArchiveFileAllocationTable(ArchiveFile parent)
        {
            m_parent = parent;
            m_dataBlockPointers = new List<ArchiveDataBlockPointer>();

            if (m_parent.FileData.Length == 0)
            {
                // File is brand new.
                m_fileStartTime = TimeTag.MinValue;
                m_fileEndTime = TimeTag.MinValue;
                m_dataBlockSize = m_parent.DataBlockSize;
                m_dataBlockCount = ArchiveFile.MaximumDataBlocks(m_parent.FileSize, m_parent.DataBlockSize);

                for (int i = 0; i < m_dataBlockCount; i++)
                {
                    m_dataBlockPointers.Add(new ArchiveDataBlockPointer(m_parent, i));
                }
            }
            else
            {
                // File was created previously.
                byte[] fixedFatData = new byte[FixedBinaryLength];
                m_parent.FileData.Seek(-fixedFatData.Length, SeekOrigin.End);
                m_parent.FileData.Read(fixedFatData, 0, fixedFatData.Length);
                FileStartTime = new TimeTag(EndianOrder.LittleEndian.ToDouble(fixedFatData, 0));
                FileEndTime = new TimeTag(EndianOrder.LittleEndian.ToDouble(fixedFatData, 8));
                DataPointsReceived = EndianOrder.LittleEndian.ToInt32(fixedFatData, 16);
                DataPointsArchived = EndianOrder.LittleEndian.ToInt32(fixedFatData, 20);
                DataBlockSize = EndianOrder.LittleEndian.ToInt32(fixedFatData, 24);
                DataBlockCount = EndianOrder.LittleEndian.ToInt32(fixedFatData, 28);

                byte[] variableFatData = new byte[m_dataBlockCount * ArchiveDataBlockPointer.ByteCount];
                m_parent.FileData.Seek(-(variableFatData.Length + FixedBinaryLength), SeekOrigin.End);
                m_parent.FileData.Read(variableFatData, 0, variableFatData.Length);
                for (int i = 0; i < m_dataBlockCount; i++)
                {
                    m_dataBlockPointers.Add(new ArchiveDataBlockPointer(m_parent, i, variableFatData, i * ArchiveDataBlockPointer.ByteCount, variableFatData.Length));
                }
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of the oldest <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 01/01/1995 and 01/19/2063.</exception>
        public TimeTag FileStartTime
        {
            get
            {
                return m_fileStartTime;
            }
            set
            {
                if (value < TimeTag.MinValue || value > TimeTag.MaxValue)
                    throw new ArgumentException("Value must between 01/01/1995 and 01/19/2063.");

                m_fileStartTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of the newest <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 01/01/1995 and 01/19/2063.</exception>
        public TimeTag FileEndTime
        {
            get
            {
                return m_fileEndTime;
            }
            set
            {
                if (value < TimeTag.MinValue || value > TimeTag.MaxValue)
                    throw new ArgumentException("Value must between 01/01/1995 and 01/19/2063.");

                m_fileEndTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the number <see cref="ArchiveData"/> points received by the <see cref="ArchiveFile"/> for archival.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive or zero.</exception>
        public int DataPointsReceived
        {
            get
            {
                return m_dataPointsReceived;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive or zero.");

                m_dataPointsReceived = value;
            }
        }

        /// <summary>
        /// Gets or sets the number <see cref="ArchiveData"/> points archived by the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive or zero.</exception>
        public int DataPointsArchived
        {
            get
            {
                return m_dataPointsArchived;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive or zero.");

                m_dataPointsArchived = value;
            }
        }

        /// <summary>
        /// Gets the size (in KB) of a single <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlockSize
        {
            get
            {
                return m_dataBlockSize;
            }
            private set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_dataBlockSize = value;
            }
        }

        /// <summary>
        /// Gets the total number of <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlockCount
        {
            get
            {
                return m_dataBlockCount;
            }
            private set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_dataBlockCount = value;
            }
        }

        /// <summary>
        /// Gets the number of used <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlocksUsed
        {
            get
            {
                return m_dataBlockCount - DataBlocksAvailable;
            }
        }

        /// <summary>
        /// Gets the number of unused <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlocksAvailable
        {
            get
            {
                ArchiveDataBlock unusedDataBlock = FindDataBlock(-1);
                if (unusedDataBlock != null)
                    return m_dataBlockCount - unusedDataBlock.Index;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the <see cref="ArchiveDataBlockPointer"/>s to the <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <remarks>
        /// WARNING: <see cref="DataBlockPointers"/> is thread unsafe. Synchronized access is required.
        /// </remarks>
        public IList<ArchiveDataBlockPointer> DataBlockPointers
        {
            get
            {
                return m_dataBlockPointers.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return VariableBinaryLength + FixedBinaryLength;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="ArchiveFileAllocationTable"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[BinaryLength];

                Array.Copy(VariableBinaryImage, 0, image, 0, VariableBinaryLength);
                Array.Copy(FixedBinaryImage, 0, image, VariableBinaryLength, FixedBinaryLength);

                return image;
            }
        }

        private long DataBinaryLength
        {
            get
            {
                return (m_dataBlockCount * (m_dataBlockSize * 1024));
            }
        }

        private int FixedBinaryLength
        {
            get
            {
                return 32;
            }
        }

        private int VariableBinaryLength
        {
            get
            {
                // We add the extra bytes for the array descriptor that required for reading the file from VB.
                return (ArrayDescriptorLength + (m_dataBlockCount * ArchiveDataBlockPointer.ByteCount));
            }
        }

        private byte[] FixedBinaryImage
        {
            get
            {
                byte[] fixedImage = new byte[FixedBinaryLength];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_fileStartTime.Value), 0, fixedImage, 0, 8);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_fileEndTime.Value), 0, fixedImage, 8, 8);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataPointsReceived), 0, fixedImage, 16, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataPointsArchived), 0, fixedImage, 20, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataBlockSize), 0, fixedImage, 24, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataBlockCount), 0, fixedImage, 28, 4);

                return fixedImage;
            }
        }

        private byte[] VariableBinaryImage
        {
            get
            {
                byte[] variableImage = new byte[VariableBinaryLength];
                VBArrayDescriptor arrayDescriptor = VBArrayDescriptor.OneBasedOneDimensionalArray(m_dataBlockCount);

                Array.Copy(arrayDescriptor.BinaryImage, 0, variableImage, 0, arrayDescriptor.BinaryLength);
                lock (m_dataBlockPointers)
                {
                    for (int i = 0; i < m_dataBlockPointers.Count; i++)
                    {
                        Array.Copy(m_dataBlockPointers[i].BinaryImage, 0, variableImage, (i * ArchiveDataBlockPointer.ByteCount) + arrayDescriptor.BinaryLength, ArchiveDataBlockPointer.ByteCount);
                    }
                }

                return variableImage;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="ArchiveFileAllocationTable"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="ArchiveFileAllocationTable"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="ArchiveFileAllocationTable"/>.</returns>
        /// <exception cref="NotSupportedException">Always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Saves the <see cref="ArchiveFileAllocationTable"/> data to the <see cref="ArchiveFile"/>.
        /// </summary>
        public void Save()
        {
            // Leave space for data blocks.
            lock (m_parent.FileData)
            {
                m_parent.FileData.Seek(DataBinaryLength, SeekOrigin.Begin);
                m_parent.FileData.Write(BinaryImage, 0, BinaryLength);
                if (!m_parent.CacheWrites)
                    m_parent.FileData.Flush();
            }
        }

        /// <summary>
        /// Extends the <see cref="ArchiveFile"/> by one <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public void Extend()
        {
            Extend(1);
        }

        /// <summary>
        /// Extends the <see cref="ArchiveFile"/> by the specified number of <see cref="ArchiveDataBlock"/>s.
        /// </summary>
        /// <param name="dataBlocksToAdd">Number of <see cref="ArchiveDataBlock"/>s to add to the <see cref="ArchiveFile"/>.</param>
        public void Extend(int dataBlocksToAdd)
        {
            // Extend the FAT.
            lock (m_dataBlockPointers)
            {
                for (int i = 1; i <= dataBlocksToAdd; i++)
                {
                    m_dataBlockPointers.Add(new ArchiveDataBlockPointer(m_parent, m_dataBlockPointers.Count));

                }
                m_dataBlockCount = m_dataBlockPointers.Count;
            }
            Save();

            // Initialize newly added data blocks.
            ArchiveDataBlock dataBlock;
            for (int i = m_dataBlockCount - dataBlocksToAdd; i < m_dataBlockCount; i++)
            {
                dataBlock = new ArchiveDataBlock(m_parent, i, -1, true);
            }
        }

        /// <summary>
        /// Returns the first <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier whose <see cref="ArchiveDataBlock"/> is to be retrieved.</param>
        /// <returns><see cref="ArchiveDataBlock"/> object if a match is found; otherwise null.</returns>
        public ArchiveDataBlock FindDataBlock(int historianID)
        {
            ArchiveDataBlockPointer pointer = null;
            lock (m_dataBlockPointers)
            {
                // Setup the search criteria to find the first data block pointer for the specified id.
                m_searchHistorianID = historianID;
                m_searchStartTime = TimeTag.MinValue;
                m_searchEndTime = TimeTag.MaxValue;

                pointer = m_dataBlockPointers.Find(FindDataBlockPointer);
            }

            if (pointer == null)
                return null;
            else
                return pointer.DataBlock;
        }

        /// <summary>
        /// Returns the last <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns><see cref="ArchiveDataBlock"/> object if a match is found; otherwise null.</returns>
        public ArchiveDataBlock FindLastDataBlock(int historianID)
        {
            ArchiveDataBlockPointer pointer = null;
            lock (m_dataBlockPointers)
            {
                // Setup the search criteria to find the last data block pointer for the specified id.
                m_searchHistorianID = historianID;
                m_searchStartTime = TimeTag.MinValue;
                m_searchEndTime = TimeTag.MaxValue;

                pointer = m_dataBlockPointers.FindLast(FindDataBlockPointer);
            }

            if (pointer == null)
                return null;
            else
                return pointer.DataBlock;
        }

        /// <summary>
        /// Returns all <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/> for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <returns>A collection of <see cref="ArchiveDataBlock"/>s.</returns>
        public IList<ArchiveDataBlock> FindDataBlocks(int historianID)
        {
            return FindDataBlocks(historianID, TimeTag.MinValue);
        }

        /// <summary>
        /// Returns all <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/> for the specified <paramref name="historianID"/> with <see cref="ArchiveData"/> points later than the specified <paramref name="startTime"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/>.</param>
        /// <returns>A collection of <see cref="ArchiveDataBlock"/>s.</returns>
        public IList<ArchiveDataBlock> FindDataBlocks(int historianID, TimeTag startTime)
        {
            return FindDataBlocks(historianID, startTime, TimeTag.MaxValue);
        }

        /// <summary>
        /// Returns all <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/> for the specified <paramref name="historianID"/> with <see cref="ArchiveData"/> points between the specified <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/>.</param>
        /// <param name="endTime">End <see cref="TimeTag"/>.</param>
        /// <returns>A collection of <see cref="ArchiveDataBlock"/>s.</returns>
        public IList<ArchiveDataBlock> FindDataBlocks(int historianID, TimeTag startTime, TimeTag endTime)
        {
            List<ArchiveDataBlockPointer> blockPointers = null;
            lock (m_dataBlockPointers)
            {
                // Setup the search criteria to find all data block pointers for the specified point id
                // that fall between the specified start and end time.
                m_searchHistorianID = historianID;
                m_searchStartTime = (startTime != null ? startTime : TimeTag.MinValue);
                m_searchEndTime = (endTime != null ? endTime : TimeTag.MaxValue);

                blockPointers = m_dataBlockPointers.FindAll(FindDataBlockPointer);
            }

            // Build a list of data blocks that correspond to the found data block pointers.
            List<ArchiveDataBlock> blocks = new List<ArchiveDataBlock>();
            for (int i = 0; i < blockPointers.Count; i++)
            {
                blocks.Add(blockPointers[i].DataBlock);
            }

            return blocks;
        }

        /// <summary>
        /// Returns an <see cref="ArchiveDataBlock"/> for writting <see cref="ArchiveData"/> points for the specified <paramref name="historianID"/>.
        /// </summary>
        /// <param name="historianID">Historian identifier for which the <see cref="ArchiveDataBlock"/> is being requested.</param>
        /// <param name="dataTime"><see cref="TimeTag"/> of the <see cref="ArchiveData"/> point to be written to the <see cref="ArchiveDataBlock"/>.</param>
        /// <param name="blockIndex"><see cref="ArchiveDataBlock.Index"/> of the <see cref="ArchiveDataBlock"/> last used for writting <see cref="ArchiveData"/> points for the <paramref name="historianID"/>.</param>
        /// <returns><see cref="ArchiveDataBlock"/> object if available; otherwise null if all <see cref="ArchiveDataBlock"/>s have been allocated.</returns>
        internal ArchiveDataBlock RequestDataBlock(int historianID, TimeTag dataTime, int blockIndex)
        {
            ArchiveDataBlock dataBlock = null;
            ArchiveDataBlockPointer dataBlockPointer = null;
            if (blockIndex >= 0 && blockIndex < m_dataBlockCount)
            {
                // Valid data block index is specified, so retrieve the corresponding data block.
                lock (m_dataBlockPointers)
                {
                    dataBlockPointer = m_dataBlockPointers[blockIndex];
                }

                dataBlock = dataBlockPointer.DataBlock;
                if (!dataBlockPointer.IsAllocated && dataBlock.SlotsUsed > 0)
                {
                    // Clear existing data from the data block since it is unallocated.
                    dataBlock.Reset();
                }
                else if (dataBlockPointer.IsAllocated &&
                         (dataBlockPointer.HistorianID != historianID ||
                          (dataBlockPointer.HistorianID == historianID && dataBlock.SlotsAvailable == 0)))
                {
                    // Search for a new data block since the suggested data block cannot be used.
                    blockIndex = -1;
                }
            }

            if (blockIndex < 0)
            {
                // Negative data block index is specified indicating a search must be performed for a data block.
                dataBlock = FindLastDataBlock(historianID);
                if (dataBlock != null && dataBlock.SlotsAvailable == 0)
                {
                    // Previously used data block is full.
                    dataBlock = null;
                }

                if (dataBlock == null)
                {
                    // Look for the first unallocated data block.
                    dataBlock = FindDataBlock(-1);
                    if (dataBlock == null)
                    {
                        // Extend the file for historic writes only.
                        if (m_parent.FileType == ArchiveFileType.Historic)
                        {
                            Extend();
                            dataBlock = m_dataBlockPointers[m_dataBlockPointers.Count - 1].DataBlock;
                        }
                    }
                    else
                    {
                        // Reset the unallocated data block if there is data in it.
                        if (dataBlock.SlotsUsed > 0)
                        {
                            dataBlock.Reset();
                        }
                    }
                }

                // Get the pointer to the data block so that its information can be updated if necessary.
                if (dataBlock == null)
                {
                    dataBlockPointer = null;
                }
                else
                {
                    lock (m_dataBlockPointers)
                    {
                        dataBlockPointer = m_dataBlockPointers[dataBlock.Index];
                    }
                }
            }

            if (dataBlockPointer != null && !dataBlockPointer.IsAllocated)
            {
                // Mark the data block as allocated.
                dataBlockPointer.HistorianID = historianID;
                dataBlockPointer.StartTime = dataTime;

                // Set the file start time if not set.
                if (m_fileStartTime == TimeTag.MinValue)
                    m_fileStartTime = dataTime;

                // Persist data block information to disk.
                lock (m_parent.FileData)
                {
                    // We'll write information about the just allocated data block to the file.
                    m_parent.FileData.Seek(DataBinaryLength + ArrayDescriptorLength + (dataBlockPointer.Index * ArchiveDataBlockPointer.ByteCount), SeekOrigin.Begin);
                    m_parent.FileData.Write(dataBlockPointer.BinaryImage, 0, ArchiveDataBlockPointer.ByteCount);
                    // We'll also write the fixed part of the FAT data that resides at the end.
                    m_parent.FileData.Seek(-FixedBinaryLength, SeekOrigin.End);
                    m_parent.FileData.Write(FixedBinaryImage, 0, FixedBinaryLength);
                    if (!m_parent.CacheWrites)
                        m_parent.FileData.Flush();
                }

                // Re-fetch the data block with updated information after allocation.
                dataBlock = dataBlockPointer.DataBlock;
            }

            return dataBlock;
        }

        /// <summary>
        /// Finds <see cref="ArchiveDataBlockPointer"/> that match the search criteria that is determined by member variables.
        /// </summary>
        private bool FindDataBlockPointer(ArchiveDataBlockPointer dataBlockPointer)
        {
            if (dataBlockPointer != null)
                // Note: The StartTime value of the pointer is ignored if m_searchStartTime = TimeTag.MinValue and
                //       m_searchEndTime = TimeTag.MaxValue. In this case only the PointID value is compared. This
                //       comes in handy when the first or last pointer is to be found from the list of pointers for
                //       a point ID in addition to all the pointer for a point ID.
                return ((dataBlockPointer.HistorianID == m_searchHistorianID) &&
                        (m_searchStartTime == TimeTag.MinValue || dataBlockPointer.StartTime >= m_searchStartTime) &&
                        (m_searchEndTime == TimeTag.MaxValue || dataBlockPointer.StartTime <= m_searchEndTime));
            else
                return false;
        }

        #endregion
    }
}