using Application.Addresses.Creat;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressController : ControllerBase
    {
       
        [HttpPost("Creat")]
        public async Task<IActionResult> CreatAddress(CreatAddressCommand command , ISender sender)
        {
            await sender.Send(command);
          
            return Ok();
        }
    }
}
