//*******************************************************************************************************
//  PkzipClassicDecryptCryptoTransform.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/25/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

// PkzipClassic encryption
//
// Copyright 2004 John Reilly
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System.Security.Cryptography;

namespace PCS.Security.Cryptography.Pkzip
{
    /// <summary>
    /// PkzipClassic CryptoTransform for decryption.
    /// </summary>
    class PkzipClassicDecryptCryptoTransform : PkzipClassicCryptoBase, ICryptoTransform
    {
        /// <summary>
        /// Initialize a new instance of <see cref="PkzipClassicDecryptCryptoTransform"></see>.
        /// </summary>
        /// <param name="keyBlock">The key block to decrypt with.</param>
        internal PkzipClassicDecryptCryptoTransform(byte[] keyBlock)
        {
            SetKeys(keyBlock);
        }

        #region ICryptoTransform Members

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
        /// <returns>The computed transform.</returns>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            byte[] result = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, result, 0);
            return result;
        }

        /// <summary>
        /// Transforms the specified region of the input byte array and copies 
        /// the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write the transform.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>The number of bytes written.</returns>
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            for (int i = inputOffset; i < inputOffset + inputCount; ++i)
            {
                byte newByte = (byte)(inputBuffer[i] ^ TransformByte());
                outputBuffer[outputOffset++] = newByte;
                UpdateKeys(newByte);
            }
            return inputCount;
        }

        /// <summary>
        /// Gets a value indicating whether the current transform can be reused.
        /// </summary>
        public bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the size of the input data blocks in bytes.
        /// </summary>
        public int InputBlockSize
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets the size of the output data blocks in bytes.
        /// </summary>
        public int OutputBlockSize
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether multiple blocks can be transformed.
        /// </summary>
        public bool CanTransformMultipleBlocks
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Cleanup internal state.
        /// </summary>
        public void Dispose()
        {
            Reset();
        }

        #endregion
    }
}
