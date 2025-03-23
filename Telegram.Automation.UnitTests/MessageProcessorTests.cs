﻿namespace Telegram.Automation.UnitTests;
public class MessageProcessorTests
{
    [Fact]
    public void IsStatusMessageTest()
    {
        var loader = new MockFileLoader(".");
        Assert.True(MessageProcessor.IsStatusMessage(loader.GetStatusResponseFull()));
        Assert.True(MessageProcessor.IsStatusMessage(loader.GetStatusResponsePart1()));
        Assert.True(MessageProcessor.IsStatusMessage(loader.GetStatusResponsePart2()));
    }

}
