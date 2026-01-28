using System;
using CounterStrikeSharp.API.Core;

namespace ChatProcessor.API;

/// <summary>
/// Api for ChatProcessor.
/// </summary>
public interface IChatProcessor
{
    delegate HookResult MessageCallbackPre(CCSPlayerController sender, ref string name, ref string message, ref List<CCSPlayerController> recipients, ref ChatFlags flags);
    delegate void MessageCallbackPost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, ChatFlags flags);

    /// <summary>
    /// Registers a Pre chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterPre(MessageCallbackPre handler);

    /// <summary>
    /// Registers a Post chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to register.</param>
    void RegisterPost(MessageCallbackPost handler);

    /// <summary>
    /// De-registers a Pre chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    void DeregisterPre(MessageCallbackPre handler);

    /// <summary>
    /// De-registers a Post chat event handler.
    /// </summary>
    /// <param name="handler">The event handler to deregister.</param>
    void DeregisterPost(MessageCallbackPost handler);
}
