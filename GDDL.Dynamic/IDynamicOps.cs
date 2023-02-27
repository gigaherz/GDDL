using System.Collections;

namespace GDDL.Dynamic;

public interface IDynamicOps
{
}

public interface IDynamicOps<T> : IDynamicOps
{
    #region CONVERT
    TResult ConvertTo<TResult>(IDynamicOps<TResult> to, T value);
    #endregion

    #region ENCODE
    T Empty { get; }

    T EmptyMap => EncodeMap(new Dictionary<object, object>());
    T EmptyList => EncodeList(new List<object>());
    
    T EncodeString(string value);
    T EncodeByte(byte value);
    T EncodeSByte(sbyte value);
    T EncodeShort(short value);
    T EncodeUShort(ushort value);
    T EncodeInt(int value);
    T EncodeUInt(uint value);
    T EncodeLong(long value);
    T EncodeULong(ulong value);
    T EncodeSingle(float value);
    T EncodeDouble(double value);
    
    T EncodeAny(object? obj)
    {
        return obj switch
        {
            null => Empty,
            string s => EncodeString(s),
            byte b => EncodeByte(b),
            sbyte b => EncodeSByte(b),
            short s => EncodeShort(s),
            ushort s => EncodeUShort(s),
            int i => EncodeInt(i),
            uint i => EncodeUInt(i),
            long l => EncodeLong(l),
            ulong l => EncodeULong(l),
            float f => EncodeSingle(f),
            double d => EncodeDouble(d),
            IDictionary d => EncodeMap(d),
            IEnumerable l => EncodeList(l),
            _ => throw new InvalidOperationException("Do not know how to encode value of type " + obj.GetType())
        };
    }

    IMapBuilder CreateMapBuilder();
    IListBuilder CreateListBuilder();

    T EncodeMap(IDictionary dictionary)
    {
        var builder = CreateMapBuilder();
        for(var e = dictionary.GetEnumerator(); e.MoveNext();)
        {
            var kt = EncodeAny(e.Key);
            var kv = EncodeAny(e.Value);
            builder.Add(kt, kv);
        }
        return builder.Build();
    }

    T EncodeList(IEnumerable list)
    {
        var builder = CreateListBuilder();
        foreach (var v in list)
        {
            builder.Add(EncodeAny(v));
        }
        return builder.Build();
    }

    #endregion

    #region DECODE
    Result<IConvertible> DecodeConvertible(T data);
    Result<string> DecodeString(T data);
    Result<byte>   DecodeByte  (T data);
    Result<sbyte>  DecodeSByte (T data);
    Result<short>  DecodeShort (T data);
    Result<ushort> DecodeUShort(T data);
    Result<int>    DecodeInt   (T data);
    Result<uint>   DecodeUInt  (T data);
    Result<long>   DecodeLong  (T data);
    Result<ulong>  DecodeULong (T data);
    Result<float>  DecodeSingle(T data);
    Result<double> DecodeDouble(T data);
    Result<IEnumerable<T>> DecodeList(T data);
    Result<IEnumerable<KeyValuePair<T,T>>> DecodeMap(T data);

    Result<List<TValue>> DecodeList<TValue>(T data, Func<T, TValue> decoder)
    {
        return DecodeList(data).Map(d => d.Select(decoder).ToList());
    }

    Result<Dictionary<TKey, TValue>> DecodeDictionary<TKey, TValue>(T data, Func<T, TKey> keyDecoder, Func<T, TValue> valueDecoder)
        where TKey : notnull
    {
        return DecodeMap(data).Map(dec =>
            dec.ToDictionary(kvp => keyDecoder(kvp.Key), kvp => valueDecoder(kvp.Value)));
    }
    #endregion

    #region BUILDERS
    public interface IMapBuilder
    {
        void Add(T k, T v);
        T Build();
    }

    public interface IListBuilder
    {
        void Add(T v);
        T Build();
    }
    #endregion

    Result<TFormat> MergeToPrimitive<TFormat>(TFormat prefix, TFormat value)
    {
        return Equals(prefix, Empty)
            ? Result.Success(value)
            : Result.Failure("Do not know how to append a primitive value " + value + " to " + prefix, value);
    }
}