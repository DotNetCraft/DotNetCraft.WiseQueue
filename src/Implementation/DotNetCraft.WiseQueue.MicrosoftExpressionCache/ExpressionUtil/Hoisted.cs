﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace DotNetCraft.WiseQueue.MicrosoftExpressionCache.ExpressionUtil
{
    internal delegate TValue Hoisted<in TModel, out TValue>(TModel model, List<object> capturedConstants);
}
