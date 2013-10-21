using System;

namespace concord.Extensions
{
    public static class ActionExtensions
    {
        public static bool Try<T>(this Func<T> funcToGet, out T outT)
        {
            outT = default(T);
            try
            {
                outT = funcToGet();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}