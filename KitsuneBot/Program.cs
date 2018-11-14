using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KitsuneBot.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace KitsuneBot
{
    internal class Program
    {
        private static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;

        private const int _delay = -1;

        private CommandService commands;

#pragma warning disable 1998

        public async Task MainAsync()
#pragma warning restore 1998
        {
            _client = new DiscordSocketClient();
            commands = new CommandService();

            var services = new ServiceCollection().BuildServiceProvider();

            await InstallCommands();

            _client.Log += Log;

            string Token = Resources.DiscordToken;

            // Retrive and store BotToken
            await _client.LoginAsync(TokenType.Bot, Token);

            // Start as Async
            await _client.StartAsync();

            //Block Until Closed
            await Task.Delay(_delay);
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new CommandContext(_client, message);
            // Execute the command. (result does not indicate a return value,
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos/*, service*/);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        /*
         * Creation of logger to write errors console following Discord.Net Doc Gettting Started guide replace with better logger later!
         */

        private Task Log(LogMessage _message)
        {
            Console.WriteLine(_message.ToString());
            return Task.CompletedTask;
        }
    }
}