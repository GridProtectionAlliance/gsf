//*******************************************************************************************************
//  TVA.Scheduling.ScheduleEventArgs.vb - Defines an event argument for scheduling.
//  Copyright © 2005 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/01/2006 - Pinal C. Patel
//      Generated original version of source code.
//  09/15/2008 - J. Ritchie Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Scheduling
{
    /// <summary>Defines an event argument for schedules.</summary>
    public class ScheduleEventArgs : EventArgs
    {
        private Schedule m_schedule;

        public ScheduleEventArgs(Schedule schedule)
        {
            m_schedule = schedule;
        }

        public Schedule Schedule
        {
            get
            {
                return m_schedule;
            }
            set
            {
                m_schedule = value;
            }
        }
    }
}