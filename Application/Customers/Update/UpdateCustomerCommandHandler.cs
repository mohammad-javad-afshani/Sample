using Application.Customers.Creat;
using Application.Data;
using Domain.Customers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Customers.Update
{
    internal sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand>
    {
        private readonly ICustomerRepository _customerrRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
        {
            _customerrRepository = repository;
            _unitOfWork = unitOfWork;
        }
        public async Task  Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerrRepository.FindByIdAsync(request.customerId);
            if (customer == null)
            {
                throw new CustomerNotFoundExeption(request.customerId);
            }
            customer.Update(request.Firstname, request.Lastname, request.DateOfBirth, request.Email, request.PhoneNumber, request.BankAccountNumber, request.Addresses);

            _customerrRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);   
        }
    }

}
