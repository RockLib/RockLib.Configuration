using RockLib.Messaging;
using System;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    // This class is only for the appsettings.json test.
    // Use Moq to create a mocked version of Receiver instead.
    public sealed class FakeReceiver : Receiver
    {
        public FakeReceiver(string name)
            : base(name) { }

        protected override void Start()
        {
            throw new NotImplementedException();
        }
    }
}
