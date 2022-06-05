using System;
using Xunit;

namespace GGroupp.Infra.Bot.Builder.ChatFlow.Step.Date.Tests;

partial class DateParserTest
{
    [Theory]
    [InlineData("01.11.2022", 2022, 11, 01)]
    [InlineData("01.11.2022  ", 2022, 11, 01)]
    [InlineData("   01.11.2022  ", 2022, 11, 01)]
    [InlineData(" 01.11.2022", 2022, 11, 01)]
    [InlineData("29.02.2024", 2024, 02, 29)]
    [InlineData("17.5.2021", 2021, 05, 17)]
    [InlineData("3.12.2019", 2019, 12, 03)]
    [InlineData("31.01.2031", 2031, 01, 31)]
    [InlineData("01.01.0001", 0001, 01, 01)]
    [InlineData("21.10.1997", 1997, 10, 21)]
    [InlineData("03.01.122", 2122, 01, 03)]
    [InlineData("03.01.0122", 0122, 01, 03)]
    [InlineData("05.07.22", 2022, 07, 05)]
    public void ParseOrFailure_TextIsCorrectDate_ExpectSuccess(string text, int year, int month, int day)
    {
        var actual = DateParser.ParseOrFailure(text);
        var expected = new DateOnly(year, month, day);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("15.01", 01, 15)]
    [InlineData("  31.05", 05, 31)]
    [InlineData("31.05", 05, 31)]
    [InlineData("31.05   ", 05, 31)]
    [InlineData(" 31.05  ", 05, 31)]
    [InlineData("31.12", 12, 31)]
    [InlineData("7.3", 03, 07)]
    [InlineData("1.03", 03, 01)]
    public void ParseOrFailure_TextIsOnlyDayAndMonthNumbers_ExpectSuccessWithCurrentYear(string text, int month, int day)
    {
        var actual = DateParser.ParseOrFailure(text);

        var now = DateTime.Now;
        var expected = new DateOnly(now.Year, month, day);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("17", 17)]
    [InlineData(" 17", 17)]
    [InlineData("17 ", 17)]
    [InlineData("  17   ", 17)]
    [InlineData("1", 01)]
    [InlineData("09", 09)]
    public void ParseOrFailure_TextIsOnlyDayNumber_ExpectSuccessWithCurrentYearAndMonth(string text, int day)
    {
        var actual = DateParser.ParseOrFailure(text);

        var now = DateTime.Now;
        var expected = new DateOnly(now.Year, now.Month, day);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("t31")]
    [InlineData("32")]
    [InlineData("101")]
    [InlineData("-17")]
    [InlineData("0")]
    [InlineData("01.13")]
    [InlineData("01.00")]
    [InlineData("02.01.1")]
    [InlineData("29.02.2021")]
    [InlineData("31.04.2022")]
    [InlineData("0.04.2022")]
    [InlineData("32.05.2022")]
    [InlineData("01.05.2022 m")]
    [InlineData("01..12")]
    [InlineData(".01.12")]
    public void ParseOrFailure_TextIsNotDate_ExpectFailure(string? text)
    {
        var actual = DateParser.ParseOrFailure(text);
        var expected = Result.Absent<DateOnly>();

        Assert.Equal(expected, actual);
    }
}