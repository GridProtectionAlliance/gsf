//*******************************************************************************************************
//  Note.cs - Gbtc
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
//  08/06/2009 - Josh L. Patterson
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
using System.ComponentModel;
using System.Reflection;

namespace TVA.Media.Music
{
    #region [ Namespace Documentation ]

    /// <tocexclude />
    /// <remarks>
    /// This namespace is excluded from the official documentation TOC.
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    class NamespaceDoc
    {
    }
    
    #endregion

    /// <summary>
    /// Defines fundamental musical note frequencies and methods to create them.
    /// </summary>
    /// <example>
    /// This example creates an in-memory wave file and adds notes to create a basic musical scale:
    /// <code>
    /// using System;
    /// using TVA.Media;
    /// using TVA.Media.Music;
    ///
    /// static class Program
    /// {
    ///     static void Main()
    ///      {
    ///          WaveFile waveFile = new WaveFile();
    ///          long samplePeriod = 6 * waveFile.SampleRate;   // Compute total sample period
    ///          int totalNotes = 15;                           // Total notes to traverse
    ///          string noteID = Note.MiddleC;                  // Start note at middle C
    ///          double frequency = Note.GetFrequency(noteID);  // Get frequency for middle C
    ///          bool reverse = false;                          // Traverse notes in reverse order
    ///
    ///          for (int sample = 0; sample <![CDATA[<]]> samplePeriod; sample++)
    ///          {
    ///              // Change notes at even intervals within the sample period
    ///              if (sample > 0 <![CDATA[&&]]> (sample % (samplePeriod / totalNotes)) == 0)
    ///              {
    ///                  if (reverse)
    ///                  {
    ///                      noteID = Note.GetPreviousID(noteID, false);
    ///                      frequency = Note.GetFrequency(noteID);
    ///                  }
    ///                  else
    ///                  {
    ///                      noteID = Note.GetNextID(noteID, false);
    ///                      frequency = Note.GetFrequency(noteID);
    ///                  }
    ///
    ///                  // Go back down the scale after C5
    ///                  if (noteID == "C5")
    ///                      reverse = true;
    ///              }
    ///
    ///              waveFile.AddSample(Timbre.BasicNote(frequency, sample, samplePeriod, waveFile.SampleRate) * 4500);
    ///          }
    ///
    ///          waveFile.Play();
    ///          Console.ReadKey();
    ///      }
    /// }
    /// </code>
    /// </example>
    public class Note : IEquatable<Note>, IComparable<Note>, IComparable
    {
        #region [ Members ]

        // Constants

        // Fundamental musical note frequencies (http://www.phy.mtu.edu/~suits/notefreqs.html)

        /// <summary>
        /// Fundamental frequency for note C0.
        /// </summary>
        public const double C0 = 16.35;

        /// <summary>
        /// Fundamental frequency for note C0#.
        /// </summary>
        public const double C0S = 17.32;

        /// <summary>
        /// Fundamental frequency for note D0.
        /// </summary>
        public const double D0 = 18.35;

        /// <summary>
        /// Fundamental frequency for note D0#.
        /// </summary>
        public const double D0S = 19.45;

        /// <summary>
        /// Fundamental frequency for note E0.
        /// </summary>
        public const double E0 = 20.6;

        /// <summary>
        /// Fundamental frequency for note F0.
        /// </summary>
        public const double F0 = 21.83;

        /// <summary>
        /// Fundamental frequency for note F0#.
        /// </summary>
        public const double F0S = 23.12;

        /// <summary>
        /// Fundamental frequency for note G0.
        /// </summary>
        public const double G0 = 24.5;

        /// <summary>
        /// Fundamental frequency for note G0#.
        /// </summary>
        public const double G0S = 25.96;

        /// <summary>
        /// Fundamental frequency for note A0.
        /// </summary>
        public const double A0 = 27.5;

        /// <summary>
        /// Fundamental frequency for note A0#.
        /// </summary>
        public const double A0S = 29.14;

        /// <summary>
        /// Fundamental frequency for note B0.
        /// </summary>
        public const double B0 = 30.87;

        /// <summary>
        /// Fundamental frequency for note C1.
        /// </summary>
        public const double C1 = 32.7;

        /// <summary>
        /// Fundamental frequency for note C1#.
        /// </summary>
        public const double C1S = 34.65;

        /// <summary>
        /// Fundamental frequency for note D1.
        /// </summary>
        public const double D1 = 36.71;

        /// <summary>
        /// Fundamental frequency for note D1#.
        /// </summary>
        public const double D1S = 38.89;

        /// <summary>
        /// Fundamental frequency for note E1.
        /// </summary>
        public const double E1 = 41.2;

        /// <summary>
        /// Fundamental frequency for note F1.
        /// </summary>
        public const double F1 = 43.65;

        /// <summary>
        /// Fundamental frequency for note F1#.
        /// </summary>
        public const double F1S = 46.25;

        /// <summary>
        /// Fundamental frequency for note G1.
        /// </summary>
        public const double G1 = 49.0;

        /// <summary>
        /// Fundamental frequency for note G1#.
        /// </summary>
        public const double G1S = 51.91;

        /// <summary>
        /// Fundamental frequency for note A1.
        /// </summary>
        public const double A1 = 55.0;

        /// <summary>
        /// Fundamental frequency for note A1#.
        /// </summary>
        public const double A1S = 58.27;

        /// <summary>
        /// Fundamental frequency for note B1.
        /// </summary>
        public const double B1 = 61.74;

        /// <summary>
        /// Fundamental frequency for note C2.
        /// </summary>
        public const double C2 = 65.41;

        /// <summary>
        /// Fundamental frequency for note C2#.
        /// </summary>
        public const double C2S = 69.3;

        /// <summary>
        /// Fundamental frequency for note D2.
        /// </summary>
        public const double D2 = 73.42;

        /// <summary>
        /// Fundamental frequency for note D2#.
        /// </summary>
        public const double D2S = 77.78;

        /// <summary>
        /// Fundamental frequency for note E2.
        /// </summary>
        public const double E2 = 82.41;

        /// <summary>
        /// Fundamental frequency for note F2.
        /// </summary>
        public const double F2 = 87.31;

        /// <summary>
        /// Fundamental frequency for note F2#.
        /// </summary>
        public const double F2S = 92.5;

        /// <summary>
        /// Fundamental frequency for note G2.
        /// </summary>
        public const double G2 = 98.0;

        /// <summary>
        /// Fundamental frequency for note G2#.
        /// </summary>
        public const double G2S = 103.83;

        /// <summary>
        /// Fundamental frequency for note A2.
        /// </summary>
        public const double A2 = 110.0;

        /// <summary>
        /// Fundamental frequency for note A2#.
        /// </summary>
        public const double A2S = 116.54;

        /// <summary>
        /// Fundamental frequency for note B2.
        /// </summary>
        public const double B2 = 123.47;

        /// <summary>
        /// Fundamental frequency for note C3.
        /// </summary>
        public const double C3 = 130.81;

        /// <summary>
        /// Fundamental frequency for note C3#.
        /// </summary>
        public const double C3S = 138.59;

        /// <summary>
        /// Fundamental frequency for note D3.
        /// </summary>
        public const double D3 = 146.83;

        /// <summary>
        /// Fundamental frequency for note D3#.
        /// </summary>
        public const double D3S = 155.56;

        /// <summary>
        /// Fundamental frequency for note E3.
        /// </summary>
        public const double E3 = 164.81;

        /// <summary>
        /// Fundamental frequency for note F3.
        /// </summary>
        public const double F3 = 174.61;

        /// <summary>
        /// Fundamental frequency for note F3#.
        /// </summary>
        public const double F3S = 185.0;

        /// <summary>
        /// Fundamental frequency for note G3.
        /// </summary>
        public const double G3 = 196.0;

        /// <summary>
        /// Fundamental frequency for note G3#.
        /// </summary>
        public const double G3S = 207.65;

        /// <summary>
        /// Fundamental frequency for note A3.
        /// </summary>
        public const double A3 = 220.0;

        /// <summary>
        /// Fundamental frequency for note A3#.
        /// </summary>
        public const double A3S = 233.08;

        /// <summary>
        /// Fundamental frequency for note B3.
        /// </summary>
        public const double B3 = 246.94;

        /// <summary>
        /// Fundamental frequency for note C4 - Middle C.
        /// </summary>
        public const double C4 = 261.63;

        /// <summary>
        /// Fundamental frequency for note C4#.
        /// </summary>
        public const double C4S = 277.18;

        /// <summary>
        /// Fundamental frequency for note D4.
        /// </summary>
        public const double D4 = 293.66;

        /// <summary>
        /// Fundamental frequency for note D4#.
        /// </summary>
        public const double D4S = 311.13;

        /// <summary>
        /// Fundamental frequency for note E4.
        /// </summary>
        public const double E4 = 329.63;

        /// <summary>
        /// Fundamental frequency for note F4.
        /// </summary>
        public const double F4 = 349.23;

        /// <summary>
        /// Fundamental frequency for note F4#.
        /// </summary>
        public const double F4S = 369.99;

        /// <summary>
        /// Fundamental frequency for note G4.
        /// </summary>
        public const double G4 = 392.0;

        /// <summary>
        /// Fundamental frequency for note G4#.
        /// </summary>
        public const double G4S = 415.3;

        /// <summary>
        /// Fundamental frequency for note A4.
        /// </summary>
        public const double A4 = 440.0;

        /// <summary>
        /// Fundamental frequency for note A4#.
        /// </summary>
        public const double A4S = 466.16;

        /// <summary>
        /// Fundamental frequency for note B4.
        /// </summary>
        public const double B4 = 493.88;

        /// <summary>
        /// Fundamental frequency for note C5.
        /// </summary>
        public const double C5 = 523.25;

        /// <summary>
        /// Fundamental frequency for note C5#.
        /// </summary>
        public const double C5S = 554.37;

        /// <summary>
        /// Fundamental frequency for note D5.
        /// </summary>
        public const double D5 = 587.33;

        /// <summary>
        /// Fundamental frequency for note D5#.
        /// </summary>
        public const double D5S = 622.25;

        /// <summary>
        /// Fundamental frequency for note E5.
        /// </summary>
        public const double E5 = 659.26;

        /// <summary>
        /// Fundamental frequency for note F5.
        /// </summary>
        public const double F5 = 698.46;

        /// <summary>
        /// Fundamental frequency for note F5#.
        /// </summary>
        public const double F5S = 739.99;

        /// <summary>
        /// Fundamental frequency for note G5.
        /// </summary>
        public const double G5 = 783.99;

        /// <summary>
        /// Fundamental frequency for note G5#.
        /// </summary>
        public const double G5S = 830.61;

        /// <summary>
        /// Fundamental frequency for note A5.
        /// </summary>
        public const double A5 = 880.0;

        /// <summary>
        /// Fundamental frequency for note A5#.
        /// </summary>
        public const double A5S = 932.33;

        /// <summary>
        /// Fundamental frequency for note B5.
        /// </summary>
        public const double B5 = 987.77;

        /// <summary>
        /// Fundamental frequency for note C6.
        /// </summary>
        public const double C6 = 1046.5;

        /// <summary>
        /// Fundamental frequency for note C6#.
        /// </summary>
        public const double C6S = 1108.73;

        /// <summary>
        /// Fundamental frequency for note D6.
        /// </summary>
        public const double D6 = 1174.66;

        /// <summary>
        /// Fundamental frequency for note D6#.
        /// </summary>
        public const double D6S = 1244.51;

        /// <summary>
        /// Fundamental frequency for note E6.
        /// </summary>
        public const double E6 = 1318.51;

        /// <summary>
        /// Fundamental frequency for note F6.
        /// </summary>
        public const double F6 = 1396.91;

        /// <summary>
        /// Fundamental frequency for note F6#.
        /// </summary>
        public const double F6S = 1479.98;

        /// <summary>
        /// Fundamental frequency for note G6.
        /// </summary>
        public const double G6 = 1567.98;

        /// <summary>
        /// Fundamental frequency for note G6#.
        /// </summary>
        public const double G6S = 1661.22;

        /// <summary>
        /// Fundamental frequency for note A6.
        /// </summary>
        public const double A6 = 1760.0;

        /// <summary>
        /// Fundamental frequency for note A6#.
        /// </summary>
        public const double A6S = 1864.66;

        /// <summary>
        /// Fundamental frequency for note B6.
        /// </summary>
        public const double B6 = 1975.53;

        /// <summary>
        /// Fundamental frequency for note C7.
        /// </summary>
        public const double C7 = 2093.0;

        /// <summary>
        /// Fundamental frequency for note C7#.
        /// </summary>
        public const double C7S = 2217.46;

        /// <summary>
        /// Fundamental frequency for note D7.
        /// </summary>
        public const double D7 = 2349.32;

        /// <summary>
        /// Fundamental frequency for note D7#.
        /// </summary>
        public const double D7S = 2489.02;

        /// <summary>
        /// Fundamental frequency for note E7.
        /// </summary>
        public const double E7 = 2637.02;

        /// <summary>
        /// Fundamental frequency for note F7.
        /// </summary>
        public const double F7 = 2793.83;

        /// <summary>
        /// Fundamental frequency for note F7#.
        /// </summary>
        public const double F7S = 2959.96;

        /// <summary>
        /// Fundamental frequency for note G7.
        /// </summary>
        public const double G7 = 3135.96;

        /// <summary>
        /// Fundamental frequency for note G7#.
        /// </summary>
        public const double G7S = 3322.44;

        /// <summary>
        /// Fundamental frequency for note A7.
        /// </summary>
        public const double A7 = 3520.0;

        /// <summary>
        /// Fundamental frequency for note A7#.
        /// </summary>
        public const double A7S = 3729.31;

        /// <summary>
        /// Fundamental frequency for note B7.
        /// </summary>
        public const double B7 = 3951.07;

        /// <summary>
        /// Fundamental frequency for note C8.
        /// </summary>
        public const double C8 = 4186.01;

        /// <summary>
        /// Fundamental frequency for note C8#.
        /// </summary>
        public const double C8S = 4434.92;

        /// <summary>
        /// Fundamental frequency for note D8.
        /// </summary>
        public const double D8 = 4698.64;

        /// <summary>
        /// Fundamental frequency for note D8#.
        /// </summary>
        public const double D8S = 4978.03;

        /// <summary>Note ID for "Middle C"</summary>
        public const string MiddleC = "C4";

        // Fields
        private string m_ID;
        private double m_frequency;
        private double m_value;
        private double m_valueTime;
        private int m_dots;
        private long m_startTimeIndex;
        private long m_endTimeIndex;
        private long m_samplePeriod;
        private TimbreFunction m_timbre;
        private DampingFunction m_damping;
        private double m_dynamic;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new note of the specified frequency and length.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default note will be rest (zero frequency), quarter note.
        /// </para>
        /// <para>
        /// It is expected that <see cref="Note"/> objects will be constructed using object intializers.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// // Create a 3/8th length, middle C note
        /// Note note1 = new Note { Frequency = Note.C4, NamedValue = NoteValue.Quarter, Dots = 1 };
        /// 
        /// // Create a 1/8th length, E above middle C note
        /// Note note2 = new Note { ID = "E4", NamedValueBritish = NoteValueBritish.Quaver };
        /// 
        /// // Create a whole length, F# above middle C note, "p" soft dynamic
        /// Note note3 = new Note { Frequency = Note.F4S, Value = 1.0, NamedDynamic = Dynamic.Piano };
        /// </code>
        /// </example>
        public Note()
        {
            m_dynamic = -1.0D;
        }

        #endregion

        #region [ Properties ]

        /// <summary>Gets or sets frequency of this note.</summary>
        public double Frequency
        {
            get
            {
                return m_frequency;
            }
            set
            {
                if (m_frequency != value)
                    m_ID = null;

                m_frequency = value;
            }
        }

        /// <summary>Gets or sets note ID of the note.</summary>
        /// <exception cref="ArgumentNullException">noteID is null.</exception>
        /// <exception cref="ArgumentException">Invalid note ID format - expected "Note + Octave + S?" (e.g., A2 or C5S).</exception>
        public string ID
        {
            get
            {
                return ToString();
            }
            set
            {
                m_frequency = GetFrequency(value);
                m_ID = value;
            }
        }

        /// <summary>Get or sets the relative note value representing the length of the note.</summary>
        public double Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets the cached note value time, in seconds, calculated from a call to <see cref="CalculateValueTime"/>.
        /// </summary>
        public double ValueTime
        {
            get
            {
                return m_valueTime;
            }
        }

        /// <summary>Get or sets the note value, expressed in American form, representing the length of the note.</summary>
        public NoteValue NamedValue
        {
            get
            {
                return (NoteValue)Note.NamedValueIndex(m_value);
            }
            set
            {
                m_value = value.Duration(m_dots);
            }
        }

        /// <summary>Get or sets the note value, expressed in British form, representing the length of the note.</summary>
        public NoteValueBritish NamedValueBritish
        {
            get
            {
                return (NoteValueBritish)Note.NamedValueIndex(m_value);
            }
            set
            {
                m_value = value.Duration(m_dots);
            }
        }

        /// <summary>Gets or sets the total dotted note length extensions that apply to this note.</summary>
        /// <remarks>This is only used in conjunction with the named note values.</remarks>
        public int Dots
        {
            get
            {
                return m_dots;
            }
            set
            {
                m_dots = value;
                m_value = NamedValue.Duration(m_dots);
            }
        }

        /// <summary>
        /// Gets or sets the individual tibre function used to synthesize the sounds
        /// for this note (i.e., the instrument). If this timbre function is not defined,
        /// the timbre of the song will be used for the note.
        /// </summary>
        /// <remarks>
        /// Set this value to null to use current timbre function of the song.
        /// </remarks>
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
        /// Gets or sets the individual damping function used to lower the sound volume
        /// for this note over time. If this damping function is not defined, the
        /// damping algorithm of the song will be used for the note.
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
        /// Gets or sets the named dynamic (i.e., volume) for this note.  If the dynamic
        /// is undefined, the dynamic of the song will be used.
        /// </summary>
        /// <remarks>
        /// Set this value to undefined to use the current dynamic of the song.
        /// </remarks>
        public Dynamic NamedDynamic
        {
            get
            {
                if (m_dynamic == -1.0D)
                    return Music.Dynamic.Undefined;

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
                    m_dynamic = -1.0D;
                else
                    m_dynamic = (double)value / 100.0D;
            }
        }

        /// <summary>
        /// Gets or sets the dynamic (i.e., volume) expressed as percentage in
        /// the range of 0 to 1 for this note. If the dynamic is set to -1, the
        /// dynamic of the song will be used.
        /// </summary>
        /// <remarks>
        /// Set this value to -1 to use the current dynamic of the song.
        /// </remarks>
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
                    throw new ArgumentOutOfRangeException("value", "Value must be expressed as a fractional percentage between zero and one.");

                m_dynamic = value;
            }
        }

        /// <summary>Gets or sets start time index for this note.</summary>
        /// <remarks>This is typically assigned and used by host <see cref="Song"/></remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public long StartTimeIndex
        {
            get
            {
                return m_startTimeIndex;
            }
            set
            {
                m_startTimeIndex = value;
            }
        }

        /// <summary>Gets or sets end time index for this note.</summary>
        /// <remarks>This is typically assigned and used by host <see cref="Song"/></remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public long EndTimeIndex
        {
            get
            {
                return m_endTimeIndex;
            }
            set
            {
                m_endTimeIndex = value;
            }
        }

        /// <summary>Gets or sets the sample period for this note.</summary>
        /// <remarks>This is typically assigned and used by host <see cref="Song"/></remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public long SamplePeriod
        {
            get
            {
                return m_samplePeriod;
            }
            set
            {
                m_samplePeriod = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calculates the actual time duration, in seconds, for the specified tempo that
        /// the note value will last. For example, if tempo is M.M. 120 quarter-notes per
        /// minte, then each quarter-note would last a half-second.
        /// </summary>
        /// <param name="tempo">Tempo used to calculate note value time.</param>
        /// <returns>Calculated note value time.</returns>
        /// <remarks>
        /// Calculated value is cached and available from <see cref="ValueTime"/> property.
        /// </remarks>
        public double CalculateValueTime(Tempo tempo)
        {
            m_valueTime = tempo.CalculateNoteValueTime(m_value);
            return m_valueTime;
        }
        
        /// <summary>
        /// Returns a string representation for the note.
        /// </summary>
        /// <returns>A <see cref="String"/> value representation for the note.</returns>
        public override string ToString()
        {
            if (m_ID == null && m_frequency > 0.0D)
            {
                // Attempt to look up note ID
                foreach (FieldInfo field in typeof(Note).GetFields())
                {
                    if (m_frequency == (double)field.GetRawConstantValue())
                    {
                        m_ID = field.Name;
                        break;
                    }
                }

                // If no note ID was found for given frequency, just assign the
                // frequency as the note ID
                if (m_ID == null)
                    m_ID = m_frequency.ToString();
            }

            return m_ID;
        }

        /// <summary>Returns True if the frequency and value of this note equals the frequency and value of the specified other note.</summary>
        /// <param name="other">The other <see cref="Note"/> to compare against.</param>
        /// <returns>A <see cref="Boolean"/> indicating the result.</returns>
        public bool Equals(Note other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>Returns True if the frequency and value of this note equals the frequency and value of the specified other note.</summary>
        /// <param name="obj">The other <see cref="Object"/> to compare against.</param>
        /// <returns>A <see cref="Boolean"/> indicating the result.</returns>
        public override bool Equals(object obj)
        {
            Note other = obj as Note;
            if (other != null) return Equals(other);
            throw new ArgumentException("Object is not an Note", "obj");
        }

        /// <summary>Notes are compared by frequency, then by value (i.e., duration).</summary>
        /// <param name="other">A <see cref="Note" /> that is compared against.</param>
        /// <returns>An <see cref="Int32"/> that indicates: this object is greater than if 1, equal to if 0, or less than if -1.</returns>
        public int CompareTo(Note other)
        {
            int result = m_frequency.CompareTo(other.Frequency);

            if (result == 0)
                result = m_value.CompareTo(other.Value);

            return result;
        }

        /// <summary>Notes are compared by frequency, then by value (i.e., duration).</summary>
        /// <param name="obj">An <see cref="Object" /> that is compared against.</param>
        /// <returns>An <see cref="Int32"/> that indicates: this object is greater than if 1, equal to if 0, or less than if -1.</returns>
        public int CompareTo(object obj)
        {
            Note other = obj as Note;
            if (other != null) return CompareTo(other);
            throw new ArgumentException("Note can only be compared with other Notes...");
        }

        /// <summary>
        /// Serves as a hash function for the current <see cref="Note"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Note"/>.</returns>
        public override int GetHashCode()
        {
            return (Frequency * Value).GetHashCode();
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="Note"/> frequencies and values for equality.
        /// </summary>
        /// <param name="note1">A <see cref="Note"/> left hand operand.</param>
        /// <param name="note2">A <see cref="Note"/> right hand operand.</param>
        /// <returns>A boolean indicating the result of the comparison.</returns>
        public static bool operator ==(Note note1, Note note2)
        {
            return note1.Equals(note2);
        }

        /// <summary>
        /// Compares two <see cref="Note"/> frequencies and values for inequality.
        /// </summary>
        /// <param name="note1">A <see cref="Note"/> left hand operand.</param>
        /// <param name="note2">A <see cref="Note"/> right hand operand.</param>
        /// <returns>A boolean indicating the result of the comparison.</returns>
        public static bool operator !=(Note note1, Note note2)
        {
            return !note1.Equals(note2);
        }

        /// <summary>
        /// Returns true if left <see cref="Note"/> timestamp is greater than right <see cref="Note"/>.
        /// </summary>
        /// <param name="note1">A <see cref="Note"/> left hand operand.</param>
        /// <param name="note2">A <see cref="Note"/> right hand operand.</param>
        /// <returns>A boolean indicating the result of the comparison.</returns>
        public static bool operator >(Note note1, Note note2)
        {
            return note1.CompareTo(note2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Note"/> timestamp is greater than or equal to right <see cref="Note"/>.
        /// </summary>
        /// <param name="note1">A <see cref="Note"/> left hand operand.</param>
        /// <param name="note2">A <see cref="Note"/> right hand operand.</param>
        /// <returns>A boolean indicating the result of the comparison.</returns>
        public static bool operator >=(Note note1, Note note2)
        {
            return note1.CompareTo(note2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Note"/> timestamp is less than right <see cref="Note"/>.
        /// </summary>
        /// <param name="note1">A <see cref="Note"/> left hand operand.</param>
        /// <param name="note2">A <see cref="Note"/> right hand operand.</param>
        /// <returns>A boolean indicating the result of the comparison.</returns>
        public static bool operator <(Note note1, Note note2)
        {
            return note1.CompareTo(note2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="Note"/> timestamp is less than or equal to right <see cref="Note"/>.
        /// </summary>
        /// <param name="note1">A <see cref="Note"/> left hand operand.</param>
        /// <param name="note2">A <see cref="Note"/> right hand operand.</param>
        /// <returns>A boolean indicating the result of the comparison.</returns>
        public static bool operator <=(Note note1, Note note2)
        {
            return note1.CompareTo(note2) <= 0;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        static private int m_lastNamedValueIndex;

        // Static Constructor
        static Note()
        {
            // Compute last index of note value enumeration
            m_lastNamedValueIndex = Enum.GetValues(typeof(NoteValue)).Length - 1;
        }

        // Static Methods

        /// <summary>
        /// Returns closest note value index (for <see cref="NoteValue"/> or <see cref="NoteValueBritish"/>)
        /// given the relative duration of a note.
        /// </summary>
        /// <param name="value">Relative duration of the note.</param>
        /// <returns>Closest note value enumeration index given the relative duration of a note.</returns>
        public static int NamedValueIndex(double value)
        {
            int result = 2 - (int)Math.Log(value, 2);

            if (result < 0)
                return 0;
            
            if (result > m_lastNamedValueIndex)
                return m_lastNamedValueIndex;

            return result;
        }

        /// <summary>
        /// Gets the specified note frequency.
        /// </summary>
        /// <param name="noteID">ID of the note to retrieve - expected format is "Note + Octave + S?" (e.g., A2 or C5S)</param>
        /// <returns>The specified note.</returns>
        /// <exception cref="ArgumentNullException">noteID is null.</exception>
        /// <exception cref="ArgumentException">Invalid note ID format - expected "Note + Octave + S?" (e.g., A2 or C5S).</exception>
        public static double GetFrequency(string noteID)
        {
            noteID = ValidID(noteID);
            return GetFrequency(noteID[0], int.Parse(noteID[1].ToString()), noteID.Length > 2 && noteID[2] == 'S' ? true : false);
        }

        /// <summary>
        /// Gets the specified note frequency.
        /// </summary>
        /// <param name="note">Note (A - G) to retrieve.</param>
        /// <param name="octave">Octave of the the note to retrieve (0 - 8).</param>
        /// <param name="sharp">Indicates to get the "sharp" version of the note.</param>
        /// <returns>The specified note.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Notes must be A - G, octaves must be 0 - 8, first note is C0, last note is D8S.</exception>
        /// <exception cref="ArgumentException">Sharps are not defined for notes 'B' and 'E'.</exception>
        public static double GetFrequency(char note, int octave, bool sharp)
        {
            if (note < 'A' || note > 'G')
                throw new ArgumentOutOfRangeException("note", "Note must be A - G");

            if (octave < 0 || octave > 8)
                throw new ArgumentOutOfRangeException("octave", "Octave must be between 0 and 8");

            if (octave == 8 && (note < 'C' || note > 'D'))
                throw new ArgumentOutOfRangeException("note", "Maximum note defined for octave 8 is \'D#\'");

            if (sharp && (note == 'B' || note == 'E'))
                throw new ArgumentException("Sharps are not defined for notes \'B\' and \'E\'");

            return (double)typeof(Note).GetField(string.Format("{0}{1}{2}", note, octave, sharp ? "S" : "")).GetRawConstantValue();
        }

        /// <summary>
        /// Gets the next note ID in sequence after the specified note ID.
        /// </summary>
        /// <param name="noteID">ID of the current note - expected format is "Note + Octave + S?" (e.g., A2 or C5S)</param>
        /// <param name="includeSharps">Set to True to include sharp notes in the sequence.</param>
        /// <returns>The next note ID that is after the specified note ID.</returns>
        /// <exception cref="ArgumentNullException">noteID is null.</exception>
        /// <exception cref="ArgumentException">Invalid note ID format - expected "Note + Octave + S?" (e.g., A2 or C5S).</exception>
        public static string GetNextID(string noteID, bool includeSharps)
        {
            noteID = ValidID(noteID);

            char note = noteID[0];
            int octave = int.Parse(noteID[1].ToString());
            bool sharp = (noteID.Length > 2 && noteID[2] == 'S' ? true : false);

            // Transition to next octave after each B note
            if (note == 'B')
                octave++;

            // Include sharp notes if requested
            if (includeSharps && !sharp && note != 'B' && note != 'E')
            {
                sharp = true;
            }
            else
            {
                sharp = false;

                // Transition to next note frequency
                if (note == 'G')
                    note = 'A';
                else
                    note++;
            }

            return string.Format("{0}{1}{2}", note, octave, sharp ? "S" : "");
        }

        /// <summary>
        /// Gets the previous note ID in sequence before the specified note ID.
        /// </summary>
        /// <param name="noteID">ID of the current note - expected format is "Note + Octave + S?" (e.g., A2 or C5S)</param>
        /// <param name="includeSharps">Set to True to include sharp notes in the sequence.</param>
        /// <returns>The previous note ID that is before the specified note ID.</returns>
        /// <exception cref="ArgumentNullException">noteID is null.</exception>
        /// <exception cref="ArgumentException">Invalid note ID format - expected "Note + Octave + S?" (e.g., A2 or C5S).</exception>
        public static string GetPreviousID(string noteID, bool includeSharps)
        {
            noteID = ValidID(noteID);

            char note = noteID[0];
            int octave = int.Parse(noteID[1].ToString());
            bool transition = true, sharp = (noteID.Length > 2 && noteID[2] == 'S' ? true : false);

            // Transition to previous octave at each C note
            if (note == 'C' && !sharp)
                octave--;

            // Include sharp notes if requested
            if (includeSharps)
            {
                if (!sharp)
                {
                    if (note != 'C' && note != 'F')
                        sharp = true;
                    else
                        sharp = false;
                }
                else
                {
                    transition = false;
                    sharp = false;
                }
            }
            else
            {
                sharp = false;
            }

            if (transition)
            {
                // Transition to previous note frequency
                if (note == 'A')
                    note = 'G';
                else
                    note--;
            }

            return string.Format("{0}{1}{2}", note, octave, sharp ? "S" : "");
        }

        private static string ValidID(string noteID)
        {
            if (noteID == null)
                throw new ArgumentNullException("noteID");

            if (noteID.Length < 2)
                throw new ArgumentException("Invalid note ID format - expected \"Note + Octave + S?\" (e.g., A2 or C5S)");

            // TODO: RegEx validate note format...

            return noteID.ToUpper();
        }

        #endregion
    }
}
