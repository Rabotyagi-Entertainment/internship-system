using Internship_system.BLL.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Internship_system.Controllers.Extensions;

public static class ControllerExtensions {
    public static Guid GetUserId(this Controller controller) {
        if (controller.User.Identity == null || Guid.TryParse(controller.User.Identity.Name, out Guid userId) == false) {
            throw new UnauthorizedException("User is not authorized");
        }

        return userId;
    }
}