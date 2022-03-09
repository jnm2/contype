using System.Collections.Immutable;
using System.Globalization;

internal abstract record Cref
{
    public static Cref Parse(ReadOnlySpan<char> serializedCref)
    {
        var kindSeparator = serializedCref.IndexOf(':');
        if (kindSeparator == -1)
            throw new ArgumentException("Cref must start with a kind followed by a colon.", nameof(serializedCref));

        var kind = serializedCref[..kindSeparator];
        var remainder = serializedCref[(kindSeparator + 1)..];

        if (kind.Equals("N", StringComparison.OrdinalIgnoreCase))
        {
            return new NamespaceCref(remainder.ToString());
        }

        if (kind.Equals("T", StringComparison.OrdinalIgnoreCase))
        {
            return ParseTypeCref(remainder);
        }

        if (kind.Equals("P", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("E", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("F", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("M", StringComparison.OrdinalIgnoreCase)
            || kind.Equals("Override", StringComparison.OrdinalIgnoreCase))
        {
            return ParseMemberCref(remainder);
        }

        throw new NotImplementedException();
    }

    private static MemberCref ParseMemberCref(ReadOnlySpan<char> text)
    {
        var signature = (ImmutableArray<TypeCref>.Builder?)null;
        if (text[^1] == ')')
        {
            signature = ImmutableArray.CreateBuilder<TypeCref>();

            var paramSeparatorIndex = text.LastIndexOf('(');
            if (paramSeparatorIndex == -1)
                throw new ArgumentException("Unmatched ')'", nameof(text));

            var parametersText = text[(paramSeparatorIndex + 1)..^1];

            foreach (var parameterText in parametersText.Split(','))
                signature.Add(ParseTypeCref(parameterText.Trim()));

            text = text[..paramSeparatorIndex];
        }

        var nameSeparatorIndex = text.LastIndexOf('.');
        if (nameSeparatorIndex == -1)
            throw new ArgumentException("Member cref with no type", nameof(text));

        var type = ParseTypeCref(text[..nameSeparatorIndex]);
        var name = text[(nameSeparatorIndex + 1)..].ToString();

        return new MemberCref(type, name, signature?.ToImmutable());
    }

    private static TypeCref ParseTypeCref(ReadOnlySpan<char> text)
    {
        var typeParameters = (ImmutableArray<string>.Builder?)null;
        if (text[^1] == '}')
        {
            typeParameters = ImmutableArray.CreateBuilder<string>();

            var typeParamSeparatorIndex = text.LastIndexOf('{');
            if (typeParamSeparatorIndex == -1)
                throw new ArgumentException("Unmatched '}'", nameof(text));

            var typeParametersText = text[(typeParamSeparatorIndex + 1)..^1];

            foreach (var typeParameterText in typeParametersText.Split(','))
                typeParameters.Add(typeParameterText.Trim().ToString());

            text = text[..typeParamSeparatorIndex];
        }

        var arity = 0;
        var aritySeparatorIndex = text.LastIndexOf('`');
        if (aritySeparatorIndex != -1)
        {
            arity = int.Parse(text[(aritySeparatorIndex + 1)..], NumberStyles.None, CultureInfo.InvariantCulture);
            text = text[..aritySeparatorIndex];
        }

        var @namespace = (NamespaceCref?)null;
        var namespaceSeparatorIndex = text.LastIndexOf('.');
        if (namespaceSeparatorIndex != -1)
        {
            @namespace = new(text[..namespaceSeparatorIndex].ToString());
            text = text[(namespaceSeparatorIndex + 1)..];
        }
        
        return new TypeCref(@namespace, text.ToString(), arity, typeParameters?.ToImmutable());
    }
}

internal sealed record NamespaceCref(string Name) : Cref;

internal sealed record TypeCref(
    NamespaceCref? Namespace,
    string Name,
    int Arity,
    ImmutableArray<string>? TypeParameters) : Cref;

internal sealed record MemberCref(
    TypeCref Type,
    string Name,
    ImmutableArray<TypeCref>? Signature) : Cref;
