﻿using System.Xml;
using Argon;
using FluentAssertions;
using StaticDictionaries.Tests.StaticDictionaries;

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

    [Theory]
    [InlineData(1, Status.New)]
    [InlineData(4, Status.Ready)]
    [InlineData(5, Status.Completed)]
    [InlineData(8, Status.LastStatus)]
    public void GetById(int id, Status expectedStatus)
    {
        id.Should().Be((int)expectedStatus);
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
    public void IdPropertyMustOverrideDefaultIdOfMember()
    {
        EnumWithOverriddenIdProperty.Chair.Id().Should().Be(19);
        EnumWithOverriddenIdProperty.Table.Id().Should().Be(7);
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

    [Theory]
    [InlineData(Status.Finished)]
    [InlineData(Status.Packing)]
    [InlineData(Status.InProgress)]
    [InlineData(Status.LastStatus)]
    public void DefaultIdMethodMustWorkCorrect(Status status)
    {
        status.Id().Should().Be((int)status);
    }

    [Theory]
    [InlineData(Employee.John)]
    [InlineData(Employee.Maxim)]
    public void NameMethodMustWorkCorrect(Employee employee)
    {
        employee.Id().Should().Be((int)employee);
    }

    [Theory]
    [InlineData(666, OverriddenIdProperty.Member1)]
    [InlineData(555, OverriddenIdProperty.Member2)]
    [InlineData(111, OverriddenIdProperty.Member3)]
    [InlineData(9999999, OverriddenIdProperty.Member4)]
    public void GetByIdMustFindByOverridenIdIfItExists(int overriddenId, OverriddenIdProperty enumMember)
    {
        enumMember.Id().Should().Be(overriddenId);

        OverriddenIdPropertyExtensions.GetById(overriddenId).Should().Be(enumMember);
    }

    [Fact]
    public void EnumWithIntAndDoubleMustWotkCorrectAndConvertIntToDoubleType()
    {
        double member1Price = EnumWithIntAndDouble.Member1.Price();
        member1Price.Should().Be(67.89);

        double member2Price = EnumWithIntAndDouble.Member2.Price2();
        member2Price.Should().Be(100.123);

        EnumWithIntAndDouble.Member1.Id().Should().Be(1);
        EnumWithIntAndDouble.Member2.Id().Should().Be(2);
    }

    [Fact]
    public void JsonSerializationMustWorkCorrect()
    {
        string json = EnumWithSerializationSupportExtensions.Json();

        json.Should().Contain(@"""Nick"": ""CaesarTurbo""");
        json.Should().Contain(@"""Cost"": 12.23");
        json.Should().Contain(@"""Active"": true");
        json.Should().Contain("[");
        json.Should().Contain("]");
        json.Should().Contain("{");
        json.Should().Contain("}");
        json.Should().Contain(",");

        JArray jArray = JArray.Parse(json);

        jArray.Count.Should().Be(EnumWithSerializationSupportExtensions.Length);
    }

    [Fact]
    public void XmlSerializationMustWorkCorrect()
    {
        string xml = EnumWithSerializationSupportExtensions.Xml();

        xml.Should().Contain(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        xml.Should().Contain(@"<Nick>MisterMuscle</Nick>");
        xml.Should().Contain(@"<Id>3</Id>");
        xml.Should().Contain(@"<Age>22</Age>");
        xml.Should().Contain(@"<Active>True</Active>");
        xml.Should().Contain(@"<Lobster>");
        xml.Should().Contain(@"<Mister>");
        xml.Should().Contain(@"<root>");
        xml.Should().Contain(@"</root>");

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
    }
}