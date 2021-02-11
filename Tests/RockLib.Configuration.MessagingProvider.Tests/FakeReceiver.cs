using RockLib.Messaging;

namespace RockLib.Configuration.MessagingProvider.Tests
{
    internal class FakeReceiver : Receiver
    {
        public FakeReceiver(string name)
            : base(name)
        {
        }

        public bool IsStarted { get; private set; }

        protected override void Start()
        {
            IsStarted = true;
        }
    }
}
