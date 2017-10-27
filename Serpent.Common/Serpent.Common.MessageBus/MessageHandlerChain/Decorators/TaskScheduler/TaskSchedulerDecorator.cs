﻿namespace Serpent.Common.MessageBus.MessageHandlerChain.Decorators.TaskScheduler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class TaskSchedulerDecorator<TMessageType> : MessageHandlerChainDecorator<TMessageType>
    {
        private readonly Func<TMessageType, CancellationToken, Task> handlerFunc;

        private readonly TaskScheduler taskScheduler;

        public TaskSchedulerDecorator(Func<TMessageType, CancellationToken, Task> handlerFunc, TaskScheduler taskScheduler)
        {
            this.handlerFunc = handlerFunc;
            this.taskScheduler = taskScheduler;
        }

        public TaskSchedulerDecorator(MessageHandlerChainDecorator<TMessageType> innerSubscription, TaskScheduler taskScheduler)
        {
            this.taskScheduler = taskScheduler;
            this.handlerFunc = innerSubscription.HandleMessageAsync;
        }

        public override Task HandleMessageAsync(TMessageType message, CancellationToken token)
        {
            var task = Task.Factory.StartNew(
                () => this.handlerFunc(message, token), 
                token,
                TaskCreationOptions.None, 
                this.taskScheduler).Unwrap();
            return task;
        }
    }
}