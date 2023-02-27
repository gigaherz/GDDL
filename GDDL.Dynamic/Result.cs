using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GDDL.Dynamic;

public static class Result
{
    public static Result<TFirst> Success<TFirst>(TFirst first)
    {
        return new Result<TFirst>(first);
    }
    public static Result<TFirst> Failure<TFirst>(string message, TFirst partialResult)
    {
        return Failure(new ErrorContext<TFirst>(message, partialResult, null, 0, 0));
    }

    public static Result<TFirst> Failure<TFirst>(ErrorContext<TFirst> second)
    {
        return new Result<TFirst>(second);
    }
}

public record ErrorContext<T>(string Message, T partialResult, string? File, int Line, int Column);

public readonly struct Result<TFirst>
{
    private readonly bool isSuccess;
    private readonly object? data;

    internal Result(TFirst first)
    {
        isSuccess = true;
        data = first;
    }

    internal Result(ErrorContext<TFirst> second)
    {
        isSuccess = false;
        data = second;
    }

    private Result(bool isSuccess, object? data)
    {
        this.isSuccess = isSuccess;
        this.data = data;
    }

    public bool IsSuccess => isSuccess;
    public bool IsError => !isSuccess;

    public TFirst First => isSuccess ? ToResult : throw new InvalidOperationException($"Cannot get result value of an erroring Result.");
    public ErrorContext<TFirst> Second => !isSuccess ? ToError : throw new InvalidOperationException($"Cannot get error context of a success Result.");

    public bool TryGetResult(out TFirst value)
    {
        if (isSuccess)
        {
            value = ToResult;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }

    public bool TryGetError(out ErrorContext<TFirst> value)
    {
        if (isSuccess)
        {
            value = default!;
            return false;
        }
        else
        {
            value = ToError;
            return true;
        }
    }

    public Result<T> Map<T>(Func<TFirst, T> mapping)
    {
        return isSuccess ? new Result<T>(isSuccess, mapping(ToResult)) : new Result<T>(false, data);
    }

    public Result<T> FlatMap<T>(Func<TFirst, Result<T>> mapping)
    {
        return isSuccess ? mapping(ToResult) : new Result<T>(false, data);
    }

    public Result<T> MapError<T>(Func<TFirst, T> mapping)
    {
        return isSuccess ? new Result<T>(isSuccess, mapping(ToResult)) : new Result<T>(false, data);
    }

    private TFirst ToResult => Cast<TFirst>();
    private ErrorContext<TFirst> ToError => Cast<ErrorContext<TFirst>>();

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
