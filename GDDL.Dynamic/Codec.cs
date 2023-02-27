namespace GDDL.Dynamic;

public interface ICodec
{
    public static readonly ICodec<int> Int = new IntCodec();
}

public interface ICodec<T> : ICodec
{
   Result<TFormat> Encode<TFormat>(IDynamicOps<TFormat> ops, T value, TFormat prefix);
   Result<(T, TFormat)> Decode<TFormat>(IDynamicOps<TFormat> ops, TFormat value);
}

public abstract class PrimitiveCodec<T> : ICodec<T>
        where T : struct
{
    public abstract TFormat EncodeSimple<TFormat>(IDynamicOps<TFormat> ops, T value);
    public abstract Result<T> DecodeSimple<TFormat>(IDynamicOps<TFormat> ops, TFormat input);

    Result<TFormat> ICodec<T>.Encode<TFormat>(IDynamicOps<TFormat> ops, T value, TFormat prefix)
    {
        return ops.MergeToPrimitive(prefix, EncodeSimple(ops, value));
    }
    Result<(T, TFormat)> ICodec<T>.Decode<TFormat>(IDynamicOps<TFormat> ops, TFormat value)
    {
        return DecodeSimple(ops, value).Map(r => (r, ops.Empty));
    }
}

public class IntCodec : PrimitiveCodec<int>
{
    public override TFormat EncodeSimple<TFormat>(IDynamicOps<TFormat> ops, int value)
    {
        return ops.EncodeInt(value);
    }

    public override Result<int> DecodeSimple<TFormat>(IDynamicOps<TFormat> ops, TFormat input)
    {
        return ops.DecodeInt(input);
    }
}

public class TupleCodec<T1, T2> : ICodec<Tuple<T1, T2>>
{
    private readonly ICodec<T1> codec1;
    private readonly ICodec<T2> codec2;

    public TupleCodec(ICodec<T1> codec1, ICodec<T2> codec2)
    {
        this.codec1 = codec1;
        this.codec2 = codec2;
    }

    public Result<TFormat> Encode<TFormat>(IDynamicOps<TFormat> ops, Tuple<T1, T2> value, TFormat prefix)
    {
        return codec2.Encode(ops, value.Item2, prefix).FlatMap(f=>codec1.Encode(ops, value.Item1, f));
    }

    public Result<(Tuple<T1, T2>, TFormat)> Decode<TFormat>(IDynamicOps<TFormat> ops, TFormat value)
    {
        return codec1.Decode(ops, value).FlatMap(p1=>
                codec2.Decode(ops, p1.Item2).Map(p2=>
                    (Tuple.Create(p1.Item1, p2.Item1), p2.Item2)
            )
        );
    }
}