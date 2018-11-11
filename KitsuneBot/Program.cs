using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KitsuneBot.CommandHandler;
using KitsuneBot.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace KitsuneBot
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

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

        // Create a module with no prefix
        public class InfoModule : ModuleBase<CommandContext>
        {
            // say hello world returns: hello world
            [Command("say"), Summary("Echos a Message")]
            public Task SayAsAsync([Remainder, Summary("The Text To Echo")] string echo) => ReplyAsync(echo);

            // ReplyAsync is a method on ModuleBase
        }

        // Create a module with the 'sample' prefix
        [Group("sample")]
        public class SampleModule : ModuleBase<CommandContext>
        {
            // sample square 20 returns: 400
            [Command("square")]
            [Summary("Squares a number.")]
            public async Task SquareAsync([Summary("The number to square.")] int num)
            {
                // We can also access the channel from the Command Context.
                await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
            }

            // sample userinfo returns: foxbot#0282
            // sample userinfo @Khionu returns: Khionu#8708
            // sample userinfo Khionu#8708 returns: Khionu#8708
            // sample userinfo Khionu returns: Khionu#8708
            // sample userinfo 96642168176807936 returns: Khionu#8708
            // sample whois 96642168176807936 returns: Khionu#8708
            [Command("userinfo")]
            [Summary("Returns info about the current user, or the user parameter, if one passed.")]
            [Alias("user", "whois")]
            public async Task UserInfoAsync([Summary("The (optional) user to get info from")] SocketUser user = null)
            {
                if(user != null)
                {
                    var userInfo = user;
                    await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
                }
                else
                {
                    var userInfo = Context.Client.CurrentUser;
                    await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
                }
            }
        }
    }
}
