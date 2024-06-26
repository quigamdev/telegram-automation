﻿namespace Telegram.Automation;
public class CommandBuilder
{
    private const string AccountCommand = "/";

    public static string StopAccount(string accountName)
    {
        return $"{AccountCommand}stop {accountName}";
    }
    public static string StartAccount(string accountName)
    {
        return $"{AccountCommand}start {accountName}";
    }
    public static string GetAccountsStatus()
    {
        return $"{AccountCommand}status";
    }
}
