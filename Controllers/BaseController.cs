using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolSuppliesInventory.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public abstract class BaseController : Controller
    {
    }
}
