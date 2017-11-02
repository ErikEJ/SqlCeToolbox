// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Modifications copyright(C) 2017 Tony Sneed.

using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Scaffolding.Handlebars.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class RelationalPropertyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetConfiguredColumnType(this IProperty property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            return (string) property[RelationalAnnotationNames.ColumnType];
        }
    }
}
