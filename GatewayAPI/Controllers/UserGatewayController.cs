using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GatewayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserGatewayController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserGatewayController(IHttpClientFactory factory)
        {
            _httpClientFactory = factory;
        }

        [HttpGet("{email}/notifications")]
        public async Task<IActionResult> GetNotifications(string email)
        {
            var response = await _httpClientFactory.CreateClient("Customer").GetAsync($"/api/User/{email}/notifications");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement loginData)
        {
            var client = _httpClientFactory.CreateClient("Customer");
            var response = await client.PostAsync("/api/User/login", new StringContent(
                loginData.GetRawText(),
                System.Text.Encoding.UTF8,
                "application/json"
            ));

            var content = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, content);
        }

    }
}
