using FluentAssertions;
using StaticDictionaries.Tests.StaticDictionaries;
using Xunit;

namespace StaticDictionaries.Tests;

public class MainTests
{
    [Fact]
    public void AccessProperty()
    {
        int johnAge = Employee.John.Age();
        int maximAge = Employee.Maxim.Age();
        bool maximActive = Employee.Maxim.Active();
        bool johnActive = Employee.John.Active();
        string maximName = Employee.Maxim.Name();
        string johnName = Employee.John.Name();
        char completedSymbol = Status.Completed.Symbol();

        johnAge.Should().Be(23);
        maximAge.Should().Be(18);
        maximActive.Should().Be(true);
        johnActive.Should().Be(false);
        maximName.Should().Be("Максим");
        johnName.Should().Be("Джон");
        completedSymbol.Should().Be('"');
    }

    [Fact]
    public void All()
    {
        Employee[] employees = EmployeeExtensions.All();

        employees.Length.Should().Be(2);
        employees.First().Should().Be(Employee.Maxim);
        employees.Last().Should().Be(Employee.John);

        Status[] statuses = StatusExtensions.All();
        statuses.Length.Should().Be(8);
        statuses.First().Should().Be(Status.New);
    }

    [Fact]
    public void GetById()
    {
        Employee maximEmployee = EmployeeExtensions.GetById(1);
        maximEmployee.Should().Be(Employee.Maxim);

        Employee johnEmployee = EmployeeExtensions.GetById(2);
        johnEmployee.Should().Be(Employee.John);

        Status status = StatusExtensions.GetById(4);
        status.Name().Should().Be("Ready1");
    }

    [Fact]
    public void AccessGeneratedIdAndNameProperties()
    {
        int id = EnumWithoutSettedIdAndName.Value1.Id();
        string name = EnumWithoutSettedIdAndName.Value2.Name();

        id.Should().Be(1);
        name.Should().Be(nameof(EnumWithoutSettedIdAndName.Value2));
    }

    [Fact]
    public void StaticDictionaryWithoutArgumentsMustGenerateNameAndIdProperties()
    {
        string member1Name = StaticDictionaryWithoutArguments.Member1.Name();
        int member2Id = StaticDictionaryWithoutArguments.Member2.Id();

        member1Name.Should().Be(nameof(StaticDictionaryWithoutArguments.Member1));
        member2Id.Should().Be((int)StaticDictionaryWithoutArguments.Member2);
    }

    [Fact]
    public void NamePropertyMustOverrideDefaultNameOfMember()
    {
        Employee.Maxim.Name().Should().NotBeEquivalentTo(nameof(Employee.Maxim));
    }

    [Fact]
    public void MaxIdMustReturnMaxEnumMemberId()
    {
        const int maxId = EmployeeExtensions.MaxId;
        maxId.Should().Be(2);

        const int minId = EmployeeExtensions.MinId;
        minId.Should().Be(1);
    }

    [Fact]
    public void EnumWithDifferentDataTypesAndDuplicatedValueAttributeMustWorkWell()
    {
        StrangeEnum.duck.Id().Should().Be((int)StrangeEnum.duck);

        StrangeEnumExtensions.Length.Should().Be(4);
        StrangeEnum.Tiger.priCE().Should().Be(10.00);
        StrangeEnum.Elephant.Active().Should().Be(true);

        StrangeEnumExtensions.MinId.Should().Be(1);
        StrangeEnumExtensions.MaxId.Should().Be(1000000);

        StrangeEnumExtensions.All().Length.Should().Be(4);
        StrangeEnumExtensions.All().First().Should().Be(StrangeEnum.duck);
    }
}