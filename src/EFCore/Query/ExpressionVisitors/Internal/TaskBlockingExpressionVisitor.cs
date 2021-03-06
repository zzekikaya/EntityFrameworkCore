// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class TaskBlockingExpressionVisitor : ExpressionVisitorBase, ITaskBlockingExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                var typeInfo = expression.Type.GetTypeInfo();

                if (typeInfo.IsGenericType
                    && typeInfo.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return Expression.Call(
                        ResultMethodInfo.MakeGenericMethod(typeInfo.GenericTypeArguments[0]),
                        expression);
                }
            }

            return expression;
        }

        internal static MethodInfo ResultMethodInfo { get; }
            = typeof(TaskBlockingExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Result));

        [UsedImplicitly]
        private static T Result<T>(Task<T> task) => task.GetAwaiter().GetResult();
    }
}
