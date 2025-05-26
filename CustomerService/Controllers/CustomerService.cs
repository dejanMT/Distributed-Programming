using CustomerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationController(NotificationService service)
        {
            _service = service;
        }

        [HttpPost("discount")]
        public async Task<IActionResult> AddDiscount(string email)
        {
            var result = await _service.AddDiscountNotification(email);
            return result != null
                ? Ok(result)
                : Conflict("It seams the notification is already sent or the user not found.");
        }

    }
}
