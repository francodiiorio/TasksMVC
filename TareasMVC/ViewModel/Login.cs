﻿using System.ComponentModel.DataAnnotations;

namespace TareasMVC.ViewModel
{
    public class Login
    {
        [Required(ErrorMessage = "Error.Requerido")]
        [EmailAddress(ErrorMessage = "Error.Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Error.Requerido")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Recuerdame")]
        public bool RememberMe { get; set; }
    }
}
