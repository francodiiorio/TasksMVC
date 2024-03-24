using Microsoft.AspNetCore.Mvc.Rendering;

namespace TareasMVC.Services
{
    public class Constants
    {
        public const string RolAdmin = "admin";

        public static readonly SelectListItem[] UICultures = new SelectListItem[]
        {
            new SelectListItem{Value = "es", Text = "Español"},
            new SelectListItem{Value = "en", Text = "English"}
        };
    }
}
