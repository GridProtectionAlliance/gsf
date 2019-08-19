using GSF.ComponentModel.DataAnnotations;
using GSF.Data.Model;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DeviceStatAdapters.Model
{
    [PrimaryLabel("Acronym")]
    public class Device
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public Guid UniqueID { get; set; }

        [Required]
        [StringLength(200)]
        [AcronymValidation]
        public string Acronym { get; set; }

        [StringLength(200)]
        [AcronymValidation]
        public string Name { get; set; }

        [StringLength(200)]
        public string ParentAcronym { get; set; }

        public string Protocol { get; set; }

        public decimal? Longitude { get; set; }

        public decimal? Latitude { get; set; }

        [DefaultValue(30)]
        public int FramesPerSecond { get; set; }
    }
}