﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Converters;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Azure.WebJobs.Host.Queues.Bindings
{
    internal class CollectorArgumentBindingProvider : IQueueArgumentBindingProvider
    {
        public IArgumentBinding<CloudQueue> TryCreate(ParameterInfo parameter)
        {
            Type parameterType = parameter.ParameterType;

            if (!parameterType.IsGenericType)
            {
                return null;
            }

            Type genericTypeDefinition = parameterType.GetGenericTypeDefinition();

            if (genericTypeDefinition != typeof(ICollector<>))
            {
                return null;
            }

            Type itemType = parameterType.GetGenericArguments()[0];
            return CreateBinding(itemType);
        }

        private static IArgumentBinding<CloudQueue> CreateBinding(Type itemType)
        {
            MethodInfo method = typeof(CollectorArgumentBindingProvider).GetMethod("CreateBindingGeneric",
                BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(method != null);
            MethodInfo genericMethod = method.MakeGenericMethod(itemType);
            Debug.Assert(genericMethod != null);
            Func<IArgumentBinding<CloudQueue>> lambda = (Func<IArgumentBinding<CloudQueue>>)Delegate.CreateDelegate(
                typeof(Func<IArgumentBinding<CloudQueue>>), genericMethod);
            return lambda.Invoke();
        }

        private static IArgumentBinding<CloudQueue> CreateBindingGeneric<TItem>()
        {
            return new CollectorQueueArgumentBinding<TItem>(MessageConverterFactory.Create<TItem>());
        }

        private class CollectorQueueArgumentBinding<TItem> : IArgumentBinding<CloudQueue>
        {
            private readonly IMessageConverterFactory<TItem> _converterFactory;

            public CollectorQueueArgumentBinding(IMessageConverterFactory<TItem> converterFactory)
            {
                _converterFactory = converterFactory;
            }

            public Type ValueType
            {
                get { return typeof(ICollector<TItem>); }
            }

            public Task<IValueProvider> BindAsync(CloudQueue value, ValueBindingContext context)
            {
                IConverter<TItem, CloudQueueMessage> converter = _converterFactory.Create(context.FunctionInstanceId);
                ICollector<TItem> collector = new QueueCollector<TItem>(value, converter);
                IValueProvider provider = new CollectorValueProvider(value, collector, typeof(ICollector<TItem>));
                return Task.FromResult(provider);
            }
        }
    }
}