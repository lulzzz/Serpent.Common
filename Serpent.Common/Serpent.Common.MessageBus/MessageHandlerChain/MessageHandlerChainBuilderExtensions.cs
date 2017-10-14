﻿// ReSharper disable once CheckNamespace

namespace Serpent.Common.MessageBus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Serpent.Common.MessageBus.Interfaces;
    using Serpent.Common.MessageBus.MessageHandlerChain;

    /// <summary>
    ///     Extensions for IMessageHandlerChainBuilder
    /// </summary>
    public static class MessageHandlerChainBuilderExtensions
    {
        /// <summary>
        /// Add a decorator to the message handler chain builder
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="messageHandlerChainBuilder">The mhc builder</param>
        /// <param name="addFunc">The method that returns the </param>
        /// <returns>The mhc builder</returns>
        public static IMessageHandlerChainBuilder<TMessageType> Add<TMessageType>(
            this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder,
            Func<Func<TMessageType, CancellationToken, Task>, MessageHandlerChainDecorator<TMessageType>> addFunc)
        {
            return messageHandlerChainBuilder.Add(previousHandler => addFunc(previousHandler).HandleMessageAsync);
        }

        /// <summary>
        ///     Use a factory method to provide a message handler instance for each message passing through
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <typeparam name="THandler">The handler type</typeparam>
        /// <param name="messageHandlerChainBuilder">The mhc builder</param>
        /// <param name="handlerFactory">The handler factory method</param>
        /// <returns>The mhc builder</returns>
        public static IMessageBusSubscription Factory<TMessageType, THandler>(
            this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder,
            Func<THandler> handlerFactory)
            where THandler : IMessageHandler<TMessageType>
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(THandler)))
            {
                return messageHandlerChainBuilder.Handler(
                    async (message, token) =>
                        {
                            var handler = handlerFactory();
                            try
                            {
                                await handler.HandleMessageAsync(message, token);
                            }
                            finally
                            {
                                ((IDisposable)handler).Dispose();
                            }
                        });
            }

            return messageHandlerChainBuilder.Handler(
                (message, token) =>
                    {
                        var handler = handlerFactory();
                        return handler.HandleMessageAsync(message, token);
                    });
        }

        /// <summary>
        ///     Set the chain handler method or lambda method. Use this overload if your handler has no need for async or
        ///     cancellationToken
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="messageHandlerChainBuilder">The mhc builder</param>
        /// <param name="handlerAction">The action to invoke</param>
        /// <returns>The mhc builder</returns>
        public static IMessageBusSubscription Handler<TMessageType>(this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder, Action<TMessageType> handlerAction)
        {
            return messageHandlerChainBuilder.Handler(
                (message, token) =>
                    {
                        handlerAction(message);
                        return Task.CompletedTask;
                    });
        }

        /// <summary>
        ///     Set the chain handler method or lambda method. Use this overload if you need async but no cancellation token
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="messageHandlerChainBuilder">The mhc builder</param>
        /// <param name="handlerFunc">The method to invoke</param>
        /// <returns>The mhc builder</returns>
        public static IMessageBusSubscription Handler<TMessageType>(this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder, Func<TMessageType, Task> handlerFunc)
        {
            return messageHandlerChainBuilder.Handler((message, token) => handlerFunc(message));
        }

        /// <summary>
        ///     Set the chain handler method to a ISimpleMessageHandler
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="messageHandlerChainBuilder">The mhc builder</param>
        /// <param name="messageHandler">The ISimpleMessageHandler to invoke</param>
        /// <returns>The mhc builder</returns>
        public static IMessageBusSubscription Handler<TMessageType>(
            this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder,
            ISimpleMessageHandler<TMessageType> messageHandler)
        {
            return messageHandlerChainBuilder.Handler((message, token) => messageHandler.HandleMessageAsync(message));
        }

        /// <summary>
        ///     Set the chain message handler
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="messageHandlerChainBuilder">The mhc builder</param>
        /// <param name="messageHandler">The ISimpleMessageHandler to invoke</param>
        /// <returns>The mhc builder</returns>
        public static IMessageBusSubscription Handler<TMessageType>(
            this IMessageHandlerChainBuilder<TMessageType> messageHandlerChainBuilder,
            IMessageHandler<TMessageType> messageHandler)
        {
            return messageHandlerChainBuilder.Handler(messageHandler.HandleMessageAsync);
        }

        /// <summary>
        ///     Create a message handler chain to set up a subscription
        /// </summary>
        /// <typeparam name="TMessageType">The message type</typeparam>
        /// <param name="subscriptions">The subscriptions interface</param>
        /// <returns>The new message handler chain</returns>
        public static IMessageHandlerChainBuilder<TMessageType> Subscribe<TMessageType>(this IMessageBusSubscriptions<TMessageType> subscriptions)
        {
            return new MessageHandlerChainBuilder<TMessageType>(subscriptions);
        }
    }
}