using GDDL.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GDDL.Util
{
    public static class Optional
    {
        [return: NotNull]
        public static Optional<T> OfNullable<T>([MaybeNull] T value)
            where T : class
        {
            return new Optional<T>(value is null, value);
        }

        [return: NotNull]
        public static Optional<T> OfNullable<T>(T? value)
            where T : struct
        {
            return new Optional<T>(value.HasValue, value.Value);
        }

        [return: NotNull]
        public static Optional<T> Of<T>([NotNull] T value)
        {
            return new Optional<T>(true, value);
        }

        internal static Optional<T> Empty<T>()
        {
            return Optional<T>.Empty;
        }
    }

    public class Optional<T> : IEnumerable<T>, IEquatable<Optional<T>>
    {
        [NotNull]
        public static Optional<T> Empty { get; } = new Optional<T>(false, default);

        [MaybeNull]
        private readonly T _value;

        public bool HasValue { get; }

        public T Value
        {
            get
            {
                if (!HasValue)
                    throw new InvalidOperationException("Can not get the value of an empty Optional.");
                return _value;
            }
        }

        internal protected Optional(bool hasValue, T value)
        {
            HasValue = hasValue;
            _value = value;
        }

        public Optional<R> Map<R>(Func<T, R> mapping)
        {
            if (HasValue)
                return Optional.Of(mapping(_value));
            return Optional<R>.Empty;
        }

        public Optional<R> FlatMap<R>(Func<T, Optional<R>> mapping)
        {
            if (HasValue)
                return mapping(_value);
            return Optional<R>.Empty;
        }

        public Optional<T> Filter(Func<T, bool> mapping)
        {
            if (HasValue && mapping(_value))
                return this;
            return Empty;
        }

        internal T OrElse(T def)
        {
            return HasValue ? _value : def;
        }

        internal T OrElseGet(Func<T> defGetter)
        {
            return HasValue ? _value : defGetter();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (HasValue)
                yield return Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is null || !(obj is Optional<T> other)) return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return HasValue ? HashCode.Combine(true, _value) : HashCode.Combine(false);
        }

        public bool Equals(Optional<T> other)
        {
            if (ReferenceEquals(other, this)) return true;
            if (other is null) return false;
            if (!HasValue) return !other.HasValue;
            return other.HasValue && Equals(_value, other._value);
        }
    }
}
