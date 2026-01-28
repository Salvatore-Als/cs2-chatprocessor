using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using ChatProcessor.API;
using Microsoft.Extensions.Logging;

namespace ChatProcessor.Example;

[MinimumApiVersion(318)]
public class ChatProcessorExample : BasePlugin
{
    public override string ModuleName => "ChatProcessor Example";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Kriax";
    public override string ModuleDescription => "Example plugin for ChatProcessor";

    private readonly PluginCapability<IChatProcessor> _chatProcessorCapability = new("ChatProcessor:api");
    private IChatProcessor? _chatProcessor;

    public override void Load(bool hotReload)
    {
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _chatProcessor = _chatProcessorCapability.Get();

        if (_chatProcessor == null)
        {
            Logger.LogError("ChatProcessor API not found! Make sure ChatProcessor is loaded.");
            return;
        }

        _chatProcessor.RegisterPre(OnChatMessagePre);
        Logger.LogInformation("ChatProcessor Example plugin loaded successfully!");
    }

    public override void Unload(bool hotReload)
    {
        if (_chatProcessor != null)
            _chatProcessor.DeregisterPre(OnChatMessagePre);
    }

    private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref ChatFlags flags)
    {
        char teamColor = ChatColors.ForPlayer(sender);
        name = $" {ChatColors.Red}[TEST] {teamColor}{name}";

        if(message.Contains("color"))
            message = $"{ChatColors.Green}{message}";

        return HookResult.Changed;
    }
}
