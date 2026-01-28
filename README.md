# CS2 ChatProcessor

A simple chat processing API plugin for CounterStrikeSharp that allows other plugins to intercept, modify, and format chat messages before they are sent to players.

## Acknowledgments

Special thanks to [VeryGames](https://verygames.com) for providing their test server, which was instrumental in the development and testing of this plugin.

## Features

- **Pre-processing hooks**: Modify chat messages before they are sent (change name, message, recipients, flags)
- **Post-processing hooks**: Execute code after messages are sent
- **Multiple handler support**: Register multiple handlers that process messages in sequence
- **Flexible API**: Simple and intuitive interface for plugin developers
- **Type-safe**: Strongly typed delegates for better development experience

## Installation

1. Build the project:
   ```bash
   ./build.sh
   ```

2. Copy the built files to your CounterStrikeSharp installation:
   - Copy `build/shared/ChatProcessorApi/` to `addons/counterstrikesharp/shared/`
   - Copy `build/plugins/ChatProcessorCore/` to `addons/counterstrikesharp/plugins/`

3. Restart your server or reload plugins

## Usage

### Getting the API

```csharp
using ChatProcessor.API;
using CounterStrikeSharp.API.Core.Capabilities;

private readonly PluginCapability<IChatProcessor> _chatProcessorCapability = new("ChatProcessor:api");
private IChatProcessor? _chatProcessor;

public override void OnAllPluginsLoaded(bool hotReload)
{
    _chatProcessor = _chatProcessorCapability.Get();
    if (_chatProcessor == null)
    {
        Logger.LogError("ChatProcessor API not found!");
        return;
    }
    
    _chatProcessor.RegisterPre(OnChatMessagePre);
}
```

### Registering Pre Handlers

Pre handlers allow you to modify messages before they are sent:

```csharp
private HookResult OnChatMessagePre(CCSPlayerController sender, ref string name, ref string message, 
    ref List<CCSPlayerController> recipients, ref ChatFlags flags)
{
    // Modify the name
    name = $"[TAG] {name}";
    
    // Modify the message
    message = $"{message} (modified)";
    
    // Return Changed to apply modifications
    return HookResult.Changed;
}

public override void OnAllPluginsLoaded(bool hotReload)
{
    _chatProcessor?.RegisterPre(OnChatMessagePre);
}
```

### Registering Post Handlers

Post handlers execute after messages are sent (read-only):

```csharp
private void OnChatMessagePost(CCSPlayerController sender, string name, string message, 
    List<CCSPlayerController> recipients, ChatFlags flags)
{
    Logger.LogInformation($"Message sent: {name} said '{message}'");
}

public override void OnAllPluginsLoaded(bool hotReload)
{
    _chatProcessor?.RegisterPost(OnChatMessagePost);
}
```

### Handler Return Values

When using Pre handlers, you can return different values:

- `HookResult.Continue`: Ignore modifications from this handler, continue to next handler
- `HookResult.Changed`: Apply modifications, continue to next handler
- `HookResult.Handled`: Apply modifications and stop processing other handlers
- `HookResult.Stop`: Stop completely, don't send the message

### Unregistering Handlers

Always unregister handlers when unloading your plugin:

```csharp
public override void Unload(bool hotReload)
{
    if (_chatProcessor != null)
    {
        _chatProcessor.DeregisterPre(OnChatMessagePre);
        _chatProcessor.DeregisterPost(OnChatMessagePost);
    }
}
```

## API Reference

### IChatProcessor Interface

```csharp
public interface IChatProcessor
{
    delegate HookResult MessageCallbackPre(CCSPlayerController sender, ref string name, 
        ref string message, ref List<CCSPlayerController> recipients, ref ChatFlags flags);
    
    delegate void MessageCallbackPost(CCSPlayerController sender, string name, 
        string message, List<CCSPlayerController> recipients, ChatFlags flags);

    void RegisterPre(MessageCallbackPre handler);
    void RegisterPost(MessageCallbackPost handler);
    void DeregisterPre(MessageCallbackPre handler);
    void DeregisterPost(MessageCallbackPost handler);
}
```

### ChatFlags Enum

```csharp
[Flags]
public enum ChatFlags
{
    None = 0,
    Team = (1 << 0),
    Dead = (1 << 1)
}
```

## Example Plugin

See `src/ChatProcessor.Example/` for a complete example plugin demonstrating how to use the ChatProcessor API.

## Building

```bash
./build.sh          # Build in Debug mode (default)
./build.sh debug    # Build in Debug mode
./build.sh release  # Build in Release mode
```

## Requirements

- CounterStrikeSharp API version 318 or higher
- .NET 8.0
