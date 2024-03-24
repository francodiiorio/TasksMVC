using System.Security.Claims;

namespace TareasMVC.Services
{
    public interface IUsersService
    {
        string GetUserId();
    }

    public class UsersService : IUsersService
    {
        private HttpContext httpContext;
        public UsersService(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }
        public string GetUserId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User.Claims.Where(u => u.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
                return idClaim.Value;
            }
            else 
            {
                throw new Exception("Usuario no autenticado");
            }
        }
    }
}
