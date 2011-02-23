using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Net.Http;

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

        public static StringBuilder RemoveLastCharacter(this StringBuilder builder, int numCharacters = 1)
        {
            return builder.Remove(builder.Length - numCharacters, numCharacters);
        }

        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static string FormatWith(this string source, params object[] args)
        {
            return string.Format(source, args);
        }

        public static string ToBase64(this string source)
        {
            var ms = new MemoryStream();
            new BinaryFormatter().Serialize(ms, source);
            ms.Seek(0, 0);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }

        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            var copy = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content,
                Version = request.Version
            };

            foreach (var header in request.Headers)
            {
                foreach (var value in header.Value)
                {
                    copy.Headers.AddWithoutValidation(header.Key, value);    
                }
            }

            return copy;
        }
    }
}
