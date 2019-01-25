# RockLib.Configuration.MessagingProvider [![Build status](https://ci.appveyor.com/api/projects/status/qoufisw8y904oawa?svg=true)](https://ci.appveyor.com/project/RockLib/rocklib-configuration)

A configuration provider that reloads when it receives a message containing configuration changes from a `RockLib.Messaging.IReceiver`.

### Setup

Add the messaging provider to an existing configuration builder with the `AddRockLibMessagingProvider` extension (in the `RockLib.Configuration.MessagingProvider` namespace). Note that the messaging configuration provider does not add any values to the configuration initially.

There are two overloads for the extension method. The first one uses an existing instance of `IReceiver`:

```c#
IReceiver receiver = new NamedPipeReceiver(name: "example_receiver", pipeName: "example-pipe-name");

ConfigurationBuilder builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddRockLibMessagingProvider(receiver); // Add the messaging provider last
```

The other overload creates a new instance of `IReceiver` by name from the configuration under construction:

```json
{
  "RockLib.Messaging": {
    "type": "RockLib.Messaging.NamedPipes.NamedPipeReceiver, RockLib.Messaging.NamedPipes",
    "value": {
      "name": "example_receiver",
      "pipeName": "example-pipe-name"
    }
  }
}
```

```c#
ConfigurationBuilder builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json") // Assume appsettings.json contains the above JSON
    .AddRockLibMessagingProvider("configuration_messaging_provider"); // Add the messaging provider last
```

Each of these example adds an equivalent messaging provider to the configuration builder. All `IConfigurationRoot` objects built by the configuration builder will listen to the same `IReceiver` for configuration changes.

### Making a configuration change

This section assumes there is an `appsettings.json` file and a configuration built as follows:

```json
{
  "AppSettings": {
    "Foo": "abc",
    "Bar": 123
  }
}
```

```c#
IReceiver receiver = new NamedPipeReceiver(name: "example_receiver", pipeName: "example-pipe-name");

ConfigurationBuilder builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddRockLibMessagingProvider(receiver);

IConfigurationRoot configuration = builder.Build();
```

To make a configuration change, send a message to the receiver with a JSON payload that describes the configuration changes. In this example, we want to change the values of `AppSettings:Foo` and `AppSettings:Bar` to `"xyz"` and `456` respectively. Note that the JSON is flattened into key/value pairs.

```c#
string configChange = @"{
  ""AppSettings:Foo"": ""xyz"",
  ""AppSettings:Bar"": 456
}";

using (ISender sender = new NamedPipeSender(name: "example_receiver", pipeName: "example-pipe-name"))
{
    await sender.SendAsync(configChange);
}
```

After the message is received, the `configuration` object will have the new values for "AppSettings:Foo" and "AppSettings:Bar".

### Filters

The messaging configuration provider can be protected by passing a instance of the `ISettingFilter` instance to the extension methods. For reference, this is the definition of that interface:

```c#
public interface ISettingFilter
{
    bool ShouldProcessSettingChange(string setting, IReadOnlyDictionary<string, object> receivedMessageHeaders);
}
```

When a message is received, each setting is passed to the `ShouldProcessSettingChange` method along with the `Headers` property of the received message. If the method returns true, the setting is changed. Otherwise, the setting is not changed.

RockLib.Configuration.MessagingProvider has three implementations of the `ISettingFilter` interface:

- `BlocklistSettingFilter`
  - Blocks specified settings, including child settings.
  - Has an optional inner filter.
- `SafelistSettingFilter`
  - Blocks any settings, including child settings, that are *not* specified.
  - Has an optional inner filter.
- `NullSettingFilter`
  - Doesn't block any settings.
