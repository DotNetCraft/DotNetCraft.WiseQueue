﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using DotNetCraft.Common.Core.Utils.Logging;
using DotNetCraft.Common.Utils.Logging;
using DotNetCraft.WiseQueue.Core.Caching;
using DotNetCraft.WiseQueue.Core.Converters;

namespace DotNetCraft.WiseQueue.Domain.Common.Converters
{
    public class ExpressionConverter : IExpressionConverter
    {
        #region Fields...

        private readonly ICommonLogger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IJsonConverter"/> instance.
        /// </summary>
        private readonly IJsonConverter jsonConverter;

        /// <summary>
        /// The <see cref="ICachedExpressionCompiler"/> instance.
        /// </summary>
        private readonly ICachedExpressionCompiler cachedExpressionCompiler;

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="jsonConverter">The <see cref="IJsonConverter"/> instance.</param>
        /// <param name="cachedExpressionCompiler">The <see cref="ICachedExpressionCompiler"/> instance.</param>
        public ExpressionConverter(IJsonConverter jsonConverter, ICachedExpressionCompiler cachedExpressionCompiler)
        {
            if (jsonConverter == null)
                throw new ArgumentNullException("jsonConverter");
            if (cachedExpressionCompiler == null)
                throw new ArgumentNullException("cachedExpressionCompiler");

            this.jsonConverter = jsonConverter;
            this.cachedExpressionCompiler = cachedExpressionCompiler;
        }

        #region Implementation of IExpressionConverter

        #region Serialize helpful methods...

        /// <summary>
        /// Extract arguments from the expression.
        /// </summary>
        /// <param name="expressions">Arguments.</param>
        /// <returns><c>List</c> of arguments.</returns>
        private object[] ExtractArguments(IReadOnlyCollection<Expression> expressions)
        {
            object[] result = new object[expressions.Count];
            for (int i = 0; i < expressions.Count; i++)
            {
                Expression expression = expressions.ElementAt(i);
                object item = cachedExpressionCompiler.GetValue(expression);
                result[i] = item;
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Convert <see cref="Expression"/> into the <see cref="ActivationData"/> instance.
        /// </summary>
        /// <param name="action">The <see cref="Expression"/> instance.</param>
        /// <returns>The <see cref="ActivationData"/> instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Expression body should be of type `MethodCallExpression`</exception>
        /// <exception cref="InvalidOperationException">Expression object should be not null.</exception>
        public ActivationData Convert(Expression<Action> action)
        {
            logger.Trace("Converting Expression<Action> into the ActivationData...");

            if (action == null)
                throw new ArgumentNullException("action");

            MethodCallExpression callExpression = action.Body as MethodCallExpression;
            if (callExpression == null)
                throw new ArgumentException("Expression body should be of type `MethodCallExpression`", "action");

            Type instanceType;

            if (callExpression.Object != null)
            {
                var objectValue = cachedExpressionCompiler.GetValue(callExpression.Object);
                if (objectValue == null)
                    throw new InvalidOperationException("Expression object should be not null.");

                instanceType = objectValue.GetType();
            }
            else
            {
                instanceType = callExpression.Method.DeclaringType;
            }

            MethodInfo method = callExpression.Method;

            object[] arguments = ExtractArguments(callExpression.Arguments);
            Type[] argumentTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();

            ActivationData result = new ActivationData(instanceType, method, arguments, argumentTypes);

            logger.Trace("Converting Expression<Action> into the TaskActivationDetailsModel has been successfully completed.");

            return result;
        }

        public string[] SerializeArguments(IReadOnlyCollection<object> arguments)
        {
            var serializedArguments = new List<string>(arguments.Count);
            foreach (var argument in arguments)
            {
                string value;

                if (argument != null)
                {
                    if (argument is DateTime)
                    {
                        value = ((DateTime)argument).ToString("o", CultureInfo.InvariantCulture);
                    }
                    else if (argument is CancellationToken)
                    {
                        // CancellationToken type instances are substituted with ShutdownToken 
                        // during the background job performance, so we don't need to store 
                        // their values.
                        value = null;
                    }
                    else
                    {
                        value = jsonConverter.ConvertToJson(argument);
                    }
                }
                else
                {
                    value = null;
                }

                // Logic, related to optional parameters and their default values, 
                // can be skipped, because it is impossible to omit them in 
                // lambda-expressions (leads to a compile-time error).

                serializedArguments.Add(value);
            }

            return serializedArguments.ToArray();
        }

        public MethodInfo GetNonOpenMatchingMethod(Type instanceType, string methodName, Type[] argumentTypes)
        {
            var methodCandidates = new List<MethodInfo>(instanceType.GetMethods());

            if (instanceType.IsInterface)
            {
                methodCandidates.AddRange(instanceType.GetInterfaces().SelectMany(x => x.GetMethods()));
            }

            foreach (var methodCandidate in methodCandidates)
            {
                if (!methodCandidate.Name.Equals(methodName, StringComparison.Ordinal))
                    continue;

                ParameterInfo[] parameters = methodCandidate.GetParameters();
                if (parameters.Length != argumentTypes.Length)
                    continue;

                bool parameterTypesMatched = true;
                List<Type> genericArguments = new List<Type>();

                // Determining whether we can use this method candidate with
                // current parameter types.
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    Type parameterType = parameter.ParameterType;
                    Type actualType = argumentTypes[i];

                    // Skipping generic parameters as we can use actual type.
                    if (parameterType.IsGenericParameter)
                    {
                        genericArguments.Add(actualType);
                        continue;
                    }

                    // Skipping non-generic parameters of assignable types.
                    if (parameterType.IsAssignableFrom(actualType)) continue;

                    parameterTypesMatched = false;
                    break;
                }

                if (!parameterTypesMatched) continue;

                // Return first found method candidate with matching parameters.
                return methodCandidate.ContainsGenericParameters
                    ? methodCandidate.MakeGenericMethod(genericArguments.ToArray())
                    : methodCandidate;
            }

            return null; //TODO: NullReferenceExcretion can be raised because of this.
        }

        private object DeserializeArgument(string argument, Type type)
        {
            object value;
            try
            {
                value = argument != null
                    ? jsonConverter.ConvertFromJson(argument, type)
                    : null;
            }
            catch (Exception jsonException)
            {
                if (type == typeof(object))
                {
                    value = argument;
                }
                else
                {
                    try
                    {
                        TypeConverter converter = TypeDescriptor.GetConverter(type);
                        value = converter.ConvertFromInvariantString(argument);
                    }
                    catch (Exception)
                    {
                        throw jsonException;
                    }
                }
            }
            return value;
        }

        public object[] DeserializeArguments(MethodInfo methodInfo, string[] serializedArguments)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            List<object> result = new List<object>(serializedArguments.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                string argument = serializedArguments[i];

                var value = DeserializeArgument(argument, parameter.ParameterType);

                result.Add(value);
            }

            return result.ToArray();
        }

        #endregion
    }
}
