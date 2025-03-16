using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace TweenLibSourceGenerator
{
    public class ITweenerSyntaxReceiver : ISyntaxReceiver
    {
        public List<StructDeclarationSyntax> Syntaxes { get; } = new List<StructDeclarationSyntax>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is StructDeclarationSyntax structDeclaration)
            {
                if (!this.IsPartialStruct(structDeclaration)) return;
                if (!this.ImplementsITweenerInterface(structDeclaration)) return;

                this.Syntaxes.Add(structDeclaration);
            }
                
        }

        private bool ImplementsITweenerInterface(StructDeclarationSyntax structDeclaration)
        {
            if (structDeclaration.BaseList == null) return false;

            return structDeclaration.BaseList.Types
                .Any(baseType =>
                {
                    if (baseType.Type is GenericNameSyntax genericName)
                    {
                        return genericName.Identifier.Text == "ITweener" &&
                            genericName.TypeArgumentList.Arguments.Count == 2;
                    }

                    return false;
                });

        }

        private bool IsPartialStruct(StructDeclarationSyntax structDeclaration)
        {
            return structDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword));
        }

    }


}