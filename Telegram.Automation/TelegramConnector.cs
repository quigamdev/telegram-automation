using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Threading;
using TdLib;
using static TdLib.TdApi;
using static TdLib.TdApi.InputMessageContent;
using static TdLib.TdApi.MessageSender;
using static TdLib.TdApi.Update;

namespace Telegram.Automation;

public class TelegramConnector : IDisposable, ITelegramConnector
{
    private static SemaphoreSlim locker = new SemaphoreSlim(1, 1);
    private readonly TelegramConnectorOptions settings;
    private readonly ILogger<ITelegramConnector> logger;
    private TdClient client;
    private Queue<UpdateChatLastMessage> Messages { get; set; } = new();
    private List<MessageLog> messagesLog = new();

    public TelegramConnector(IOptions<TelegramConnectorOptions> settings, ILogger<ITelegramConnector> logger)
    {
        this.settings = settings.Value;
        this.logger = logger;
    }

    public void Dispose()
    {
        client?.Dispose();
        client = null;
    }

    public async Task<AuthenticationResult> Start()
    {

        if (client != null) return await Authenticate(CancellationToken.None);
        await locker.WaitAsync();
        if (client != null) return await Authenticate(CancellationToken.None);


        client = new TdClient();

        await TdApi.SetLogTagVerbosityLevelAsync(client, "actor", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "binlog", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "connections", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "notifications", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "proxy", 0);

        client.UpdateReceived += Client_UpdateReceived;

        await client.SetTdlibParametersAsync(
            useSecretChats: true,
            useFileDatabase: true,
            databaseDirectory: (string)settings.DatabaseLocation,
            apiId: (int)settings.ApiID,
            apiHash: (string)settings.ApiHash,
            systemLanguageCode: "en-US",
            deviceModel: "Desktop",
            applicationVersion: "0.1.0.0");

        return await Authenticate(CancellationToken.None);
    }

    private async Task<Chat> GetChat(TdClient client, long automationChatId, string automationChatName)
    {
        var chat = await client.GetChatAsync(automationChatId);
        if (chat.Title != automationChatName)
            throw new Exception("Chat name doesn't match!");
        return chat;
    }

    private async Task<Chat> GetChatByTitle(string title)
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

    private async Task<AuthenticationResult> Authenticate(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var status = await client.GetAuthorizationStateAsync();
            if (status.DataType == "authorizationStateWaitPhoneNumber")
            {
                await this.InitAuthentication();
            }
            if (status.DataType == "authorizationStateWaitCode")
            {
                return AuthenticationResult.WaitingForCode;
            }
            if (status.DataType == "authorizationStateReady")
            {
                return AuthenticationResult.Authenticated;
            }
            else
                await Task.Delay(1000, token);
        }
        return AuthenticationResult.Unauthorized;
    }

    public async Task InitAuthentication() => await client.SetAuthenticationPhoneNumberAsync(settings.PhoneNumber);
    public async Task CheckAuthCode(string code) => await client.CheckAuthenticationCodeAsync(code);

    private void Handle_UpdateAuthorizationState(UpdateAuthorizationState status)
    {
        logger.LogInformation($"Authentication Status: {status.AuthorizationState.Extra}");
    }
    private void Handle_UpdateChatLastMessage(UpdateChatLastMessage lastMessageUpdate)
    {
        Messages.Enqueue(lastMessageUpdate);
    }

    private void Client_UpdateReceived(object? sender, Update e)
    {
        if (e is UpdateAuthorizationState status) Handle_UpdateAuthorizationState(status);
        if (e is UpdateChatLastMessage lastMessageUpdate) Handle_UpdateChatLastMessage(lastMessageUpdate);
    }

    public async Task<string> SendMessage(string message, CancellationToken? token = null)
    {
        token ??= CancellationToken.None;

        var chat = await GetChatByTitle(settings.AutomationChatName);

        //var chat = await GetChatByTitle(settings.AutomationChatName);
        //settings.AutomationChatId = chat.Id;

        var sentMessage = await client.SendMessageAsync(chat.Id, inputMessageContent: new InputMessageText() { Text = new FormattedText() { Text = message } });

        var senderId = ((MessageSenderUser)sentMessage.SenderId).UserId;

        return await GetResponse(senderId, sentMessage.ChatId, (CancellationToken)token);
    }

    private async Task<string> GetResponse(long senderId, long chatId, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            while (Messages.TryDequeue(out var update))
            {
                if (update.ChatId != chatId) continue;
                if ((update.LastMessage.SenderId as MessageSenderUser)?.UserId == senderId) continue;
                return ((TdLib.TdApi.MessageContent.MessageText)update.LastMessage.Content).Text.Text;
            }
            await Task.Delay(128);
        }
        return null;
    }

    public IEnumerable<MessageLog> GetLog() => messagesLog;
}