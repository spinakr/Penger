using System.Runtime.CompilerServices;

namespace Domain;

public class GlobalHelperFunctions
{
    public static object NotNull<T>(T obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException($"Argument with type {typeof(T).Name} cannot be null");
        }
        return obj;
    }

    public static string NotNullOrEmpty(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            throw new ArgumentNullException($"Argument {nameof(str)} cannot be null");
        }
        return str;
    }

    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message);
        }
    }

    public static string Matches(string str, string pattern)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(str, pattern))
        {
            throw new ArgumentException($"Argument {nameof(str)} does not match pattern {pattern}");
        }
        return str;
    }
}