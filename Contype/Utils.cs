internal static class Utils
{
    public static T ChooseFrom<T>(this Random random, IReadOnlyList<T> source)
    {
        return source[random.Next(source.Count)];
    }

    public static T RemoveFrom<T>(this Random random, IList<T> source)
    {
        var index = random.Next(source.Count);
        var item = source[index];
        source.RemoveAt(index);
        return item;
    }

    public static SpanSplitEnumerable Split(this ReadOnlySpan<char> span, char separator)
    {
        return new(span, separator);
    }

    public readonly ref struct SpanSplitEnumerable
    {
        private readonly ReadOnlySpan<char> span;
        private readonly char separator;

        public SpanSplitEnumerable(ReadOnlySpan<char> span, char separator)
        {
            this.span = span;
            this.separator = separator;
        }

        public SpanSplitEnumerator GetEnumerator()
        {
            return new(span, separator);
        }
    }

    public ref struct SpanSplitEnumerator
    {
        private readonly char separator;
        private ReadOnlySpan<char> remainingSpan;
        private bool isEnded;

        public SpanSplitEnumerator(ReadOnlySpan<char> span, char separator)
        {
            this.separator = separator;
            remainingSpan = span;
            Current = default;
            isEnded = false;
        }

        public ReadOnlySpan<char> Current { get; private set; }

        public bool MoveNext()
        {
            if (isEnded) 
                return false;

            var index = remainingSpan.IndexOf(separator);
            if (index == -1)
            {
                Current = remainingSpan;
                remainingSpan = default;
                isEnded = true;
            }
            else
            {
                Current = remainingSpan[..index];
                remainingSpan = remainingSpan[(index + 1)..];
            }

            return true;
        }
    }
}
