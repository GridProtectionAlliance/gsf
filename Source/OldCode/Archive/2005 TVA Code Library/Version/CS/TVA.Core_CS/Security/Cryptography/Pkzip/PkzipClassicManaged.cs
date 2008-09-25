//*******************************************************************************************************
//  PkzipClassicManaged.cs
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

using System;
using System.Security.Cryptography;
using TVA;

namespace TVA.Security.Cryptography.Pkzip
{
    /// <summary>
    /// Defines a wrapper object to access the Pkzip algorithm. 
    /// This class cannot be inherited.
    /// </summary>
    public sealed class PkzipClassicManaged : PkzipClassic
    {
        /// <summary>
        /// Get / set the applicable block size in bits.
        /// </summary>
        /// <remarks>The only valid block size is 8.</remarks>
        public override int BlockSize
        {
            get
            {
                return 8;
            }

            set
            {
                if (value != 8)
                {
                    throw new CryptographicException("Block size is invalid");
                }
            }
        }

        /// <summary>
        /// Get an array of legal <see cref="KeySizes">key sizes.</see>
        /// </summary>
        public override KeySizes[] LegalKeySizes
        {
            get
            {
                KeySizes[] keySizes = new KeySizes[1];
                keySizes[0] = new KeySizes(12 * 8, 12 * 8, 0);
                return keySizes;
            }
        }

        /// <summary>
        /// Generate an initial vector.
        /// </summary>
        public override void GenerateIV()
        {
            // Do nothing.
        }

        /// <summary>
        /// Get an array of legal <see cref="KeySizes">block sizes</see>.
        /// </summary>
        public override KeySizes[] LegalBlockSizes
        {
            get
            {
                KeySizes[] keySizes = new KeySizes[1];
                keySizes[0] = new KeySizes(1 * 8, 1 * 8, 0);
                return keySizes;
            }
        }

        /// <summary>
        /// Get / set the key value applicable.
        /// </summary>
        public override byte[] Key
        {
            get
            {
                if (key_ == null)
                {
                    GenerateKey();
                }

                return (byte[])key_.Clone();
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value.Length != 12)
                {
                    throw new CryptographicException("Key size is illegal");
                }

                key_ = (byte[])value.Clone();
            }
        }

        /// <summary>
        /// Generate a new random key.
        /// </summary>
        public override void GenerateKey()
        {
            key_ = new byte[12];
            
            //Random rnd = new Random();
            //rnd.NextBytes(key_);

            // JRC: Converted to use cryptographically strong sequence of random values.
            Random.GetBytes(key_);
        }

        /// <summary>
        /// Create an encryptor.
        /// </summary>
        /// <param name="rgbKey">The key to use for this encryptor.</param>
        /// <param name="rgbIV">Initialisation vector for the new encryptor.</param>
        /// <returns>Returns a new PkzipClassic encryptor</returns>
        public override ICryptoTransform CreateEncryptor(
            byte[] rgbKey,
            byte[] rgbIV)
        {
            key_ = rgbKey;
            return new PkzipClassicEncryptCryptoTransform(Key);
        }

        /// <summary>
        /// Create a decryptor.
        /// </summary>
        /// <param name="rgbKey">Keys to use for this new decryptor.</param>
        /// <param name="rgbIV">Initialisation vector for the new decryptor.</param>
        /// <returns>Returns a new decryptor.</returns>
        public override ICryptoTransform CreateDecryptor(
            byte[] rgbKey,
            byte[] rgbIV)
        {
            key_ = rgbKey;
            return new PkzipClassicDecryptCryptoTransform(Key);
        }

        #region Instance Fields
        byte[] key_;
        #endregion
    }
}
