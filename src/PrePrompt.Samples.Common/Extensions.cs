using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PrePrompt.Samples.Common
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

        public static bool HasAttribute<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            var attributes = member.GetCustomAttributes(typeof(TAttribute), true);
            return attributes.Length > 0;
        }

        public static TAttribute Attribute<TAttribute>(this MemberInfo member)
            where TAttribute : Attribute
        {
            return (TAttribute)member.GetCustomAttributes(typeof(TAttribute), true).SingleOrDefault();
        }

        public static int CompareToIgnoreCase(this string source, string other)
        {
            return string.Compare(source, other, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
