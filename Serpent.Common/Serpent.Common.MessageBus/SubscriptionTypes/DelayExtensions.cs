﻿// ReSharper disable once CheckNamespace

namespace Serpent.Common.MessageBus
{
    using System;

    using Serpent.Common.MessageBus.SubscriptionTypes;

    public static class DelayExtensions
    {
        public static IMessageHandlerChainBuilder<TMessageType> Delay<TMessageType>(this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder, TimeSpan timeToWait)
        {
            return messageHandlerChainBuilder.Add(currentHandler => new DelayDecorator<TMessageType>(currentHandler, timeToWait).HandleMessageAsync);
        }

        public static IMessageHandlerChainBuilder<TMessageType> Delay<TMessageType>(
            this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder,
            int timeInMilliseconds)
        {
            return messageHandlerChainBuilder.Add(
                currentHandler => new DelayDecorator<TMessageType>(currentHandler, TimeSpan.FromMilliseconds(timeInMilliseconds)).HandleMessageAsync);
        }
    }


// #define README
#if README
    internal interface IDelayExtensionsForReadme
    {
        IMessageHandlerChainBuilder<TMessageType> Delay<TMessageType>(TimeSpan timeToWait);

        IMessageHandlerChainBuilder<TMessageType> Delay<TMessageType>(int timeInMilliseconds);
    }
#endif
}