using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TweenLibSourceGenerator
{
    public static class Utilities
    {
        public static string GetNamespace(SyntaxNode syntaxNode)
        {
            SyntaxNode parent = syntaxNode.Parent;

            while (parent != null)
            {
                if (parent is NamespaceDeclarationSyntax namespaceDeclaration)
                    return namespaceDeclaration.Name.ToString();

                parent = parent.Parent;

            }

            return null;

        }

        public static void GetNameAndNamespaceOfGenericArgument(
            SemanticModel semanticModel
            , ExpressionSyntax expressionSyntax
            , out string typeName
            , out string namespaceName)
        {
            ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(expressionSyntax).Type;
            if (typeSymbol != null)
            {
                typeName = typeSymbol.Name;
                namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString() ?? "(NoNamespace)";
                return;
            }

            throw new System.Exception($"Can not resolve {nameof(ITypeSymbol)} for Generic argument");

        }

    }

}
