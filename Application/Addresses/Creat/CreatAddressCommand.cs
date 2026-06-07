using MediatR;

namespace Application.Addresses.Creat
{
    public record CreatAddressCommand(
        string CountryTitle,
        string CityTitle, 
        string PostalCode, 
        string Street) : IRequest;


    public record CreatAddressCommandd(
        string CountryTitle,
        string CityTitle, 
        string PostalCode, 
        string Street) : IRequest;

}
