using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TdLib;
using static TdLib.TdApi;
using static TdLib.TdApi.InputMessageContent;
using static TdLib.TdApi.MessageSender;
using static TdLib.TdApi.Update;

namespace Telegram.Automation;

public class TelegramConnector(IOptions<TelegramConnectorOptions> settings, ILogger<ITelegramConnector> logger) : IDisposable, ITelegramConnector
{
    private static SemaphoreSlim locker = new SemaphoreSlim(1, 1);
    private readonly TelegramConnectorOptions settings = settings.Value;
    private TdClient client;
    private Queue<UpdateChatLastMessage> Messages { get; set; } = new();
    private List<MessageLog> messagesLog = new();

    public void Dispose()
    {
        client?.Dispose();
        client = null;
    }

    public async Task<AuthenticationResult> Start()
    {

        using var ct1 = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        if (client != null) return await Authenticate(ct1.Token);

        await locker.WaitAsync();
        try
        {
            using var ct2 = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            if (client != null) return await Authenticate(ct2.Token);

            client = new TdClient();

            await SetLoggingLevel();

            client.UpdateReceived += Client_UpdateReceived;

            await client.SetTdlibParametersAsync(
                useSecretChats: true,
                useFileDatabase: true,
                databaseDirectory: settings.DatabaseLocation,
                apiId: settings.ApiID,
                apiHash: settings.ApiHash,
                systemLanguageCode: "en-US",
                deviceModel: "Desktop",
                applicationVersion: "0.1.0.0");
            using var ct3 = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            return await Authenticate(ct3.Token);

        }
        catch (Exception e)
        {
            throw;
        }
        finally
        {

            locker.Release();
        }
    }

    private async Task SetLoggingLevel()
    {
        client.Bindings.SetLogVerbosityLevel(0);

        await TdApi.SetLogTagVerbosityLevelAsync(client, "actor", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "binlog", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "connections", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "notifications", 0);
        await TdApi.SetLogTagVerbosityLevelAsync(client, "proxy", 0);
    }

    private async Task<Chat> GetChat(TdClient client, long automationChatId, string automationChatName)
    {
        var chat = await client.GetChatAsync(automationChatId);
        if (chat.Title != automationChatName)
            throw new Exception("Chat name doesn't match!");
        return chat;
    }

    private async Task<Chat?> GetChatByTitle(string title)
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

    public async Task<AuthenticationResult> IsAuthenticated()
    {
        var status = await client.GetAuthorizationStateAsync();
        if (status.DataType == "authorizationStateReady")
        {
            return AuthenticationResult.Authenticated;
        }
        return AuthenticationResult.Unauthorized;
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
        => logger.LogInformation($"Authentication Status: {status.AuthorizationState.Extra}");

    private void Handle_UpdateChatLastMessage(UpdateChatLastMessage lastMessageUpdate)
        => Messages.Enqueue(lastMessageUpdate);

    private void Client_UpdateReceived(object? sender, Update e)
    {
        if (e is UpdateAuthorizationState status) Handle_UpdateAuthorizationState(status);
        if (e is UpdateChatLastMessage lastMessageUpdate) Handle_UpdateChatLastMessage(lastMessageUpdate);
    }

    public async Task<string> SendMessage(
        string message,
        Func<string, bool> messagePredicate,
        bool multipleMessagesExpected,
        CancellationToken? token = null)
    {
        token ??= CancellationToken.None;
        logger.LogDebug("Sending message: {0}", message);

        var chat = await GetChatByTitle(settings.AutomationChatName);

        //var chat = await GetChatByTitle(settings.AutomationChatName);
        //settings.AutomationChatId = chat.Id;

        logger.LogDebug("Sending message: {0}", message);
        Messages.Clear();

        var sentMessage = await client.SendMessageAsync(chat.Id, inputMessageContent: new InputMessageText() { Text = new FormattedText() { Text = message } });

        var senderId = ((MessageSenderUser)sentMessage.SenderId).UserId;

        var tsc = CancellationTokenSource.CreateLinkedTokenSource(token ?? CancellationToken.None, new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);

        return await GetResponse(senderId, sentMessage.ChatId, messagePredicate, multipleMessagesExpected, tsc.Token);
    }

    private async Task<string> GetResponse(
        long senderId,
        long chatId,
        Func<string, bool> messagePredicate,
        bool multipleMessagesExpected,
        CancellationToken token)
    {
        try
        {
            if (multipleMessagesExpected)
                return await GetMultiMessageResponse(senderId, chatId, messagePredicate, token);
            return await GetSingleMessageResponse(senderId, chatId, messagePredicate, token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Parsing response failed.");
            return "";
        }
    }

    private async Task<string> GetSingleMessageResponse(long senderId, long chatId, Func<string, bool> messagePredicate, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            while (Messages.TryDequeue(out var update) && !token.IsCancellationRequested)
            {
                if (!IsMessageValid(senderId, chatId, update)) continue;

                if (update?.LastMessage?.Content is TdApi.MessageContent.MessageText message &&
                    message?.Text?.Text is string text &&
                    messagePredicate(text))
                    return text;
            }
            await Task.Delay(128, token);
        }
        return "";
    }

    private async Task<string> GetMultiMessageResponse(long senderId, long chatId, Func<string, bool> messagePredicate, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1_000, token); // wait to receive multiple message 
            var responses = new List<string>();

            while (Messages.TryDequeue(out var update) && !token.IsCancellationRequested)
            {
                if (!IsMessageValid(senderId, chatId, update)) continue;
                logger.LogInformation("Processing message (to be processed {messages})", Messages.Count);

                if (update?.LastMessage?.Content is TdApi.MessageContent.MessageText message && message?.Text?.Text is string)
                {
                    if (messagePredicate(message.Text.Text))
                    {
                        responses.Add(message.Text.Text);
                    }
                    else if (responses.Count > 0)
                        return string.Concat(responses);
                }
            }
            
            if (responses.Count > 0)
            {
                logger.LogInformation("No more messages, returned {0} responses", responses.Count);
                return string.Concat(responses);
            }
        }
        return "";
    }

    private bool IsMessageValid(long senderId, long chatId, UpdateChatLastMessage? update)
    {
        if (update is null) return false;
        if (update.ChatId != chatId) return false;
        if ((update.LastMessage.SenderId as MessageSenderUser)?.UserId == senderId) return false;
        return true;
    }

    public IEnumerable<MessageLog> GetLog() => messagesLog;
}