using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;
using TareasMVC.ViewModel;

namespace TareasMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ApplicationDbContext context;

        public UsersController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(Register model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var usuario = new IdentityUser() { Email = model.Email, UserName = model.Email };

            var result = await userManager.CreateAsync(usuario, password: model.Password);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }
        [AllowAnonymous]
        public IActionResult Login(string msj = null)
        {
            if (msj != null)
            {
                ViewData["msj"] = msj;
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                return View(model);
            }
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario y/o contrasenia incorrecta");
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult ExternalLogin(string provider, string returnUrl = null)
        {
            var urlRedirection = Url.Action("ExternalUserRegister", values: new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, urlRedirection);
            return new ChallengeResult(provider, properties);
        }
        [AllowAnonymous]
        public async Task<IActionResult> ExternalUserRegister(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var msj = "";
            
            if(remoteError != null)
            {
                msj = $"Error del proveedor externo: {remoteError}";
                return RedirectToAction("Login", routeValues: new { msj });
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                msj = "Error cargando la data de login externo";
                return RedirectToAction("Login", routeValues: new { msj });
            }
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            string email = "";
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                email = info.Principal.FindFirstValue(ClaimTypes.Email);
            }
            else
            {
                msj = "Error leyendo el email del usuario del proeedor";
                return RedirectToAction("Login", routeValues: new { msj });
            }

            var user = new IdentityUser { Email = email, UserName = email };
            var createUserResult = await userManager.CreateAsync(user);
            if (!createUserResult.Succeeded)
            {
                msj = createUserResult.Errors.First().Description;
                return RedirectToAction("Login", routeValues: new { msj });
            }
            var addLoginResult = await userManager.AddLoginAsync(user, info);

            if (addLoginResult.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            msj = "Ha ocurrido un error inesperado";
            return RedirectToAction("Login", routeValues: new { msj });
        }
        [HttpGet]
        [Authorize(Roles = Services.Constants.RolAdmin)]
        public async Task<IActionResult> List(string msj = null)
        {
            var users = await context.Users.Select(u => new User
            {
                Email = u.Email
            }).ToListAsync();

            var model = new UserList();
            model.Users = users;
            model.Msj = msj;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = Services.Constants.RolAdmin)]
        public async Task<IActionResult> MakeAdmin(string email)
        {
            var user = await context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            await userManager.AddToRoleAsync(user, Services.Constants.RolAdmin);
            return RedirectToAction("List", routeValues: new { msj = "Rol asignado correctamente a " + email});
        }

        [HttpPost]
        [Authorize(Roles = Services.Constants.RolAdmin)]
        public async Task<IActionResult> RemoveAdmin(string email)
        {
            var user = await context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            await userManager.RemoveFromRoleAsync(user, Services.Constants.RolAdmin);
            return RedirectToAction("List", routeValues: new { msj = "Rol removido correctamente a " + email });
        }
    }
}
