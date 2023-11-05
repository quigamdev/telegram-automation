﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Telegram.Automation;
public class MessageProcessor
{
    public static List<BotAccount> ProcessStatusMessage(string input)
    {
        if (!input.Contains("Active:")) return new List<BotAccount>();

        var lines = input.Split(new[] { '\r', '\n' }).Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Skip(1).ToList();
        var isOnline = false;
        var result = new List<BotAccount>();
        

        foreach (var line in lines)
        {
            isOnline = DetermineStatus(isOnline, line);
            var words = line.Split("[");
            
            if (words.Length != 2) continue;

            var account = new BotAccount();
            account.Name = words[0].Trim();
            account.AccountNumber = words[1].Trim().TrimEnd(']');
            account.IsOnline = isOnline;

            result.Add(account);
        }
        return result;
    }

    private static bool DetermineStatus(bool isOnline, string? line)
    {
        if (line.Contains("online", StringComparison.OrdinalIgnoreCase))
        {
            isOnline = true;
        }
        if (line.Contains("offline", StringComparison.OrdinalIgnoreCase))
        {
            isOnline = false;
        }

        return isOnline;
    }
}
