using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

class TextProvider
{
    private readonly Random random = new();
    private readonly List<XElement> documentationElements;

    public TextProvider()
    {
        var allPaths = Directory.GetFiles(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.2\ref\net6.0", "*.xml", SearchOption.AllDirectories);
        var document = XDocument.Load(random.ChooseFrom(allPaths));

        documentationElements = (
            from members in document.Root!.Element("members")!.Elements("member")
            from element in members.Elements()
            where GetInnerParts(element).Any()
            select element).ToList();
    }

    private static IEnumerable<XNode> GetInnerParts(XElement element)
    {
        return element.Nodes().Where(node =>
            node is XElement
            || node is XText text && !string.IsNullOrWhiteSpace(text.Value));
    }

    public string GetNextParagraph()
    {
        var text = new StringBuilder();

        VisitElement(random.RemoveFrom(documentationElements));

        void VisitElement(XElement element)
        {
            var innerParts = GetInnerParts(element);

            if (innerParts.Any())
            {
                foreach (var node in innerParts)
                {
                    if (node is XElement childElement)
                    {
                        VisitElement(childElement);
                    }
                    else if (node is XText childText)
                    {
                        AppendProse(childText.Value);
                    }
                }
            }
            else if (element.Name.LocalName is "see" or "seealso")
            {
                if ((element.Attribute("langword")?.Value
                    ?? element.Attribute("href")?.Value) is { } verbatim)
                {
                    text.Append(verbatim);
                }
                else if (element.Attribute("cref")?.Value is { } cref)
                {
                    AppendCref(Cref.Parse(cref));
                }
            }
            else if (element.Name.LocalName is "paramref" or "typeparamref")
            {
                if (element.Attribute("name")?.Value is { } verbatim)
                {
                    text.Append(verbatim);
                }
            }
            else throw new NotImplementedException();
        }

        void AppendCref(Cref cref)
        {
            switch (cref)
            {
                case NamespaceCref @namespace:
                    text.Append(@namespace.Name);
                    break;

                case TypeCref type:
                    AppendCref(type);
                    break;

                case MemberCref member:
                    AppendCref(member.Type);
                    text.Append('.').Append(member.Name);

                    if (member.Signature is { } signature)
                    {
                        text.Append('(');

                        for (var i = 0; i < signature.Length; i++)
                        {
                            if (i != 0) text.Append(", ");
                            AppendCref(signature[i]);
                        }

                        text.Append(')');
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }

            void AppendCref(TypeCref type)
            {
                text.Append(type.Name);

                if (type.TypeParameters is { Length: not 0 } typeParameters)
                {
                    text.Append('<');

                    for (var i = 0; i < typeParameters.Length; i++)
                    {
                        if (i != 0) text.Append(", ");
                        text.Append(typeParameters[i]);
                    }

                    text.Append('>');
                }
                else if (type.Arity != 0)
                {
                    text.Append('<');

                    for (var i = type.Arity - 2; i >= 0; i--)
                        text.Append(',');

                    text.Append('>');
                }
            }
        }

        void AppendProse(string prose)
        {
            text.Append(Regex.Replace(prose, @"\s+", " "));
        }

        return text.ToString();
    }
}
