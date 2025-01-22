# :warning: Deprecation Warning :warning:

This library has been deprecated and will no longer receive updates.

---

RockLib has been a cornerstone of our open source efforts here at Rocket Mortgage, and it's played a significant role in our journey to drive innovation and collaboration within our organization and the open source community. It's been amazing to witness the collective creativity and hard work that you all have poured into this project.

However, as technology relentlessly evolves, so must we. The decision to deprecate this library is rooted in our commitment to staying at the cutting edge of technological advancements. While this chapter is ending, it opens the door to exciting new opportunities on the horizon.

We want to express our heartfelt thanks to all the contributors and users who have been a part of this incredible journey. Your contributions, feedback, and enthusiasm have been invaluable, and we are genuinely grateful for your dedication. 🚀

---

This repository contains the source code for four nuget packages:

### [RockLib.Configuration](RockLib.Configuration/README.md)

  Defines a static `Config` class as a general replacement for the old .NET Framework `ConfigurationManager` class.

### [RockLib.Configuration.ObjectFactory](RockLib.Configuration.ObjectFactory/README.md)

An alternative to `Microsoft.Extensions.Configuration.Binder` that supports non-default constructors and other features commonly found in JSON and XML serializers.

### [RockLib.Configuration.ProxyFactory](RockLib.Configuration.ProxyFactory/README.md)

A factory that creates instances of property-only interfaces, defined at run-time, and populated with values defined in an instance of `IConfiguration`.

### [RockLib.Configuration.MessagingProvider](RockLib.Configuration.MessagingProvider/README.md)

A configuration provider that reloads when it receives a message containing configuration changes from a `RockLib.Messaging.IReceiver`.
