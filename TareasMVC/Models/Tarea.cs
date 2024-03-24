using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Models
{
    public class Tarea
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public DateTime Date { get; set; }
        public string UserCreateId { get; set; }
        public IdentityUser UserCreate { get; set; }
        public List<Step> Steps { get; set; }
        public List<FileAttachment> FileAttachments { get; set; }
    }
}