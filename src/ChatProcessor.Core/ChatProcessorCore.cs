using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Core.Capabilities;
using ChatProcessor.API;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace ChatProcessor;

[MinimumApiVersion(318)]
public class ChatProcessor : BasePlugin
{
    public override string ModuleName => "CS2 ChatProcessor";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kriax, Rebel's Corp Team";
    public override string ModuleDescription => "Chat API formatter";

    private readonly PluginCapability<IChatProcessor> _pluginCapability = new("ChatProcessor:api");

    private ChatProcessorApi ChatProcessorApi = null!;

    public override void Load(bool hotReload)
    {
        ChatProcessorApi = new ChatProcessorApi(this);
        Capabilities.RegisterPluginCapability(_pluginCapability, () => ChatProcessorApi);

        HookUserMessage(118, OnMessage, HookMode.Pre);
    }

    public override void Unload(bool hotReload)
    {
        UnhookUserMessage(118, OnMessage, HookMode.Pre);
    }

    public HookResult OnMessage(UserMessage um)
    {
        int entityIndex = um.ReadInt("entityindex");
        CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(entityIndex);
    
        if (player == null || player.IsBot)
            return HookResult.Continue;

        string playerName = um.ReadString("param1");
        string message = um.ReadString("param2");
        bool chatSound = um.ReadBool("chat");
        string messageName = um.ReadString("messagename");

        if (string.IsNullOrEmpty(message))
            return HookResult.Handled;

        string name = playerName;
        string originalMessage = message;

        // Determine if it's a team chat
        bool isTeamChat = !messageName.Contains("All");
        
        ChatFlags flags = ChatFlags.None;

        if (isTeamChat)
            flags |= ChatFlags.Team;

        if (!player.PawnIsAlive)
            flags |= ChatFlags.Dead;

        List<CCSPlayerController> recipients = GetRecipients(isTeamChat ? player.Team : CsTeam.None);

        if (recipients.Count == 0)
            return HookResult.Stop;

        HookResult result = ChatProcessorApi.TriggerMessagePre(player, ref name, ref message, ref recipients, ref flags);

        if (result == HookResult.Stop)
            return HookResult.Stop;

        if (result == HookResult.Continue)
            return HookResult.Continue;

        string prefixTeam = null;
        if (isTeamChat)
        {
            switch (player.Team)
            {
                case CsTeam.Terrorist:
                    prefixTeam = $" \x03[T]";
                    break;
                case CsTeam.CounterTerrorist:
                    prefixTeam = $" \x03[CT]";
                    break;
                case CsTeam.Spectator:
                    prefixTeam = $" \x03[S]";
                    break;
                default:
                    prefixTeam = null;
                    break;
            }
        }
        else {
            prefixTeam = $" {ChatColors.White}[ALL]\x03";
        }

        if (!player.PawnIsAlive)
            name = $"{prefixTeam} {name} {ChatColors.White}[DEAD]";
        else if (prefixTeam != null)
            name = $"{prefixTeam} {name}";
        else
            name = $"{name}";


        string formattedMessage = $"{name}:{ChatColors.White} {message}";
        
        um.SetString("param1", name);
        um.SetString("param2", message);
        um.SetString("messagename", formattedMessage);
        um.SetBool("chat", chatSound);

        ChatProcessorApi.TriggerMessagePost(player, name, message, recipients, flags);

        return HookResult.Changed;
    }

    private List<CCSPlayerController> GetRecipients(CsTeam team = CsTeam.None)
    {
        return Utilities.GetPlayers()
            .Where(p => p != null && p.IsValid && !p.IsBot && (team == CsTeam.None || p.Team == team))
            .ToList();
    }
}
