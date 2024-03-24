using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace TareasMVC.Models
{
    public class FileAttachment
    {
        public Guid Id { get; set; }
        public int TaskId { get; set; }
        public Tarea Task { get; set; }
        [Unicode]
        public string Url { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public DateTime Date { get; set; }

    }
}
