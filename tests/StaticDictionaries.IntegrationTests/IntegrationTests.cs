using FluentAssertions;
using StaticDictionaries.IntegrationTests.StaticDictionaries;

namespace StaticDictionaries.IntegrationTests;

public class IntegrationTests
{
    [Fact]
    public void SimpleTestWithUserEnum()
    {
        string bradRealName = User.Brad.RealName();

        bradRealName.Should().Be("Sally");

        string jackName = User.Jack.Name();

        jackName.Should().Be("Jack");
    }

    [Fact]
    public void SimpleTestWithBrandEnum()
    {
        Brands.Apple.Id().Should().Be(4);
        Brands.Samsung.Id().Should().Be(1);

        BrandsExtensions.GetById(1).Should().Be(Brands.Samsung);
        BrandsExtensions.GetById(4).Should().Be(Brands.Apple);

        BrandsExtensions.All().Length.Should().Be(2);

        Brands.Apple.Price().Should().Be(999_999);
        Brands.Samsung.Price().Should().Be(777_777);

        Brands.Apple.AverageProductPrice().Should().Be(900.45);
        Brands.Samsung.AverageProductPrice().Should().Be(340.45);

        Brands.Apple.Active().Should().BeTrue();
        Brands.Samsung.Active().Should().BeTrue();
    }
}