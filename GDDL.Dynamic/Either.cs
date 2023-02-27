using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GDDL.Dynamic;

public static class Either
{
    public static Either<TFirst, TSecond> First<TFirst, TSecond>(TFirst first)
    {
        return new Either<TFirst, TSecond>(first);
    }
    public static Either<TFirst, TSecond> Second<TFirst, TSecond>(TSecond second)
    {
        return new Either<TFirst, TSecond>(second);
    }
}

public readonly struct Either<TFirst, TSecond>
{
    private readonly bool isFirst;
    private readonly object? data;

    internal Either(TFirst first)
    {
        isFirst = true;
        data = first;
    }

    internal Either(TSecond second)
    {
        isFirst = false;
        data = second;
    }

    private Either(bool isFirst, object? data)
    {
        this.isFirst = isFirst;
        this.data = data;
    }

    public bool IsFirst => isFirst;
    public bool IsSecond => !isFirst;

    public TFirst First => isFirst ? ToFirst : throw new InvalidOperationException($"Cannot get {nameof(First)} value of a non-first Either.");
    public TSecond Second => !isFirst ? ToSecond : throw new InvalidOperationException($"Cannot get {nameof(Second)} value of a non-first Either.");

    #region helpers
    public bool TryGetFirst(out TFirst value)
    {
        if (isFirst)
        {
            value = ToFirst;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }

    public bool TryGetSecond(out TSecond value)
    {
        if (isFirst)
        {
            value = default!;
            return false;
        }
        else
        {
            value = ToSecond;
            return true;
        }
    }

    public Either<T, TSecond> MapFirst<T>(Func<TFirst, T> mapping)
    {
        return isFirst ? new Either<T, TSecond>(isFirst, mapping(ToFirst)) : new Either<T, TSecond>(false, data);
    }

    public Either<TFirst, T> MapSecond<T>(Func<TSecond, T> mapping)
    {
        return isFirst ? new Either<TFirst, T>(isFirst, data) : new Either<TFirst, T>(mapping(ToSecond));
    }

    public Either<T1, T2> Map<T1, T2>(Func<TFirst, T1> mappingFirst, Func<TSecond, T2> mappingSecond)
    {
        return isFirst ? new Either<T1, T2>(isFirst, mappingFirst(ToFirst)) : new Either<T1, T2>(mappingSecond(ToSecond));
    }

    public T MapEither<T>(Func<TFirst, T> mappingFirst, Func<TSecond, T> mappingSecond)
    {
        return isFirst ? mappingFirst(ToFirst) : mappingSecond(ToSecond);
    }
    #endregion

    private TFirst ToFirst => Cast<TFirst>();
    private TSecond ToSecond => Cast<TSecond>();

    // Because the values are assigned from a constructor with matching nullability, data will NOT contain null if the type doesn't allow it.
    private T Cast<T>()
    {
#pragma warning disable CS8603
#pragma warning disable CS8600
        return (T)data;
#pragma warning restore CS8600
#pragma warning restore CS8603
    }
}
