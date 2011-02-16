using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrePROMPT.Samples.Content
{
    public static class HandyExtensions
    {
        public static T With<T>(this T t, Action<T> action)
        {
            action(t);
            return t;
        }
    }
}
