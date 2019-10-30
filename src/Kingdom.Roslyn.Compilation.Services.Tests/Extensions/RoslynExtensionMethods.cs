using System;

namespace Kingdom.Roslyn.Compilation
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

    // TODO: TBD: could refactor these to a more suitable place...
    public static class RoslynExtensionMethods
    {
        public static IdentifierNameSyntax ToAttributeIdentifierName<TAttribute>(this object _, bool truncate = false)
            where TAttribute : Attribute
        {
            string TruncateTypeName(string name, string suffix)
            {
                return name.EndsWith(suffix)
                    ? name.Substring(0, name.Length - suffix.Length)
                    : name;
            }

            return IdentifierName(TruncateTypeName(typeof(TAttribute).Name, nameof(System.Attribute)));
        }

        public static LiteralExpressionSyntax ToLiteralExpression<T>(this T value)
        {
            if (value is string s)
            {
                return LiteralExpression(StringLiteralExpression, Literal(s));
            }

            throw new ArgumentException($"Type `{typeof(T)}´ Literal Expression is unsupported.");
        }
    }
}
