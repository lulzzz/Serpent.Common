﻿namespace Serpent.Common.MessageBus.Extras
{
    using System;
    using System.Threading.Tasks;

    public struct SubscriptionsBridge<TMessageType> : IMessageBusSubscriptions<TMessageType>
    {
        private readonly IMessageBus<TMessageType> bus;

        public SubscriptionsBridge(IMessageBus<TMessageType> bus)
        {
            this.bus = bus;
        }

        public IMessageBusSubscription Subscribe(Func<TMessageType, Task> invocationFunc)
        {
            return this.bus.Subscribe(invocationFunc);
        }
    }
}