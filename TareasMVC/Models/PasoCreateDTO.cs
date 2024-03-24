using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Models
{
    public class PasoCreateDTO
    {
        [Required]
        public string Description { get; set; }
        public bool Complete { get; set; }
    }
}
