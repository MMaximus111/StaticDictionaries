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
    }
}