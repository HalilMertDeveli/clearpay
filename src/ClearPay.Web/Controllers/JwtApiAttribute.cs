using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace ClearPay.Web.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class JwtApiAttribute : AuthorizeAttribute
{
    public JwtApiAttribute()
        : this(roles: null)
    {
    }

    public JwtApiAttribute(string? roles)
    {
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
        if (!string.IsNullOrWhiteSpace(roles))
            Roles = roles;
    }
}
