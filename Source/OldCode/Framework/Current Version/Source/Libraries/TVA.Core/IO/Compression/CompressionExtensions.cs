//*******************************************************************************************************
//  CompressionExtensions.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/26/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
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
using System.IO;
using System.IO.Compression;

namespace TVA.IO.Compression
{
    #region [ Enumerations ]

    /// <summary>
    /// Level of compression enumeration.
    /// </summary>
    /// <remarks>
    /// Compression strengths represent tradeoffs on speed of compression vs. effectiveness of compression.
    /// </remarks>
    public enum CompressionStrength
    {
        /// <summary>Disables compression.</summary>
        NoCompression = 0,
        /// <summary>Enables standard compression.</summary>
        Standard = 1,
        /// <summary>Enables multi-pass compression to continue recompressing buffer as long as size continues to shrink.</summary>
        MultiPass = 2
    }

    #endregion

    /// <summary>
    /// Defines extension functions related to compression.
    /// </summary>
    public static class CompressionExtensions
    {
        /// <summary>
        /// Default compression buffer size.
        /// </summary>
        public const int BufferSize = 262144;

        /// <summary>
        /// Needed version of this library to uncompress stream (1.0.0 stored as byte 100).
        /// </summary>
        public const byte CompressionVersion = 100;
        
        /// <summary>
        /// Compress a byte array using standard compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to compress.</param>
        /// <returns>A compressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Compress(this byte[] source)
        {
	        return source.Compress(CompressionStrength.Standard);
        }

        /// <summary>
        /// Compress a byte array using specified compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to compress.</param>
        /// <param name="strength">The specified <see cref="CompressionStrength"/>.</param>
        /// <returns>A compressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Compress(this byte[] source, CompressionStrength strength)
        {
            return source.Compress(0, source.Length, strength, 0);
        }

        /// <summary>
        /// Compress a byte array using specified compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to compress.</param>
        /// <param name="length">The number of bytes to read into the byte array for compression.</param>
        /// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        /// <param name="strength">The specified <see cref="CompressionStrength"/>.</param>
        /// <returns>A compressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Compress(this byte[] source, int startIndex, int length, CompressionStrength strength)
        {
	        return source.Compress(startIndex, length, strength, 0);
        }

        // When user requests multi-pass compression, we allow multiple compression passes on a buffer because
        // this can often produce better compression results
        private static byte[] Compress(this byte[] source, int startIndex, int length, CompressionStrength strength, int compressionDepth)
        {
            if (strength == CompressionStrength.NoCompression)
            {
                // No compression requested, return specified portion of the buffer
                byte[] outBuffer = new byte[++length];
                outBuffer[0] = (byte)strength;

                for (int x = 1; x < length; x++)
                    outBuffer[x] = source[startIndex + x - 1];

                return outBuffer;
            }
            else
            {
                // Create a new compression deflater
                MemoryStream compressedData = new MemoryStream();
                DeflateStream deflater = new DeflateStream(compressedData, CompressionMode.Compress);

                // Provide data for compression
                deflater.Write(source, startIndex, length);
                deflater.Close();

                byte[] destination = compressedData.ToArray();
                int destinationLength = destination.Length;

                // Preprend compression depth and extract only used part of compressed buffer
                byte[] outBuffer = new byte[++destinationLength];
                
                // First two bits are reserved for compression strength - this leaves 6 bits for a maximum of 64 compressions
                outBuffer[0] = (byte)((compressionDepth << 2) | (int)strength);

                for (int x = 1; x < destinationLength; x++)
                    outBuffer[x] = destination[x - 1];

                if (strength == CompressionStrength.MultiPass && destinationLength < length && compressionDepth < 64)
                {
                    // See if another pass would help the compression...
                    byte[] testBuffer = outBuffer.Compress(0, outBuffer.Length, strength, compressionDepth + 1);

                    if (testBuffer.Length < outBuffer.Length)
                        return testBuffer;
                    else
                        return outBuffer;
                }
                else
                    return outBuffer;
            }
        }

        /// <summary>
        /// Compress a stream using specified compression strength.
        /// </summary>
        /// <remarks>
        /// This returns a memory stream of the compressed results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        /// <param name="source">The <see cref="Stream"/> to compress.</param>
        /// <param name="strength">The <see cref="CompressionStrength"/> of the compression.</param>
        /// <returns>Returns a <see cref="MemoryStream"/> of the compressed <see cref="Stream"/>.</returns>
        public static MemoryStream Compress(this Stream source, CompressionStrength strength)
        {
	        MemoryStream destination = new MemoryStream();
	        source.Compress(destination, strength, null);
	        return destination;
        }

        /// <summary>
        /// Compress a stream onto given output stream using specified compression strength.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> to compress.</param>
        /// <param name="strength">The <see cref="CompressionStrength"/> of the compression.</param>
        /// <param name="destination">The <see cref="Stream"/> destination.</param>
        /// <param name="progressHandler">The progress handler to check progress.</param>
        public static void Compress(this Stream source, Stream destination, CompressionStrength strength, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer = new byte[BufferSize];
	        byte[] outBuffer;
	        byte[] lengthBuffer;
	        int read;
	        long total = 0, length = -1;

	        // Send initial progress event
	        if (progressHandler != null)
	        {
		        try
		        {
			        if (source.CanSeek)
				        length = source.Length;
		        }
		        catch
		        {
			        length = -1;
		        }

                // Create a new progress handler to track compression progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Compress", length);
                progress.Complete = 0;
            }

	        // Read initial buffer
	        read = source.Read(inBuffer, 0, BufferSize);

	        // Write compression version into stream
	        byte[] version = new byte[1];
	        version[0] = CompressionVersion;
	        destination.Write(version, 0, 1);

	        while (read > 0)
	        {
		        // Compress buffer - note that we are only going to compress used part of buffer,
		        // we don't want any left over garbage to end up in compressed stream...
			    outBuffer = inBuffer.Compress(0, read, strength);

		        // The output stream is hopefully smaller than the input stream, so we prepend the final size of
		        // each compressed buffer into the destination output stream so that we can safely uncompress
		        // the stream in a "chunked" fashion later...
		        lengthBuffer = BitConverter.GetBytes(outBuffer.Length);
		        destination.Write(lengthBuffer, 0, lengthBuffer.Length);
		        destination.Write(outBuffer, 0, outBuffer.Length);

		        // Update compression progress
		        if (progressHandler != null)
		        {
			        total += read;
			        progress.Complete = total;
		        }

		        // Read next buffer
		        read = source.Read(inBuffer, 0, BufferSize);
	        }
        }

        /// <summary>
        /// Decompress a byte array.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to decompress.</param>
        /// <returns>A decompressed version of the source <see cref="Byte"/> array.</returns>
        public static byte[] Decompress(this byte[] source)
        {
            return source.Decompress(0, source.Length);
        }

        /// <summary>
        /// Decompress a byte array.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array to decompress.</param>
        /// <param name="length">The number of bytes to read into the byte array for compression.</param>
        /// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        /// <returns>A decompressed <see cref="Byte"/> array.</returns>
        public static byte[] Decompress(this byte[] source, int startIndex, int length)
        {
            const byte CompressionStrengthMask = (byte)(Bits.Bit00 | Bits.Bit01);

            // Unmask compression strength from first buffer byte
            CompressionStrength strength = (CompressionStrength)(source[startIndex] & CompressionStrengthMask);

            if (strength == CompressionStrength.NoCompression)
            {
                // No compression was applied to original buffer, return specified portion of the buffer
                return source.BlockCopy(startIndex + 1, length - 1);
            }
            else
            {
                // Create a new decompression deflater
                MemoryStream compressedData = new MemoryStream(source, startIndex + 1, length - 1);
                DeflateStream inflater = new DeflateStream(compressedData, CompressionMode.Decompress);
                int compressionDepth = (source[startIndex] & ~CompressionStrengthMask) >> 2;

                // Read uncompressed data
                byte[] destination = inflater.ReadStream();

                // When user requests muli-pass compression, there may be multiple compression passes on a buffer,
                // so we cycle through the needed uncompressions to get back to the original data
                if (strength == CompressionStrength.MultiPass && compressionDepth > 0)
                    return destination.Decompress();
                else
                    return destination;
            }
        }

        /// <summary>
        /// Decompress a stream.
        /// </summary>
        /// <remarks>
        /// This returns a memory stream of the uncompressed results, if the incoming stream is
        /// very large this will consume a large amount memory.  In this case use the overload
        /// that takes the destination stream as a parameter instead.
        /// </remarks>
        /// <param name="source">A <see cref="Stream"/> source to decompress.</param>
        /// <returns>A <see cref="MemoryStream"/> representing the decompressed source.</returns>
        public static MemoryStream Decompress(this Stream source)
        {
	        MemoryStream destination = new MemoryStream();
	        source.Decompress(destination, null);
	        return destination;
        }

        /// <summary>
        /// Decompress a stream onto given output stream.
        /// </summary>
        /// <param name="source">A source <see cref="Stream"/> to decompress.</param>
        /// <param name="destination">The destination <see cref="Stream"/> to decompress to.</param>
        /// <param name="progressHandler">A <see cref="Action"/> handler to monitor the action's progress.</param>
        public static void Decompress(this Stream source, Stream destination, Action<ProcessProgress<long>> progressHandler)
        {
            ProcessProgressHandler<long> progress = null;
            byte[] inBuffer;
	        byte[] outBuffer;
	        byte[] lengthBuffer = BitConverter.GetBytes((int)0);
	        int read, size;
	        long total = 0, length = -1;

	        // Send initial progress event
	        if (progressHandler != null)
	        {
		        try
		        {
			        if (source.CanSeek)
				        length = source.Length;
		        }
		        catch
		        {
			        length = -1;
		        }

                // Create a new progress handler to track compression progress
                progress = new ProcessProgressHandler<long>(progressHandler, "Uncompress", length);
                progress.Complete = 0;
            }

	        // Read compression version from stream
	        byte[] versionBuffer = new byte[1];

	        if (source.Read(versionBuffer, 0, 1) > 0)
	        {
		        if (versionBuffer[0] != CompressionVersion)
			        throw new InvalidOperationException("Invalid compression version encountered in compressed stream - decompression aborted.");

		        // Read initial buffer
		        read = source.Read(lengthBuffer, 0, lengthBuffer.Length);

		        while (read > 0)
		        {
			        // Convert the byte array containing the buffer size into an integer
			        size = BitConverter.ToInt32(lengthBuffer, 0);

			        if (size > 0)
			        {
				        // Create and read the next buffer
				        inBuffer = new byte[size];
				        read = source.Read(inBuffer, 0, size);

				        if (read > 0)
				        {
					        // Uncompress buffer
					        outBuffer = inBuffer.Decompress();
					        destination.Write(outBuffer, 0, outBuffer.Length);
				        }

				        // Update decompression progress
				        if (progressHandler != null)
				        {
					        total += (read + lengthBuffer.Length);
					        progress.Complete = total;
				        }
			        }

			        // Read the size of the next buffer from the stream
			        read = source.Read(lengthBuffer, 0, lengthBuffer.Length);
		        }
	        }
        }
    }
}