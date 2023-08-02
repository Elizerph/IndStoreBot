using ElizerBot.Adapter;

namespace IndStoreBot
{
    internal class UpdateHandler : IBotAdapterUpdateHandler
    {
        public async Task HandleButtonPress(IBotAdapter bot, PostedMessageAdapter message, UserAdapter user, string buttonData)
        {

        }

        public async Task HandleCommand(IBotAdapter bot, ChatAdapter sourceChat, UserAdapter sourceUser, string command)
        {

        }

        public async Task HandleIncomingMessage(IBotAdapter bot, PostedMessageAdapter message)
        {
            var newMessage = new NewMessageAdapter(message.Chat)
            {
                Text = $"Echo: {new string(message.Text.Take(1500).ToArray())}"
            };
            await bot.SendMessage(newMessage);
        }
    }
}
