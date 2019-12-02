using System;
using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel;
using GSF.Data.Model;

namespace GrafanaAdapters
{
    /// <summary>
    /// Represents the Alarm State of a connected Device.
    /// </summary>
    public class AlarmState
    {
        /// <summary>
        /// Unique ID.
        /// </summary>
        [PrimaryKey(true)]
        public int ID { get; set; }

        /// <summary>
        /// Description of the <see cref="AlarmState"/>.
        /// </summary>
        [StringLength(50)]
        public string State { get; set; }

        /// <summary>
        /// Color associated with the <see cref="AlarmState"/>.
        /// </summary>
        [StringLength(50)]
        public string Color { get; set; }
    }

    /// <summary>
    /// Represents the a connected Device in an AlarmState.
    /// </summary>
    public class AlarmDevice
    {
        /// <summary>
        /// Unique ID.
        /// </summary>
        [PrimaryKey(true)]
        public int ID { get; set; }

        /// <summary>
        /// Device ID of the Alarmed Device.
        /// </summary>
        public int DeviceID { get; set; }

        /// <summary>
        /// ID of the <see cref="AlarmState"/>.
        /// </summary>
        public int StateID { get; set; }

        /// <summary>
        /// Time of the last update.
        /// </summary>
        [DefaultValueExpression("DateTime.UtcNow")]
        [UpdateValueExpression("DateTime.UtcNow")]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// String to Diplay on the Grafana Alarm Dashboard.
        /// </summary>
        [StringLength(10)]
        public string DisplayData { get; set; }
    }

    /// <summary>
    /// Represents a Grafana Alarm Panel Block
    /// </summary>
    public class AlarmDeviceStateView
    {

        /// <summary>
        /// Unique ID.
        /// </summary>
        [PrimaryKey(true)]
        public int ID { get; set; }

        /// <summary>
        /// name of the Device.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the Device State.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Color of the Device State.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Additional data to be displayed.
        /// </summary>
        public string DisplayData { get; set; }
    }
}

