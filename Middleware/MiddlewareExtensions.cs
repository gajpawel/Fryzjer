using static Fryzjer.Middleware.PermissionController;

namespace Fryzjer.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UsePermission(this IApplicationBuilder _builder, UserTypes[] _allowedUsers)
        {
            return _builder.UseMiddleware<PermissionController>(_allowedUsers);
        }
    }
}
