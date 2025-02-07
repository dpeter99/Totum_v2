namespace Microsoft.Extensions.Hosting;

public class BrandedTypeAttribute(string brand) : Attribute
{
    public string Brand { get => brand; }
}