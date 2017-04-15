using System.Linq.Expressions;

namespace DotNetCraft.WiseQueue.Core.Caching
{
    /// <summary>
    /// Interface shows that <c>object</c> is a cache expression tree compiler.
    /// </summary>
    public interface ICachedExpressionCompiler
    {
        /// <summary>
        /// Get an <c>expression</c> result.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> instance.</param>
        /// <returns>The value.</returns>
        object GetValue(Expression expression);
    }
}
