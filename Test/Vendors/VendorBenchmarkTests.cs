using Application.Vendors.Create;

namespace Test.Vendors;

public class VendorCreateValidationTests
{
    [Fact]
    public void CreateVendorValidator_rejects_invalid_email()
    {
        var validator = new CreateVendorValidator();
        var result = validator.Validate(new CreateVendorCommand("Acme", "not-an-email", "TAX-1"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateVendorCommand.Email));
    }

    [Fact]
    public void CreateVendorValidator_accepts_valid_input()
    {
        var validator = new CreateVendorValidator();
        var result = validator.Validate(new CreateVendorCommand("Acme", "ops@acme.com", "TAX-1"));
        Assert.True(result.IsValid);
    }
}

public class VendorPlaceholderTests
{
    [Fact]
    public void Placeholder_always_passes()
    {
        Assert.True(true);
    }
}
