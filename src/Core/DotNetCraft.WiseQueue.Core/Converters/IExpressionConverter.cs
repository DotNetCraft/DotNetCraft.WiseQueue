using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetCraft.WiseQueue.Core.Converters
{
    /// <summary>
    /// Interface shows that object is a converter from <see cref="Expression"/> into the <see cref="ActivationData"/> and back.
    /// </summary>
    public interface IExpressionConverter
    {
        /// <summary>
        /// Convert <see cref="Expression"/> into the <see cref="ActivationData"/> instance.
        /// </summary>
        /// <param name="action">The <see cref="Expression"/> instance.</param>
        /// <returns>The <see cref="ActivationData"/> instance.</returns>
        ActivationData Convert(Expression<Action> action);

        /// <summary>
        /// Find the method that satisfy parameters
        /// </summary>
        /// <param name="instanceType">Type of object that should contain the method</param>
        /// <param name="methodName">Method's name.</param>
        /// <param name="argumentTypes">List of arguments' types.</param>
        /// <returns>The MethodInfo instance.</returns>
        MethodInfo GetNonOpenMatchingMethod(Type instanceType, string methodName, Type[] argumentTypes);

        /// <summary>
        /// Serialize arguments into the array of string.
        /// </summary>
        /// <param name="arguments">List of arguments.</param>
        /// <returns>Array of string</returns>
        string[] SerializeArguments(IReadOnlyCollection<object> arguments);

        /// <summary>
        /// Deserialize arguments using method info and serialized arguments.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo instance.</param>
        /// <param name="serializedArguments">Array of string that contains serialized arguments.</param>
        /// <returns>List of arguments.</returns>
        object[] DeserializeArguments(MethodInfo methodInfo, string[] serializedArguments);
    }
}
