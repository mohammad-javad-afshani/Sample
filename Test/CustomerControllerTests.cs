using Application.Customers.Creat;
using Application.Customers.Delete;
using Application.Customers.Get;
using Application.Customers.Update;
using Domain.Addresses;
using Domain.Customers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.Controllers;
using Xunit;

public class CustomerControllerTests
{

    CreatCustomerCommand command = new CreatCustomerCommand(
     "John",                  // Firstname
     "Doe",                   // Lastname
     new DateTime(1990, 1, 1), // DateOfBirth (example date, use your own)
     "john.doe@example.com",  // Email
     "123-456-7890",          // PhoneNumber
     "1234567890123456",      // BankAccountNumber
     new List<Address>
     {
        new Address("123 Main St", "Example City", "Example State", "12345")
     });

    UpdateCustomerRequest Req = new UpdateCustomerRequest(
        "John",                  // Firstname
        "Doe",                   // Lastname
        new DateTime(1990, 1, 1), // DateOfBirth (example date, use your own)
        "john.doe@example.com",  // Email
        "123-456-7890",          // PhoneNumber
        "1234567890123456",      // BankAccountNumber
        new List<Address>
        {
            new Address("123 Main St", "Example City", "Example State", "12345")
        });



    [Fact]
    public async Task CreatCustomer_ReturnsOkResult()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var controller = new CustomerController();

        // Act
        var result = await controller.CreatCustomer(command, senderMock.Object);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetCustomer_ReturnsOkResult()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        //senderMock
        //    .Setup(sender => sender.Send(It.IsAny<GetCustomerQuery>(new CustomerId(System.Guid.NewGuid())), CancellationToken.None))
        //    .ReturnsAsync(new Customer());

        var controller = new CustomerController();

        // Act
        var result = await controller.GetCustomer(Guid.NewGuid(), senderMock.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateCustomer_ReturnsOkResult()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        var controller = new CustomerController();

        // Act
        var result = await controller.UpdateCustomer(Guid.NewGuid(), Req, senderMock.Object);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteCustomer_ReturnsOkResult()
    {
        // Arrange
        var senderMock = new Mock<ISender>();
        //senderMock
        //    .Setup(sender => sender.Send(It.IsAny<DeleteCustomerCommnad>(new CustomerId(System.Guid.NewGuid())), CancellationToken.None))
        //    .ReturnsAsync(Unit.Value);

        var controller = new CustomerController();

        // Act
        var result = await controller.DeleteCustomer(Guid.NewGuid(), senderMock.Object);

        // Assert
        Assert.IsType<OkResult>(result);
    }
}
