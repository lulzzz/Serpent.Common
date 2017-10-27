﻿namespace Serpent.Common.MessageBus.MessageHandlerChain.Decorators.Take
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Common.MessageBus.Interfaces;

    internal class TakeDecorator<TMessageType> : MessageHandlerChainDecorator<TMessageType>
    {
        private readonly Func<TMessageType, CancellationToken, Task> handlerFunc;

        private int count;

        private IMessageBusSubscription subscription;

        public TakeDecorator(Func<TMessageType, CancellationToken, Task> handlerFunc, int numberOfMessages, IMessageHandlerChainSubscriptionNotification subscriptionNotification)
        {
            this.handlerFunc = handlerFunc;
            this.count = numberOfMessages;
            subscriptionNotification.AddNotification(this.SetSubscription);
        }

        /// <summary>
        ///     Handles a message.
        /// </summary>
        /// <param name="message">
        ///     The message to handle.
        /// </param>
        /// <param name="cancellationToken">
        ///     A cancellation token that should be used to cancel the message handler.
        /// </param>
        /// <returns>
        ///     A <see cref="Task" /> that completes when the message is handled.
        /// </returns>
        public override Task HandleMessageAsync(TMessageType message, CancellationToken cancellationToken)
        {
            if (this.count > 0)
            {
                var ourCount = Interlocked.Decrement(ref this.count);

                if (ourCount >= 0)
                {
                    return this.handlerFunc(message, cancellationToken);
                }
            }

            this.subscription?.Dispose();

            return Task.CompletedTask;
        }

        private void SetSubscription(IMessageBusSubscription sub)
        {
            this.subscription = sub;
        }
    }
}