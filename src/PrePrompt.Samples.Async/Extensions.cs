using System;
using System.Collections.Generic;

namespace PrePrompt.Samples.Async
{
    public static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
        
        public static TResult IfNotNull<T, TResult>(this T obj, Func<T, TResult> f) 
            where T : class
            where TResult : class
        {
            return obj != null ? f(obj) : null;
        }
    }
}
