using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AlunosApi.Models
{
    public partial class TarefaDto
    {
       
        [Required]
        [StringLength(80, ErrorMessage = "")]
        public string Status { get; set; }


    }
}
