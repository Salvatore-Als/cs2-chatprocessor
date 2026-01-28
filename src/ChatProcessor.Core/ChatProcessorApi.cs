using System;
using CounterStrikeSharp.API.Core;
using ChatProcessor.API;

namespace ChatProcessor;

public class ChatProcessorApi : IChatProcessor
{
    private readonly List<IChatProcessor.MessageCallbackPre> _messagePreHandlers = [];
    private readonly List<IChatProcessor.MessageCallbackPost> _messagePostHandlers = [];

    public ChatProcessorApi(ChatProcessor chatProcessor)
    {
      
    }

    #region Register/Deregister Handlers

    public void RegisterPre(IChatProcessor.MessageCallbackPre handler)
    {
        _messagePreHandlers.Add(handler);
    }

    public void RegisterPost(IChatProcessor.MessageCallbackPost handler)
    {
        _messagePostHandlers.Add(handler);
    }

    public void DeregisterPre(IChatProcessor.MessageCallbackPre handler)
    {
        _messagePreHandlers.Remove(handler);
    }

    public void DeregisterPost(IChatProcessor.MessageCallbackPost handler)
    {
        _messagePostHandlers.Remove(handler);
    }

    #endregion

    #region Trigger Handlers

    public HookResult TriggerMessagePre(CCSPlayerController sender, ref string name, ref string message,
        ref List<CCSPlayerController> recipients, ref ChatFlags flags)
    {
        foreach (var handler in _messagePreHandlers)
        {
            // Save original values before handler execution
            string savedName = name;
            string savedMessage = message;
            List<CCSPlayerController> savedRecipients = new List<CCSPlayerController>(recipients);
            ChatFlags savedFlags = flags;

            HookResult result = handler.Invoke(sender, ref name, ref message, ref recipients, ref flags);

            switch (result)
            {
                case HookResult.Stop:
                    // Stop completely - don't send the message
                    return HookResult.Stop;

                case HookResult.Continue:
                    // Ignore modifications from this handler, restore saved values
                    name = savedName;
                    message = savedMessage;
                    recipients = savedRecipients;
                    flags = savedFlags;
                    break;

                case HookResult.Changed:
                    // Message was modified, continue to next handler (they can modify further)
                    break;

                case HookResult.Handled:
                    // Message was handled and modified, stop processing other handlers
                    // Return Changed so the message gets sent (Handled would prevent sending)
                    return HookResult.Changed;
            }
        }

        // If we get here, either all handlers returned Continue or Changed
        // Return Changed to indicate modifications were made (if any)
        return HookResult.Changed;
    }

    public void TriggerMessagePost(CCSPlayerController sender, string name, string message, List<CCSPlayerController> recipients, ChatFlags flags)
    {
        foreach (IChatProcessor.MessageCallbackPost handler in _messagePostHandlers)
        {
            handler.Invoke(sender, name, message, recipients, flags);
        }
    }

    #endregion
}
