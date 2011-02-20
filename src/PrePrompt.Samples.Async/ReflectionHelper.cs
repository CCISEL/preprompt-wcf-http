using System;
using System.Reflection;
using System.Threading.Tasks;

namespace PrePrompt.Samples.Async
{
    internal static class ReflectionHelper
    {
        private static readonly Type _taskType = typeof(Task);

        public static MethodInfo GetFutureResultGetMethod(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;
            return GetFutureType(type).IfNotNull(futureType => futureType.GetProperty("Result", flags).GetGetMethod());
        }

        public static Type GetFutureResultType(Type type)
        {
            return GetFutureType(type).IfNotNull(futureType => futureType.GetGenericArguments()[0]);
        }

        public static Type GetFutureType(Type type)
        {
            for (; type != _taskType; type = type.BaseType)
            {
                if (type.IsGenericType && type.BaseType == _taskType)
                {
                    return type;
                }
            }
            return null;
        }
    }
}