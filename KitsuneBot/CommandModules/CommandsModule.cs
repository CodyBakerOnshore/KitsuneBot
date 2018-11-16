using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KitsuneBot.Properties;
using System;
using System.IO;
using System.Threading.Tasks;

// Create a module with no prefix
public class InfoModule : ModuleBase
{
    // ~say hello -> hello
    [Command("say"), Summary("Echos a message.")]
    public async Task Say([Remainder, Summary("The text to echo")] string echo)
    {
        // ReplyAsync is a method on ModuleBase
        await ReplyAsync(echo);
    }
}


public class SquareModule : ModuleBase
{
    // ~sample square 20 -> 400
    [Command("square"), Summary("Squares a number.")]
    public async Task Square([Summary("The number to square.")] int num)
    {
        // We can also access the channel from the Command Context.
        await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
    }
}

public class UserInfoModule : ModuleBase
{
    // ~sample userinfo --> foxbot#0282
    // ~sample userinfo @Khionu --> Khionu#8708
    // ~sample userinfo Khionu#8708 --> Khionu#8708
    // ~sample userinfo Khionu --> Khionu#8708
    // ~sample userinfo 96642168176807936 --> Khionu#8708
    // ~sample whois 96642168176807936 --> Khionu#8708
    [Command("userinfo"), Summary("Returns info about the current user, or the user parameter, if one passed.")]
    [Alias("user", "whois")]
    public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
    {
        if (user == null)
        {
            user = Context.User;
        }
        var userInfo = user ?? Context.Client.CurrentUser;
        await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
    }
}

public class CommandRequest : ModuleBase
{
    [Command("crequest"), Summary("Adds a Request to the logs for new commands that people want added to the bot")]
    public async Task CRequest([Summary("The Message that was used in request")] string message, string user = null)
    {
        if (message.Contains(" "))
        {
            message.Replace(" ", "_");
        }
        user = Context.User.Username;
        var Requester = message.ToString();
        Console.WriteLine(Requester);
        Console.WriteLine("Attn Bot Owner: {0}", message);
        var PATH = Resources.CommandRequestStore;
        await File.AppendAllTextAsync(PATH, user + ": " + message);
        await ReplyAsync(user + " Request Logged!");
    }
}

[Group("Vote")]
public class Vote : ModuleBase
{
    private string creator;
    [Command("start")]
    public async Task StartVote([Summary("Runs a command that will store replies as votes")] IMessage message, IUser user)
    {
        try
        {
            if (creator != null)
            {
                await user.SendMessageAsync("There is already a vote running please wait until it is finished!");
                await message.DeleteAsync();
            }
            creator = message.Author.Id.ToString();
            var startTime = message.Timestamp;
            var end = startTime.ToLocalTime().AddMinutes(30);
            if (DateTime.Now.ToLocalTime() == end)
            {
                await ReplyAsync(message.Author + "30 Minutes have passed!");
            }
        }
        catch (Exception e)
        {
            throw e;
        }

    }

    [Command("submit")]
    public async Task Submit(string message, IUser user = null)
    {
        var PATH = Resources.VoteReplies;
        await File.AppendAllTextAsync(PATH,
                                        "UserName: " + user.Username.ToString() + "\n\r " +
                                        "UserID: " + user.Id.ToString() + "\n\r " +
                                        "UserCreated: " + user.CreatedAt.ToString() + "\n\r " +
                                        "Message: " + message.Replace(" ", "_").ToString() + "\r\n"
                                        );
    }

    [Command("end")]
    public async Task EndVote([Summary("Runs a command that will read replies as votes")] IUser user, IMessageChannel channel)
    {
        if (user.Id.ToString() != creator)
        {
            await user.SendMessageAsync("You are not the creator of the current vote!");
        }
        var PATH = Resources.VoteReplies;
        var votes = await File.ReadAllTextAsync(PATH);

        await user.SendMessageAsync(votes);
        File.Delete(PATH);
        await channel.SendMessageAsync("Voting Is Over! Creator of vote should be reciving a PM with all of the votes!");
    }
}