namespace Expanz.Extensions.BCL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    
    public static class EnumerableExtension
    {
        [DebuggerStepThrough]
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }
        
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}