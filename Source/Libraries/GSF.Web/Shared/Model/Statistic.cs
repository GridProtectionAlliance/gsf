#pragma warning disable 1591

using System.ComponentModel.DataAnnotations;
using GSF.ComponentModel.DataAnnotations;
using GSF.Data.Model;

namespace GSF.Web.Shared.Model
{
    [PrimaryLabel("Name")]
    public class Statistic
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        [StringLength(20)]
        public string Source { get; set; }

        [Label("Signal Index")]
        public int SignalIndex { get; set; }

        [Label("Statistic Name")]
        [Required]
        [StringLength(200)]
        [Searchable]
        public string Name { get; set; }

        [Searchable]
        public string Description { get; set; }

        [Label("Assembly Name")]
        [Required]
        public string AssemblyName { get; set; }

        [Label("Type Name")]
        [Required]
        public string TypeName { get; set; }

        [Label("Method Name")]
        [StringLength(200)]
        [Required]
        public string MethodName { get; set; }

        public string Arguments { get; set; }

        public bool Enabled { get; set; }

        [Label(".NET Data Type (Fully Qualified)")]
        [StringLength(200)]
        public string DataType { get; set; }

        [Label("Display Format, e.g., '{0:N3} seconds'")]
        [StringLength(200)]
        public string DisplayFormat { get; set; }

        [Label("Represents Connected State")]
        public bool IsConnectedState { get; set; }

        [Label("Load Order")]
        public int LoadOrder { get; set; }
    }
}
