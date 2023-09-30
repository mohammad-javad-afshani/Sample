using Application.Data;
using Domain.Customers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Customers.Delete
{
    internal sealed class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommnad>
    {
        private readonly ICustomerRepository _customerrepository;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
        {
            _customerrepository = repository;
            _unitOfWork = unitOfWork;
        }
    
        public async Task Handle(DeleteCustomerCommnad request, CancellationToken cancellationToken)
        {
            var customer = await _customerrepository.FindByIdAsync(request.customerId);
            if (customer is null)
            {
                throw new CustomerNotFoundExeption(request.customerId); 
            }

            _customerrepository.Delete(customer);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

        }
    }
}
