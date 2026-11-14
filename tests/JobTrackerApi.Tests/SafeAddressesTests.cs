using System.Net;

namespace JobTrackerApi.Tests;

// Unit tests for the private-address block list
public class SafeAddressesTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("10.1.2.3")]
    [InlineData("172.20.0.5")]
    [InlineData("192.168.1.1")]
    [InlineData("169.254.169.254")] // cloud metadata
    [InlineData("100.64.0.1")]
    [InlineData("0.0.0.0")]
    [InlineData("::1")]
    [InlineData("fd00::1")]
    [InlineData("fe80::1")]
    [InlineData("::ffff:127.0.0.1")]
    public void IsBlocked_PrivateAddresses_AreBlocked(string address)
    {
        Assert.True(SafeAddresses.IsBlocked(IPAddress.Parse(address)));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("140.82.112.3")]
    [InlineData("2606:4700:4700::1111")]
    public void IsBlocked_PublicAddresses_AreAllowed(string address)
    {
        Assert.False(SafeAddresses.IsBlocked(IPAddress.Parse(address)));
    }
}
