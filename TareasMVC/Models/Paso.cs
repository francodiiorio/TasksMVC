namespace TareasMVC.Models
{
    public class Step
    {
        public Guid Id { get; set; }
        public int TaskId { get; set; }
        public Tarea Task { get; set; }
        public string Description { get; set; }
        public bool Complete { get; set; }
        public int Order { get; set; }
    }
}
