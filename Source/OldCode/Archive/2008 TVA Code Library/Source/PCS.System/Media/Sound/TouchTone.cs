/**************************************************************************\
   Copyright © 2009 - Gbtc, James Ritchie Carroll
   All rights reserved.
  
   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:
  
      * Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.
       
      * Redistributions in binary form must reproduce the above
        copyright notice, this list of conditions and the following
        disclaimer in the documentation and/or other materials provided
        with the distribution.
  
   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDER "AS IS" AND ANY
   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
   IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
   PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
   OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
   OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
  
\**************************************************************************/

using System.Collections.Generic;

namespace System.Media.Sound
{
    #region [ Enumerations ]
    
    /// <summary>
    /// Touch tone key enumeration.
    /// </summary>
    public enum TouchToneKey
    {
        /// <summary>Represents the number "0" on a touch tone key pad.</summary>
        Zero,
        /// <summary>Represents the number "1" on a touch tone key pad.</summary>
        One,
        /// <summary>Represents the number "2" on a touch tone key pad.</summary>
        Two,
        /// <summary>Represents the number "3" on a touch tone key pad.</summary>
        Three,
        /// <summary>Represents the number "4" on a touch tone key pad.</summary>
        Four,
        /// <summary>Represents the number "5" on a touch tone key pad.</summary>
        Five,
        /// <summary>Represents the number "6" on a touch tone key pad.</summary>
        Six,
        /// <summary>Represents the number "7" on a touch tone key pad.</summary>
        Seven,
        /// <summary>Represents the number "8" on a touch tone key pad.</summary>
        Eight,
        /// <summary>Represents the number "9" on a touch tone key pad.</summary>
        Nine,
        /// <summary>Represents the "*" key on a touch tone key pad.</summary>
        Asterisk,
        /// <summary>Represents the "#" key on a touch tone key pad.</summary>
        Pound
    }

    #endregion

    /// <summary>
    /// Touch tone generator.
    /// </summary>
    /// <example>
    /// This example will "dial a phone number" over the computer's speakers:
    /// <code>
    /// using System;
    /// using System.Media;
    /// using System.Media.Sound;
    /// 
    /// static class Program
    /// {
    ///     static void Main()
    ///     {
    ///         WaveFile waveFile = new WaveFile(SampleRate.Hz8000, BitsPerSample.Bits16, DataChannels.Mono);
    ///         double volume = 0.25D;  // Set volume of TouchTone to 25% of maximum
    ///         string phoneNumber = "9, 1 (535) 217-1631";
    /// 
    ///         // Generate touch tones for specified phone number
    ///         DTMF.Generate(waveFile, TouchTone.GetTouchTones(phoneNumber), volume);
    /// 
    ///         Console.WriteLine("Dialing {0}...", phoneNumber);
    ///         waveFile.Play();
    /// 
    ///         Console.ReadKey();
    ///     }
    /// } 
    /// </code>
    /// </example>
    public class TouchTone : DTMF
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Valid touch tone keys.
        /// </summary>
        public const string ValidTouchTones = "0123456789*#";
        
        /// <summary>
        /// Default duration, in seconds, of touch tones.
        /// </summary>
        public const double DefaultKeyDuration = 0.15D;
        
        /// <summary>
        /// Default duration, in seconds, of pause between touch tones.
        /// </summary>
        public const double DefaultInterKeyPause = 0.05D;

        // Fields
        private TouchToneKey m_key;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="TouchTone"/> for specified touch tone key.
        /// </summary>
        /// <param name="key">Touch tone to create.</param>
        public TouchTone(TouchToneKey key)
        {
            Key = key;
            Duration = DefaultKeyDuration;
        }

        /// <summary>
        /// Constructs a new <see cref="TouchTone"/> for specified touch tone key character.
        /// </summary>
        /// <param name="key">Character of touch tone to create.</param>
        /// <exception cref="ArgumentException"><paramref name="key"/> is not a valid touch tone character.</exception>
        public TouchTone(Char key)
        {
            TouchToneKey parsedKey;

            if (TryParseKey(key, out parsedKey))
            {
                Key = parsedKey;
                Duration = DefaultKeyDuration;
            }
            else
                throw new ArgumentException(string.Format("\'{0}\' is not a valid touch tone", key), "key");
        }

        /// <summary>
        /// Constructs a new <see cref="TouchTone"/> for specified touch tone key number.
        /// </summary>
        /// <param name="key">Number of touch tone to create (note that * = 10 and # = 11).</param>
        /// <exception cref="ArgumentException"><paramref name="key"/> is not a valid touch tone number.</exception>
        public TouchTone(int key)
        {
            if (key < 0 || key > 11)
                throw new ArgumentException(string.Format("\'{0}\' is not a valid touch tone number", key), "key");

            Key = (TouchToneKey)key;
            Duration = DefaultKeyDuration;
        }

        #endregion

        #region [ Properties ]

        /// <summary>Gets or sets touch tone key for this touch tone.</summary>
        TouchToneKey Key
        {
            get
            {
                return m_key;
            }
            set
            {
                m_key = value;

                // Define touch tone frequencies for given key
                LowFrequency = m_lowTouchTones[(int)m_key / 3];
                HighFrequency = m_highTouchTones[(int)m_key % 3];
            }
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static double[] m_lowTouchTones;
        private static double[] m_highTouchTones;

        // Static Constructor
        static TouchTone()
        {
            m_lowTouchTones = new double[] { 697, 770, 852, 941 };
            m_highTouchTones = new double[] { 1209, 1336, 1477 };
        }

        // Static Methods

        /// <summary>
        /// Get array of touch tones for given string.
        /// </summary>
        /// <param name="keys">String of touch tone characters to convert to touch tones.</param>
        /// <returns>Array of touch tones for given string.</returns>
        /// <remarks>Non-touch tone characters are ignored. Commas are interpreted as a one second pause.</remarks>
        public static DTMF[] GetTouchTones(string keys)
        {
            return GetTouchTones(keys, DefaultKeyDuration, DefaultInterKeyPause);
        }
        
        /// <summary>
        /// Get array of touch tones for given string.
        /// </summary>
        /// <param name="keys">String of touch tone characters to convert to touch tones.</param>
        /// <param name="keyDuration">Duration of touch tone key press in seconds, typically fractional.</param>
        /// <param name="interKeyPause">Time to wait between key presses in seconds, typically fractional.</param>
        /// <returns>Array of touch tones for given string.</returns>
        /// <remarks>Non-touch tone characters are ignored. Commas are interpreted as a one second pause.</remarks>
        public static DTMF[] GetTouchTones(string keys, double keyDuration, double interKeyPause)
        {
            List<DTMF> touchTones = new List<DTMF>();
            TouchTone touchTone;
            DTMF pause = new DTMF(0.0D, 0.0D, interKeyPause);
            DTMF longPause = new DTMF(0.0D, 0.0D, 1.0D);

            foreach (char key in keys)
            {
                if (key == ',')
                {
                    // Interpret commas as long pauses
                    touchTones.Add(longPause);
                }
                else if (TouchTone.TryParse(key, out touchTone))
                {
                    if (touchTones.Count > 0)
                        touchTones.Add(pause);

                    touchTone.Duration = keyDuration;
                    touchTones.Add(touchTone);
                }
            }

            return touchTones.ToArray();
        }

        /// <summary>
        /// Converts the character representation of a touch tone key into 
        /// an instance of the <see cref="TouchTone"/> class. A return value
        /// indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="key">A character containing a touch tone key to convert.</param>
        /// <param name="result">
        /// When this method returns, contains an instance of the <see cref="TouchTone"/>
        /// class equivalent to the touch tone key, if the conversion succeeded, or null
        /// if the conversion failed. The conversion fails if the <paramref name="key"/>
        /// parameter is not a valid touch tone.</param>
        /// <returns>true if s was converted successfully; otherwise, false.</returns>
        public static bool TryParse(char key, out TouchTone result)
        {
            TouchToneKey parsedKey;

            if (TryParseKey(key, out parsedKey))
            {
                result = new TouchTone(parsedKey);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Converts the character representation of a touch tone key into
        /// an instance of the <see cref="TouchTone"/> class.
        /// </summary>
        /// <param name="key">A character containing a touch tone key to convert.</param>
        /// <returns>
        /// An instance of the <see cref="TouchTone"/> class equivalent to the touch tone
        /// chracter contained in <paramref name="key"/>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="key"/> is not a valid touch tone character.</exception>
        public static TouchTone Parse(char key)
        {
            TouchToneKey parsedKey;

            if (TryParseKey(key, out parsedKey))
                return new TouchTone(parsedKey);
            else
                throw new ArgumentException(string.Format("\'{0}\' is not a valid touch tone", key), "key");
        }

        // Convert character into touch tone key enumeration
        private static bool TryParseKey(char key, out TouchToneKey result)
        {
            int index = ValidTouchTones.IndexOf(key);

            if (index > -1)
            {
                result = (TouchToneKey)index;
                return true;
            }
            else
            {
                result = default(TouchToneKey);
                return false;
            }
        }

        #endregion
    }
}
