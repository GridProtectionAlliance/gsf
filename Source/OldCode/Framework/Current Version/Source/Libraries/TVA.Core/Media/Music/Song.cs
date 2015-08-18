//*******************************************************************************************************
//  Song.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//  Code in this file licensed to TVA under one or more contributor license agreements listed below.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/29/2009 - J. Ritchie Carroll
//       Generated original version of source code.
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

#region [ Contributor License Agreements ]

/**************************************************************************\
   Copyright © 2009 - J. Ritchie Carroll
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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace TVA.Media.Music
{
    /// <summary>
    /// Allows creation of a synthesized musical score storing the resultant song in an
    /// in-memory wave file for play back or saving music to disk.
    /// </summary>
    /// <example>
    /// This example generates a multi-instrument chord:
    /// <code>
    /// using System;
    /// using TVA.Media;
    /// using TVA.Media.Music;
    ///
    /// static class Program
    /// {
    ///     static void Main()
    ///     {
    ///         Song song = new Song { Damping = Damping.Linear };
    ///
    ///         Console.WriteLine("Generating multi-instrument chord...");
    ///
    ///         song.AddNotes
    ///         (
    ///             new Note { Frequency = Note.C4, Value = 4, Timbre = Timbre.EvenHarmonicSeries },
    ///             new Note { Frequency = Note.C4, Value = 4, Timbre = Timbre.Clarinet },
    ///             new Note { Frequency = Note.C4, Value = 4, Timbre = Timbre.Organ },
    ///             new Note { Frequency = Note.E4, Value = 4, Timbre = Timbre.EvenHarmonicSeries },
    ///             new Note { Frequency = Note.E4, Value = 4, Timbre = Timbre.Clarinet },
    ///             new Note { Frequency = Note.E4, Value = 4, Timbre = Timbre.Organ },
    ///             new Note { Frequency = Note.G4, Value = 4, Timbre = Timbre.EvenHarmonicSeries },
    ///             new Note { Frequency = Note.G4, Value = 4, Timbre = Timbre.SimulatedClarinet },
    ///             new Note { Frequency = Note.G4, Value = 4, Timbre = Timbre.SimulatedOrgan }
    ///         );
    ///
    ///         song.Finish();
    ///
    ///         Console.WriteLine("Saving chord to disk...");
    ///         song.Save("MajorTriad.wav");
    ///
    ///         Console.WriteLine("Playing chord...");
    ///         song.Play();
    ///
    ///         Console.ReadKey();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This example generates a familiar tune, plays the song and saves it to disk:
    /// <code>
    /// // Add reference to System.Speech
    /// using System;
    /// using System.IO;
    /// using TVA.Media;
    /// using TVA.Media.Music;
    /// using System.Speech;
    /// using System.Speech.Synthesis;
    /// using System.Speech.AudioFormat;
    /// 
    /// static class Program
    /// {
    ///     static void Main()
    ///     {
    ///         Console.WriteLine("Synthesizing speech...");
    ///         WaveFile speech = CreateSynthesizedVoiceOver();
    /// 
    ///         Console.WriteLine("Synthesizing song at tempo for use with speech...");
    /// 
    ///         // Define all the notes of jingle bells as a single phrase of music
    ///         Phrase score = CreateJingleBellsScore();
    /// 
    ///         // Create one song at a slower tempo to help with speech synchronization
    ///         Song speechTempSong = new Song { Tempo = new Tempo(160, NoteValue.Quarter) };
    /// 
    ///         // Make sure audio specifications for song and speech match
    ///         speechTempSong.SampleRate = speech.SampleRate;
    ///         speechTempSong.BitsPerSample = speech.BitsPerSample;
    ///         speechTempSong.Channels = speech.Channels;
    /// 
    ///         // Add all the notes to the song
    ///         speechTempSong.AddPhrase(score);
    ///         speechTempSong.Finish();
    /// 
    ///         Console.WriteLine("Synthesizing song by itself at normal tempo...");
    /// 
    ///         // Create one song at a slower tempo to help with speech synchronization
    ///         Song normalTempoSong = new Song();
    /// 
    ///         // Add all the notes to the song
    ///         normalTempoSong.AddPhrase(score);
    ///         normalTempoSong.Finish();
    /// 
    ///         Console.WriteLine("Saving normal tempo song to disk as \"JingleBells.wav\"...");
    ///         normalTempoSong.Save("JingleBells.wav");
    /// 
    ///         Console.WriteLine("Combining speech with song...");
    ///         WaveFile combined = WaveFile.Combine(speech, speechTempSong);
    /// 
    ///         Console.WriteLine("Saving combined work to disk as \"SingingComputer.wav\"...");
    ///         combined.Save("SingingComputer.wav");
    /// 
    ///         Console.WriteLine("Playing combined work...");
    ///         combined.Play();
    /// 
    ///         Console.ReadKey();
    ///     }
    /// 
    ///     private static WaveFile CreateSynthesizedVoiceOver()
    ///     {
    ///         SpeechSynthesizer synthesizer = new SpeechSynthesizer();
    ///         MemoryStream speechStream = new MemoryStream();
    ///         PromptBuilder songText = new PromptBuilder();
    /// 
    ///         synthesizer.SelectVoice("Microsoft Sam");
    ///         synthesizer.Rate = 5; // Range = -10 to +10
    ///         synthesizer.SetOutputToWaveStream(speechStream);
    /// 
    ///         songText.AppendText("Jin - gull bells!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(3000000));
    ///         songText.AppendText("Jin - gull bells!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(3000000));
    ///         songText.AppendText("Jin - gull - all, theuh - way!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(12000000));
    ///         songText.AppendText("Oh - what - fun!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(1000000));
    ///         songText.AppendText("It - is!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("To - ride!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("a - one - horse!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("Open!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("Sleigh!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(1000000));
    ///         songText.AppendText("A!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(2500000));
    ///         songText.AppendText("Jin - gull bells!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(3000000));
    ///         songText.AppendText("Jin - gull bells!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(3000000));
    ///         songText.AppendText("Jin - gull - all, theuh - way!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(12000000));
    ///         songText.AppendText("Oh - what - fun!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(1000000));
    ///         songText.AppendText("It - is!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("To - ride!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("a - one - horse!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("Open!", PromptEmphasis.Strong);
    ///         songText.AppendBreak(new TimeSpan(500000));
    ///         songText.AppendText("Sleigh.", PromptEmphasis.Reduced);
    /// 
    ///         synthesizer.Speak(songText);
    ///         speechStream.Position = 0;
    ///         return WaveFile.Load(speechStream);
    ///     }
    /// 
    ///     private static Phrase CreateJingleBellsScore()
    ///     {
    ///         Phrase score = new Phrase();
    ///         Phrase passage = new Phrase();
    /// 
    ///         // Define the repeating phrase of the song
    ///         passage.AddNotes
    ///         (
    ///             new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter },
    ///             new Note { Frequency = Note.C3, NamedValue = NoteValue.Whole }
    ///         );
    ///         passage.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Half });
    /// 
    ///         passage.AddNotes
    ///         (
    ///             new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter },
    ///             new Note { Frequency = Note.G3, NamedValue = NoteValue.Whole }
    ///         );
    ///         passage.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Half });
    /// 
    ///         passage.AddNotes
    ///         (
    ///             new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter },
    ///             new Note { Frequency = Note.C3, NamedValue = NoteValue.Whole }
    ///         );
    ///         passage.AddNotes(new Note { Frequency = Note.D4, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.G3, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.A3, NamedValue = NoteValue.Quarter });
    /// 
    ///         passage.AddNotes
    ///         (
    ///             new Note { Frequency = Note.B3, NamedValue = NoteValue.Whole },
    ///             new Note { Frequency = Note.G3, NamedValue = NoteValue.Whole }
    ///         );
    /// 
    ///         passage.AddNotes
    ///         (
    ///             new Note { Frequency = Note.C4, NamedValue = NoteValue.Quarter },
    ///             new Note { Frequency = Note.D3, NamedValue = NoteValue.Whole }
    ///         );
    ///         passage.AddNotes(new Note { Frequency = Note.C4, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.C4, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.C4, NamedValue = NoteValue.Quarter });
    /// 
    ///         passage.AddNotes
    ///         (
    ///             new Note { Frequency = Note.C4, NamedValue = NoteValue.Quarter },
    ///             new Note { Frequency = Note.G3, NamedValue = NoteValue.Whole }
    ///         );
    ///         passage.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter });
    ///         passage.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter });
    /// 
    ///         score.AddPhrase(passage);
    /// 
    ///         score.AddNotes
    ///         (
    ///             new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter },
    ///             new Note { Frequency = Note.F3S, NamedValue = NoteValue.Whole }
    ///         );
    ///         score.AddNotes(new Note { Frequency = Note.A3, NamedValue = NoteValue.Quarter });
    ///         score.AddNotes(new Note { Frequency = Note.A3, NamedValue = NoteValue.Quarter });
    ///         score.AddNotes(new Note { Frequency = Note.B3, NamedValue = NoteValue.Quarter });
    /// 
    ///         score.AddNotes
    ///         (
    ///             new Note { Frequency = Note.A3, NamedValue = NoteValue.Half },
    ///             new Note { Frequency = Note.G3, NamedValue = NoteValue.Whole }
    ///         );
    ///         score.AddNotes(new Note { Frequency = Note.D4, NamedValue = NoteValue.Half });
    /// 
    ///         score.AddPhrase(passage);
    /// 
    ///         score.AddNotes
    ///         (
    ///             new Note { Frequency = Note.D4, NamedValue = NoteValue.Quarter },
    ///             new Note { Frequency = Note.G3, NamedValue = NoteValue.Whole },
    ///             new Note { Frequency = Note.F3, NamedValue = NoteValue.Whole }
    ///         );
    ///         score.AddNotes(new Note { Frequency = Note.D4, NamedValue = NoteValue.Quarter });
    ///         score.AddNotes(new Note { Frequency = Note.C4, NamedValue = NoteValue.Quarter });
    ///         score.AddNotes(new Note { Frequency = Note.A3, NamedValue = NoteValue.Quarter });
    /// 
    ///         score.AddNotes
    ///         (
    ///             new Note { Frequency = Note.G3, NamedValue = NoteValue.Whole },
    ///             new Note { Frequency = Note.E3, NamedValue = NoteValue.Whole }
    ///         );
    /// 
    ///         return score;
    ///     }
    /// }
    /// </code>
    /// </example>
    public class Song : WaveFile
    {
        #region [ Members ]

        // Fields
        private MeasureSize m_measureSize;
        private TimbreFunction m_timbre;
        private DampingFunction m_damping;
        private Tempo m_tempo;
        private List<Note> m_noteQueue;
        private double m_dynamic;
        private double m_interNoteDelay;
        private long m_currentSample;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new song with a 3/4 measure size, a tempo of 240 quarter-notes per minute,
        /// mezzo-forte prevailing dynamic level, using a basic note timbre and standard CD-quality
        /// settings for the underlying sound file.
        /// </summary>
        public Song()
        {
            m_measureSize = new MeasureSize(3, NoteValue.Quarter);
            m_tempo = new Tempo(240, NoteValue.Quarter);
            m_dynamic = (double)Music.Dynamic.MezzoForte / 100.0D;
            m_timbre = Music.Timbre.BasicNote;
            m_damping = Music.Damping.Natural;
            m_noteQueue = new List<Note>();
        }

        /// <summary>
        /// Creates a new song with a 3/4 measure size, a tempo of 240 quarter-notes per minute,
        /// mezzo-forte prevailing dynamic level, using a basic note timbre and the specified
        /// audio format settings.
        /// </summary>
        /// <param name="sampleRate">Desired sample rate</param>
        /// <param name="bitsPerSample">Desired bits-per-sample</param>
            /// <param name="channels">Desired data channels</param>
        public Song(SampleRate sampleRate, BitsPerSample bitsPerSample, DataChannels channels)
            : base(sampleRate, bitsPerSample, channels)
        {
            m_measureSize = new MeasureSize(3, NoteValue.Quarter);
            m_tempo = new Tempo(240, NoteValue.Quarter);
            m_dynamic = (double)Music.Dynamic.MezzoForte / 100.0D;
            m_timbre = Music.Timbre.BasicNote;
            m_damping = Music.Damping.Natural;
            m_noteQueue = new List<Note>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets current measure size or defines a new measure size for the song.
        /// </summary>
        public MeasureSize MeasureSize
        {
            get
            {
                return m_measureSize;
            }
            set
            {
                m_measureSize = value;
            }
        }

        /// <summary>
        /// Gets current tempo or defines a new tempo for the song.
        /// </summary>
        public Tempo Tempo
        {
            get
            {
                return m_tempo;
            }
            set
            {
                m_tempo = value;
            }
        }

        /// <summary>
        /// Returns the time for a single beat.
        /// </summary>
        public double BeatTime
        {
            get
            {
                return m_tempo.CalculateNoteValueTime(m_measureSize.NamedNoteValue, 0);
            }
        }

        /// <summary>
        /// Gets or sets the default tibre function used to synthesize the sounds
        /// of the added notes (i.e., the instrument). This timbre function will be
        /// used if no other function is specified when adding notes.
        /// </summary>
        public TimbreFunction Timbre
        {
            get
            {
                return m_timbre;
            }
            set
            {
                m_timbre = value;
            }
        }

        /// <summary>
        /// Gets or sets the default damping function used to lower the sound volume
        /// of the added notes over time. This damping function will be used if no
        /// other function is specified when adding notes.
        /// </summary>
        public DampingFunction Damping
        {
            get
            {
                return m_damping;
            }
            set
            {
                m_damping = value;
            }
        }

        /// <summary>
        /// Gets or sets the prevailing named dynamic (i.e., volume) for the song.  Individual notes
        /// can choose to override this dynamic.
        /// </summary>
        public Dynamic NamedDynamic
        {
            get
            {
                // Dynamic can be custom, so return closest match...
                int dynamic = (int)m_dynamic * 100;

                if (dynamic <= (int)Music.Dynamic.Pianissimo)
                {
                    return Music.Dynamic.Pianissimo;
                }
                else if (dynamic <= (int)Music.Dynamic.Piano)
                {
                    return Music.Dynamic.Piano;
                }
                else if (dynamic <= (int)Music.Dynamic.MezzoPiano)
                {
                    return Music.Dynamic.MezzoPiano;
                }
                else if (dynamic <= (int)Music.Dynamic.MezzoForte)
                {
                    return Music.Dynamic.MezzoForte;
                }
                else if (dynamic <= (int)Music.Dynamic.Forte)
                {
                    return Music.Dynamic.Forte;
                }
                else
                {
                    return Music.Dynamic.Fortissimo;
                }
            }
            set
            {
                if (value == Music.Dynamic.Undefined)
                    value = Music.Dynamic.MezzoForte;

                m_dynamic = (double)value / 100.0D;
            }
        }

        /// <summary>
        /// Gets or sets the prevailing dynamic (i.e., volume) expressed as percentage in the range
        /// of 0 to 1 for the song. Individual notes can choose to override this dynamic.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Value must be expressed as a fractional percentage between zero and one.
        /// </exception>
        public double Dynamic
        {
            get
            {
                return m_dynamic;
            }
            set
            {
                if (value != -1.0D && (value < 0.0D || value > 1.0D))
                    throw new ArgumentOutOfRangeException("value", "Value must be expressed as a fractional percentage between zero and one");

                if (value == -1.0D)
                    m_dynamic = (double)Music.Dynamic.MezzoForte / 100.0D;
                else
                    m_dynamic = value;
            }
        }

        /// <summary>Injects specified rest time, in seconds, between notes.</summary>
        public double InterNoteDelay
        {
            get
            {
                return m_interNoteDelay;
            }
            set
            {
                m_interNoteDelay = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Add a predefined phrase of notes to the song.
        /// </summary>
        /// <param name="phrase">Phrase to add.</param>
        public void AddPhrase(Phrase phrase)
        {
            foreach (Note[] notes in phrase.Notes)
            {
                AddNotes(notes);
            }
        }

        /// <summary>
        /// Add a series of notes to the song.
        /// </summary>
        /// <param name="notes">Notes to add.</param>
        public void AddNotes(params Note[] notes)
        {
            double samplesPerSecond = SampleRate;
            long samplePeriod;

            // Calculate value time, start and end time indicies for each note to be added
            foreach (Note note in notes)
            {
                // Calculate note value time given current tempo
                note.CalculateValueTime(m_tempo);

                // Assign relative note durations in terms of sample rate
                note.SamplePeriod = (long)(note.ValueTime * samplesPerSecond);
                note.StartTimeIndex = m_currentSample;
                note.EndTimeIndex = note.StartTimeIndex + note.SamplePeriod - 1;
            }

            // Assign sample period to note with shortest duration - all other notes
            // will remain in queue until they have completed their run
            samplePeriod = notes.Min(note => note.SamplePeriod);

            // Add notes to note queue
            m_noteQueue.AddRange(notes);

            // Add queued notes to song for given sample period
            AddQueuedNotesToSong(samplePeriod);
        }

        /// <summary>
        /// Called when there are no more notes to add.
        /// </summary>
        /// <remarks>
        /// This flushes the remaining queued notes into the song allowing them to run their remaining time.
        /// </remarks>
        public void Finish()
        {
            if (m_noteQueue.Count > 0)
            {
                double samplesPerSecond = SampleRate;
                long samplePeriod;

                // Assign sample period to note with longest duration - this makes sure all remaining
                // queued notes will complete their run
                samplePeriod = (long)(m_noteQueue.Max(note => note.ValueTime) * samplesPerSecond);

                // Add queued notes to song for given sample period
                AddQueuedNotesToSong(samplePeriod);
            }
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        public void AddRest(double restLength)
        {
            AddNotes(new Note { Value = restLength });
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        public void AddRest(NoteValue restLength)
        {
            AddNotes(new Note { NamedValue = restLength });
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        public void AddRest(NoteValueBritish restLength)
        {
            AddNotes(new Note { NamedValueBritish = restLength });
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        /// <param name="dots">Total dotted note length extensions to apply.</param>
        public void AddRest(NoteValue restLength, int dots)
        {
            AddNotes(new Note { NamedValue = restLength, Dots = dots });
        }

        /// <summary>
        /// Add a rest for the given length for the current beat.
        /// </summary>
        /// <param name="restLength">Duration of wait specified as a note value.</param>
        /// <param name="dots">Total dotted note length extensions to apply.</param>
        public void AddRest(NoteValueBritish restLength, int dots)
        {
            AddNotes(new Note { NamedValueBritish = restLength, Dots = dots });
        }

        /// <summary>
        /// Starts crescendo dynamic over the range of the specified number of beats.
        /// </summary>
        /// <param name="totalBeats">Total number of beats overwhich to gradually increase volume.</param>
        /// <param name="endDynamic">Desired volume when crescendo is complete.</param>
        /// <remarks>
        /// Current <see cref="Dynamic"/> is the starting dynamic which should be less than <paramref name="endDynamic"/>.
        /// </remarks>
        public void SetCrescendoDynamic(int totalBeats, Dynamic endDynamic)
        {
            // TODO: implement crescendo dynamic
        }

        /// <summary>
        /// Starts diminuendo dynamic over the range of the specified number of beats.
        /// </summary>
        /// <param name="totalBeats">Total number of beats overwhich to gradually decrease volume.</param>
        /// <param name="endDynamic">Desired volume when diminuendo is complete.</param>
        /// <remarks>
        /// Current <see cref="Dynamic"/> is the starting dynamic which should be greater than <paramref name="endDynamic"/>.
        /// </remarks>
        public void SetDiminuendoDynamic(int totalBeats, Dynamic endDynamic)
        {
            // TODO: implement diminuendo dynamic
        }

        private void AddQueuedNotesToSong(long samplePeriod)
        {
            List<int> completedNotes = new List<int>();
            TimbreFunction timbre;
            DampingFunction damping;
            double sample, dynamic;
            Note note;
            int x;

            for (long sampleIndex = m_currentSample; sampleIndex < m_currentSample + samplePeriod + (long)(m_interNoteDelay * SampleRate); sampleIndex++)
            {
                // Create summation of all notes at given time
                sample = 0.0D;

                for (x = 0; x < m_noteQueue.Count; x++)
                {
                    if (!completedNotes.Contains(x))
                    {
                        note = m_noteQueue[x];

                        if (sampleIndex < note.EndTimeIndex)
                        {
                            // Get timbre function
                            timbre = (note.Timbre == null ? m_timbre : timbre = note.Timbre);

                            // Get damping function
                            damping = (note.Damping == null ?  m_damping : note.Damping);

                            // Get note dynamic
                            dynamic = (note.Dynamic == -1.0D ? m_dynamic : note.Dynamic);

                            // Generate note at given time
                            sample +=
                                timbre(note.Frequency, sampleIndex, note.SamplePeriod, SampleRate) *
                                damping(sampleIndex - m_currentSample, note.SamplePeriod, SampleRate) *
                                dynamic;
                        }
                        else
                        {
                            completedNotes.Add(x);
                        }
                    }
                }

                // Adjust sample amplitude so as to not adversely affect the volume of the song
                AddSample(sample % 1.0D * AmplitudeScalar);
            }

            m_currentSample += samplePeriod;

            // Remove completed notes from queue - removal is in reverse sorted
            // order to preserve indices
            completedNotes.Sort();
            completedNotes.Reverse();

            for (x = 0; x < completedNotes.Count; x++)
            {
                m_noteQueue.RemoveAt(completedNotes[x]);
            }
        }

        #endregion
    }
}
