using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LocalizeApi.Models
{
    public partial class Tarefa
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(80, ErrorMessage = "")]
        public string Status { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "")]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

    }
}
