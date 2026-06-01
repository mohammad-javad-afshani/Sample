using Application.Customers.Creat;
using Application.Customers.Delete;
using Application.Customers.Get;
using Application.Customers.Update;
using Domain.Customers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        [HttpPost("Creat")]
        public async Task<IActionResult> CreatCustomer(CreatCustomerCommand command, ISender sender)
        {
            await sender.Send(command);

            return Ok();

        }

        [HttpGet("Get")]
        public async Task<ActionResult> GetCustomer(Guid id, ISender sender)
        {
            return Ok(await sender.Send(new GetCustomerQuery(new CustomerId(id))));
        }
        [HttpPut("Put")]
        public async Task<ActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request, ISender sender)
        {
            var command = new UpdateCustomerCommand(
                   new CustomerId(id),
                   request.Firstname,
                   request.Lastname,
                   request.DateOfBirth,
                   request.Email,
                   request.PhoneNumber,
                   request.BankAccountNumber,
                   request.Addresses);

            await sender.Send(command);

            return Ok();
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteCustomer(Guid id, ISender sender)
        {
            await sender.Send(new DeleteCustomerCommnad(new CustomerId(id)));

            return Ok();
        }
    }
}
