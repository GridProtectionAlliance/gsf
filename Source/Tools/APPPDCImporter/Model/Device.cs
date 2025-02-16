﻿// ReSharper disable CheckNamespace

#pragma warning disable 1591

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data.Model;

namespace APPPDCImporter.Model
{
    [PrimaryLabel("Acronym")]
    public class Device
    {
        public Guid NodeID { get; set; }

        [Label("Local Device ID")]
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int? ParentID { get; set; }

        [Label("Unique Device ID")]
        [DefaultValueExpression("Guid.NewGuid()")]
        public Guid UniqueID { get; set; }

        [Required]
        [StringLength(200)]
        [AcronymValidation]
        [Searchable]
        public string Acronym { get; set; }

        [NonRecordField]
        public string OldAcronym { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        [Label("Folder Name")]
        [StringLength(20)]
        public string OriginalSource { get; set; }

        [Label("Is Concentrator")]
        public bool IsConcentrator { get; set; }

        [Required]
        [Label("Company")]
        public int? CompanyID { get; set; }

        [Label("Historian")]
        public int? HistorianID { get; set; }

        [Label("Access ID")]
        public int AccessID { get; set; }

        [Label("Vendor Device")]
        public int? VendorDeviceID { get; set; }

        [Label("Protocol")]
        public int? ProtocolID { get; set; }

        public decimal? Longitude { get; set; }

        public decimal? Latitude { get; set; }

        [Label("Interconnection")]
        [InitialValueScript("1")]
        public int? InterconnectionID { get; set; }

        [Label("Connection String")]
        public string ConnectionString { get; set; }

        [StringLength(200)]
        public string TimeZone { get; set; }

        [Label("Frames Per Second")]
        [DefaultValue(30)]
        public int? FramesPerSecond { get; set; }

        public long TimeAdjustmentTicks { get; set; }

        [DefaultValue(5.0D)]
        public double DataLossInterval { get; set; }

        [DefaultValue(10)]
        public int AllowedParsingExceptions { get; set; }

        [DefaultValue(5.0D)]
        public double ParsingExceptionWindow { get; set; }

        [DefaultValue(5.0D)]
        public double DelayedConnectionInterval { get; set; }

        [DefaultValue(true)]
        public bool AllowUseOfCachedConfiguration { get; set; }

        [DefaultValue(true)]
        public bool AutoStartDataParsingSequence { get; set; }

        public bool SkipDisableRealTimeData { get; set; }

        [DefaultValue(100000)]
        public int MeasurementReportingInterval { get; set; }

        [Label("Connect On Demand")]
        [DefaultValue(true)]
        public bool ConnectOnDemand { get; set; }

        [Label("Contacts")]
        public string ContactList { get; set; }

        public int? MeasuredLines { get; set; }

        public int LoadOrder { get; set; }

        public bool Enabled { get; set; }

        [DefaultValueExpression("DateTime.UtcNow")]
        public DateTime CreatedOn { get; set; }

        [Required]
        [StringLength(50)]
        [DefaultValueExpression("UserInfo.CurrentUserID")]
        public string CreatedBy { get; set; }

        [DefaultValueExpression("this.CreatedOn", EvaluationOrder = 1)]
        [UpdateValueExpression("DateTime.UtcNow")]
        public DateTime UpdatedOn { get; set; }

        [Required]
        [StringLength(50)]
        [DefaultValueExpression("this.CreatedBy", EvaluationOrder = 1)]
        [UpdateValueExpression("UserInfo.CurrentUserID")]
        public string UpdatedBy { get; set; }
    }
}