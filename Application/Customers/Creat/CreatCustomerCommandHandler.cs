using Application.Data;
using Domain.Addresses;
using Domain.Customers;
using MediatR;

namespace Application.Customers.Creat
{
    internal sealed class CreatCustomerCommandHandler : IRequestHandler<CreatCustomerCommand>
    {
        private readonly CreatCustomerValidator _validator;  
        private readonly ICustomerRepository _customerrRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CreatCustomerCommandHandler(ICustomerRepository customerrRepository, IUnitOfWork unitOfWork, CreatCustomerValidator validator)
        {
            _customerrRepository = customerrRepository;
            _unitOfWork = unitOfWork;
            _validator = validator; 
        }
        public async Task Handle(CreatCustomerCommand request, CancellationToken cancellationToken)
        {

            var customer = new Customer(             
                request.Firstname,
                request.Lastname,
                request.DateOfBirth,
                request.Email,
                request.PhoneNumber,
                request.BankAccountNumber,
                new List<Address>(request.Addresses)
                ); 
           
            var result = _validator.Validate(customer);

            if (!result.IsValid)
            {
                throw new CustomerNotValidExeption();
            }

            _customerrRepository.Add(customer);

            await _unitOfWork.SaveChangesAsync(cancellationToken);   
        }
    }
}
