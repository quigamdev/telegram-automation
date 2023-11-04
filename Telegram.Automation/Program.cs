using Microsoft.Extensions.Configuration;
using TdLib;
using Telegram.Automation;
using static TdLib.TdApi;
using static TdLib.TdApi.InputMessageContent;
using static TdLib.TdApi.MessageSender;
using static TdLib.TdApi.Update;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var profile = args[0];

        var config = new ConfigurationBuilder()
            .AddJsonFile($"appSettings.{profile}.json")
            .Build();
        var appSettings = new AppSettings
        {
            ApiHash = config["ApiHash"],
            ApiID = int.Parse(config["ApiId"]),
            DatabaseLocation = config["DatabaseLocation"],
            PhoneNumber = config["PhoneNumber"],
            AutomationChatId = long.Parse(config["AutomationChatId"]),
            AutomationChatName = config["AutomationChatName"],
            SenderId = 0
        };

        using var client = new TdClient();

        client.UpdateReceived += Client_UpdateReceived;
        void Client_UpdateReceived(object? sender, Update e)
        {
            if (e is UpdateAuthorizationState status) Handle_UpdateAuthorizationState(status);
            if (e is UpdateChatLastMessage lastMessageUpdate) Handle_UpdateChatLastMessage(lastMessageUpdate);
        }

        void Handle_UpdateChatLastMessage(UpdateChatLastMessage lastMessageUpdate)
        {
            if (lastMessageUpdate.ChatId != appSettings.AutomationChatId) return;
            var user = lastMessageUpdate.LastMessage.SenderId as MessageSenderUser;
            if (user.UserId == appSettings.SenderId) return;

            var content = lastMessageUpdate.LastMessage.Content as MessageContent.MessageText;
            
            var text = content.Text.Text;

            var accounts = MessageProcessor.ProcessStatusMessage(text);
        }

        void Handle_UpdateAuthorizationState(UpdateAuthorizationState status)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Authentication Status: {status.AuthorizationState.Extra}");
            Console.ResetColor();
        }


        await client.SetTdlibParametersAsync(
            useSecretChats: true,
            useFileDatabase: true,
            databaseDirectory: appSettings.DatabaseLocation,
            apiId: appSettings.ApiID,
            apiHash: appSettings.ApiHash,
            systemLanguageCode: "en-US",
            deviceModel: "Desktop",
            applicationVersion: "0.1.0.0");



        //await client.SetAuthenticationPhoneNumberAsync(appSettings.PhoneNumber);
        //var code = "32256";
        //await client.CheckAuthenticationCodeAsync(code);

        await WaitUntilAuthorized(client);

        //var chat = await GetChat(client, appSettings.AutomationChatId, appSettings.AutomationChatName);
        var chat = await GetChatByTitle(client, appSettings.AutomationChatName);

        appSettings.AutomationChatId = chat.Id;

        async Task<Chat> GetChat(TdClient client, long automationChatId, string automationChatName)
        {
            var chat = await client.GetChatAsync(automationChatId);
            if (chat.Title != automationChatName)
                throw new Exception("Chat name doesn't match!");
            return chat;
        }

        var command = CommandBuilder.GetAccountsStatus();

        var message = await client.SendMessageAsync(chat.Id, inputMessageContent: new InputMessageText() { Text = new FormattedText() { Text = command } });
        appSettings.SenderId = ((MessageSenderUser)message.SenderId).UserId;


        static async Task<Chat> GetChatByTitle(TdClient client, string title)
        {
            var chats = await client.GetChatsAsync(limit: 200);

            foreach (var id in chats.ChatIds)
            {
                var chat = await client.GetChatAsync(id);
                if (chat.Title == title)
                    return chat;
            }
            return null;
        }

        static async Task WaitUntilAuthorized(TdClient client)
        {
            var authenticated = false;
            while (!authenticated)
            {
                var status = await client.GetAuthorizationStateAsync();
                if (status.DataType == "authorizationStateReady")
                {
                    authenticated = true;
                    Console.WriteLine("Authenticated");
                }
                else
                    await Task.Delay(1000);
            }
        }
    }
}