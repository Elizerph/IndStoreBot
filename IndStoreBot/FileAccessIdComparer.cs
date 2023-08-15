using System.Diagnostics.CodeAnalysis;

namespace IndStoreBot
{
    public class FileAccessIdComparer : IEqualityComparer<string>
    {
        public static IEqualityComparer<string> Intance { get; } = new FileAccessIdComparer();

        public bool Equals(string? x, string? y)
        {
            if (x == null || y == null)
                return false;

            return string.Equals(Transform(x), Transform(y));
        }

        private string Transform(string input)
        {
            var parts = input.TrimStart('/').Split('.');
            if (parts.Length >= 1)
                return parts[0];
            throw new InvalidOperationException($"Cannot transform {input}");
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return Transform(obj).GetHashCode();
        }
    }
}
