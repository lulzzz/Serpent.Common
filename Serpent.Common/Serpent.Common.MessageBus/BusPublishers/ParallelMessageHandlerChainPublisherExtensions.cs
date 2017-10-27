// ReSharper disable once CheckNamespace

namespace Serpent.Common.MessageBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Common.MessageBus.BusPublishers;
    using Serpent.Common.MessageBus.Exceptions;
    using Serpent.Common.MessageBus.Models;

    /// <summary>
    /// Extensions for parallel message handler chain publisher
    /// </summary>
    public static class ParallelMessageHandlerChainPublisherExtensions
    {
        /// <summary>
        /// Sets up a message handler chain for the bus publisher
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="options">The options</param>
        /// <param name="configureMessageHandlerChain">The action called to setup the message handler chain</param>
        /// <returns>Bus options</returns>
        public static ConcurrentMessageBusOptions<TMessageType> UseSubscriptionChain<TMessageType>(
            this ConcurrentMessageBusOptions<TMessageType> options,
            Action<MessageHandlerChainBuilder<MessageAndHandler<TMessageType>>, Func<MessageAndHandler<TMessageType>, CancellationToken, Task>> configureMessageHandlerChain)
        {
            var dispatch = new MessageHandlerPublishDispatch<MessageAndHandler<TMessageType>>();

            var builder = new MessageHandlerChainBuilder<MessageAndHandler<TMessageType>>(dispatch);
            configureMessageHandlerChain(builder, PublishToSubscription.PublishAsync<TMessageType>);

            if (dispatch.InvocationFunc == null)
            {
                throw new NoHandlerException("No handler was added to the message handler chain. Messages can not be dispatched to the bus.\r\nUse .Handler() or .Factory() on the message handler chain.");
            }

            options.UseCustomPublisher(new ParallelMessageHandlerChainPublisher<TMessageType>(builder.Build(dispatch.InvocationFunc)));
            return options;
        }

        /// <summary>
        /// Sets up a message handler chain for the bus publisher
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="options">The options</param>
        /// <param name="configureMessageHandlerChain">The action called to setup the message handler chain</param>
        /// <returns>Bus options</returns>
        public static ConcurrentMessageBusOptions<TMessageType> UseSubscriptionChain<TMessageType>(
            this ConcurrentMessageBusOptions<TMessageType> options,
            Action<MessageHandlerChainBuilder<MessageAndHandler<TMessageType>>> configureMessageHandlerChain)
        {
            var dispatch = new MessageHandlerPublishDispatch<MessageAndHandler<TMessageType>>();
            var builder = new MessageHandlerChainBuilder<MessageAndHandler<TMessageType>>(dispatch);
            
            configureMessageHandlerChain(builder);
            options.UseCustomPublisher(new ParallelMessageHandlerChainPublisher<TMessageType>(builder.Build(dispatch.InvocationFunc ?? PublishToSubscription.PublishAsync<TMessageType>)));
            return options;
        }
    }
}