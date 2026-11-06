using System.Collections;
using System.Reflection;
using Bergdahl.NewsbinPro.Client;

namespace Bergdahl.NewsbinPro.Cli;

/// <summary>Runs any public NewsbinClient method by name, so every call the library supports can be tested.</summary>
internal static class MethodCaller
{
    // Methods that would break the session or the protocol when called on their own.
    private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
    {
        "Dispose", "Connect", "Disconnect", "GetResponse", "SendCommandWithoutResponse",
    };

    private static IEnumerable<MethodInfo> Callable(bool sync) =>
        typeof(NewsbinClient)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Where(m => m.Name.EndsWith("Async", StringComparison.Ordinal) != sync)
            .Where(m => !Excluded.Contains(BaseName(m)))
            .Where(m => m.GetParameters().All(p => p.ParameterType != typeof(Stream)));

    private static string BaseName(MethodInfo method) =>
        method.Name.EndsWith("Async", StringComparison.Ordinal) ? method.Name[..^5] : method.Name;

    public static Task ListAsync(Session session, string[] args, CancellationToken ct)
    {
        string filter = args.Length > 0 ? args[0] : "";
        foreach (MethodInfo method in Callable(sync: true)
                     .Where(m => m.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(m => m.Name).ThenBy(m => m.GetParameters().Length))
        {
            string parameters = string.Join(" ", method.GetParameters().Select(p =>
                p.ParameterType == typeof(string[]) ? $"[{p.Name}...]" : $"<{p.Name}:{TypeName(p.ParameterType)}>"));
            Console.WriteLine($"{method.Name} {parameters}".TrimEnd());
        }
        Console.WriteLine();
        Console.WriteLine("Each method also runs through its ...Async version unless --sync is set.");
        return Task.CompletedTask;
    }

    private static string TypeName(Type type) =>
        type == typeof(Command) ? "Command" : type.Name.ToLowerInvariant() switch
        {
            "int32" => "int",
            "int64" => "long",
            "boolean" => "bool",
            var name => name
        };

    public static async Task CallAsync(Session session, string[] args, CancellationToken ct)
    {
        if (args.Length == 0)
            throw new UsageException("Give a method name. Run 'methods' for the list.");
        string name = args[0];
        if (name.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
            name = name[..^5];
        string[] values = args.Skip(1).ToArray();

        bool sync = session.Options.Sync;
        string target = sync ? name : name + "Async";
        List<MethodInfo> candidates = Callable(sync)
            .Where(m => string.Equals(m.Name, target, StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (candidates.Count == 0)
            throw new UsageException($"No method '{args[0]}'. Run 'methods' for the list.");

        foreach (MethodInfo method in candidates.OrderByDescending(m => m.GetParameters().Length))
        {
            if (!TryBind(method, values, ct, out object?[]? arguments))
                continue;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            object? result = await Invoke(session, method, arguments!);
            watch.Stop();
            Print(result, method);
            Console.Error.WriteLine($"({method.Name}, {watch.ElapsedMilliseconds} ms)");
            return;
        }

        string signatures = string.Join(Environment.NewLine, candidates.Select(m =>
            "  " + name + " " + string.Join(" ", m.GetParameters()
                .Where(p => p.ParameterType != typeof(CancellationToken))
                .Select(p => p.ParameterType == typeof(string[]) ? $"[{p.Name}...]" : $"<{p.Name}:{TypeName(p.ParameterType)}>"))));
        throw new UsageException($"The arguments don't match {name}:{Environment.NewLine}{signatures}");
    }

    private static async Task<object?> Invoke(Session session, MethodInfo method, object?[] arguments)
    {
        try
        {
            if (session.Options.Sync)
                return await Task.Run(() => method.Invoke(session.Client, arguments));

            var task = (Task)method.Invoke(session.Client, arguments)!;
            await task;
            return method.ReturnType.IsGenericType
                ? method.ReturnType.GetProperty("Result")!.GetValue(task)
                : null;
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private static bool TryBind(MethodInfo method, string[] values, CancellationToken ct, out object?[]? arguments)
    {
        ParameterInfo[] parameters = method.GetParameters();
        arguments = new object?[parameters.Length];
        int next = 0;
        for (int i = 0; i < parameters.Length; i++)
        {
            Type type = parameters[i].ParameterType;
            if (type == typeof(CancellationToken))
            {
                arguments[i] = ct;
                continue;
            }
            if (type == typeof(string[]))
            {
                // Takes the rest, for params string[] and the async string[]? arguments.
                arguments[i] = values.Skip(next).ToArray();
                next = values.Length;
                continue;
            }
            if (next >= values.Length)
            {
                if (!parameters[i].HasDefaultValue)
                    return false;
                arguments[i] = parameters[i].DefaultValue;
                continue;
            }
            if (!TryConvert(values[next++], type, out object? value))
                return false;
            arguments[i] = value;
        }
        return next == values.Length;
    }

    private static bool TryConvert(string text, Type type, out object? value)
    {
        value = null;
        if (type == typeof(string))
            value = text;
        else if (type == typeof(int) && int.TryParse(text, out int i))
            value = i;
        else if (type == typeof(long) && long.TryParse(text, out long l))
            value = l;
        else if (type == typeof(bool) && text.ToLowerInvariant() is "true" or "on" or "yes" or "1")
            value = true;
        else if (type == typeof(bool) && text.ToLowerInvariant() is "false" or "off" or "no" or "0")
            value = false;
        else if (type == typeof(Command) && Enum.TryParse(text, ignoreCase: true, out Command command))
            value = command;
        return value != null;
    }

    private static void Print(object? value, MethodInfo method)
    {
        switch (value)
        {
            case null:
                Console.WriteLine("OK");
                break;
            case ListResponse list:
                Console.WriteLine($"{(int)list.StatusCode} {list.StatusCode} {list.Message}");
                foreach (string item in list.ListItems)
                    Console.WriteLine($"  {item.Replace('\t', '|')}");
                break;
            case Response response:
                Console.WriteLine($"{(int)response.StatusCode} {response.StatusCode} {response.Message}");
                break;
            case GogTree tree:
                foreach (string gog in tree.GroupOfGroupNames)
                {
                    Console.WriteLine(gog);
                    foreach (string group in tree.GetGroupOfGroupChildren(gog))
                        Console.WriteLine($"  {group}");
                }
                break;
            case string or bool or int or long:
                Console.WriteLine(value);
                break;
            case IEnumerable items:
                int n = 0;
                foreach (object? item in items)
                {
                    n++;
                    Console.WriteLine($"[{n}] {Describe(item)}");
                }
                Console.WriteLine($"{n} items.");
                break;
            default:
                Console.WriteLine(Describe(value));
                break;
        }
    }

    // Uses ToString when the type overrides it, otherwise lists the public properties.
    private static string Describe(object? value)
    {
        if (value == null)
            return "null";
        Type type = value.GetType();
        if (type.GetMethod(nameof(ToString), Type.EmptyTypes)!.DeclaringType != typeof(object))
            return value.ToString()!;
        return string.Join(Environment.NewLine, type.GetProperties().Select(p =>
        {
            object? v = p.GetValue(value);
            string text = v is IEnumerable e and not string ? string.Join(", ", e.Cast<object>()) : v?.ToString() ?? "";
            return $"{p.Name}: {text}";
        }));
    }
}
