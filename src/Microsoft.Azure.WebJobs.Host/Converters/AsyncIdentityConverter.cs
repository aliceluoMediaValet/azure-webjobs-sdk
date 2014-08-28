﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Host.Converters
{
    internal class AsyncIdentityConverter<TValue> : IAsyncConverter<TValue, TValue>
    {
        public Task<TValue> ConvertAsync(TValue input, CancellationToken cancellationToken)
        {
            return Task.FromResult(input);
        }
    }
}