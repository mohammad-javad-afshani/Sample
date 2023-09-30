using Application.Data;
using Domain.Addresses;
using Domain.Customers;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Addresses.Creat
{
    internal sealed class CreatAddressCommandHandler : IRequestHandler<CreatAddressCommand>
    {
        private readonly CreatAddressValidator _validator;
        private readonly IAddressRepository _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatAddressCommandHandler(IAddressRepository addressRepository, IUnitOfWork unitOfWork, CreatAddressValidator validator)
        {
            _validator = validator; 
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }   

        public async Task Handle(CreatAddressCommand request, CancellationToken cancellationToken)
        {
            var address = new Address(
                request.CountryTitle,
                request.CityTitle,
                request.PostalCode,
                request.Street
                );

            var result = _validator.Validate(address);

            if (!result.IsValid)
            {
                throw new AddressNotValidExeption();
            }

            _addressRepository.Add(address);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

        }
    }
}
