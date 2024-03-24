using System.ComponentModel.DataAnnotations;

namespace TareasMVC.ViewModel
{
    public class Register
    {
        [Required(ErrorMessage = "Error.Requerido")]
        [EmailAddress(ErrorMessage ="El correo electronico es invalido")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Error.Requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
