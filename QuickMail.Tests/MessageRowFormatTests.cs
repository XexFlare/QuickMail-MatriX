using System;
using System.Linq;
using QuickMail.Models;
using QuickMail.Views;
using Xunit;

namespace QuickMail.Tests;

/// <summary>MailBox MatriX message rows (step c): the friendly time and the per-sender circle colour.</summary>
public class MessageRowFormatTests
{
    // Tuesday 31 October 2017, as in the reference image.
    private static readonly DateTime Now = new(2017, 10, 31, 9, 0, 0);

    [Theory]
    [InlineData(2017, 10, 31, 8, 48, "8:48 AM")]       // today: time
    [InlineData(2017, 10, 31, 0, 5, "12:05 AM")]
    [InlineData(2017, 10, 30, 14, 2, "Mon 2:02 PM")]   // yesterday: day and time
    [InlineData(2017, 10, 27, 19, 30, "Fri 10/27")]    // earlier this year: day and date
    [InlineData(2017, 1, 2, 10, 0, "Mon 1/2")]
    [InlineData(2016, 12, 30, 10, 0, "12/30/2016")]    // an earlier year: full date
    public void FriendlyDate_FollowsWindowsMail(int y, int mo, int d, int h, int mi, string expected)
        => Assert.Equal(expected, MailMessageSummary.FormatFriendlyDate(new DateTime(y, mo, d, h, mi, 0), Now));

    [Fact]
    public void FriendlyDate_ForAMessageDatedInTheFuture_IsItsTime()
        => Assert.Equal("10:00 AM", MailMessageSummary.FormatFriendlyDate(Now.AddHours(1), Now));

    [Fact]
    public void SenderColour_IsStable_AndIgnoresCaseAndSpacing()
    {
        Assert.Equal(AvatarColorIndexConverter.IndexFor("Kai Moreno"), AvatarColorIndexConverter.IndexFor(" kai moreno "));
        Assert.InRange(AvatarColorIndexConverter.IndexFor("Kai Moreno"), 0, 5);
        Assert.Equal(0, AvatarColorIndexConverter.IndexFor(null));
    }

    [Fact]
    public void SenderColours_AreSpreadAcrossThePalette()
    {
        var senders = new[] { "CodeHarbor", "VaporPlay", "Orbitflix", "Circuit Depot", "Raidline", "Nebula Reads",
                              "Byte Weekly", "Kai Moreno", "Starport Comics", "Rin Takeda", "Meridian Bank", "NimbusCloud" };
        Assert.True(senders.Select(AvatarColorIndexConverter.IndexFor).Distinct().Count() >= 4);
    }
}
