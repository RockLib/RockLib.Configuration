# RockLib.Configuration.MessagingProvider [![Build status](https://ci.appveyor.com/api/projects/status/qoufisw8y904oawa?svg=true)](https://ci.appveyor.com/project/RockLib/rocklib-configuration)

A configuration provider that reloads when it receives a message from a `RockLib.Messaging.IReceiver` that specifies configuration changes to apply.

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
using (ISender sender = new NamedPipeSender(name: "example_receiver", pipeName: "example-pipe-name"))
{
    string configChange = @"{
  ""AppSettings:Foo"": ""xyz"",
  ""AppSettings:Bar"": 456
}";

    await sender.SendAsync(configChange);
}
```

After the message is receiver, the `configuration` object will have the new values.

If a configuration change needs to be made, and then reverted after a set amount of time, send a message with a `RevertAfterMilliseconds` header. In this example we want the configuration change to last for 30 seconds:

```c#
using (ISender sender = new NamedPipeSender(name: "example_receiver", pipeName: "example-pipe-name"))
{
    string configChange = @"{
  ""AppSettings:Foo"": ""xyz"",
  ""AppSettings:Bar"": 456
}";

    var message = new SenderMessage(configChange);
    message.Headers.Add("RevertAfterMilliseconds", 30000);
    
    await sender.SendAsync(message);
}
```
