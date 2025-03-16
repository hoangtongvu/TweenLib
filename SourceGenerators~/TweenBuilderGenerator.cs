using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;

namespace TweenLibSourceGenerator
{
    [Generator]
    public class TweenBuilderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ITweenerSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            const string LOG_FILE_PATH = "C:\\Users\\Administrator\\Desktop\\SourceGenErrors.txt";

            try
            {
                if (!(context.SyntaxReceiver is ITweenerSyntaxReceiver receiver))
                    return;

                var compilation = context.Compilation;

                foreach (var structDeclaration in receiver.Syntaxes)
                {
                    string tweenerNamespace = this.GetNamespace(structDeclaration);
                    string tweenerName = structDeclaration.Identifier.ToString();

                    var genericArguments = ((GenericNameSyntax)structDeclaration.BaseList.Types[0].Type) // BUG: This only works if tweener implement only one interface
                        .TypeArgumentList.Arguments;

                    SemanticModel semanticModel = compilation.GetSemanticModel(genericArguments[0].SyntaxTree);

                    this.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[0]
                        , out string componentTypeName, out string componentNamespace);

                    this.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[1]
                        , out string targetTypeName, out string targetNamespace);


                    this.GenerateTweenBuilder(context, tweenerName, tweenerNamespace, targetTypeName, targetNamespace, componentNamespace);

                }

            }
            catch (Exception e)
            {
                File.AppendAllText(LOG_FILE_PATH, $"Source generator error:\n{e}\n");
            }
            
        }

        private string GetNamespace(SyntaxNode syntaxNode)
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

        private void GetNameAndNamespaceOfGenericArgument(
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

        private void GenerateTweenBuilder(
            GeneratorExecutionContext context
            , string tweenerName
            , string tweenerNamespace
            , string targetTypeName
            , string targetNamespace
            , string componentNamespace)
        {
            string targetIdentifier = $"{targetNamespace}.{targetTypeName}";
            string canTweenTagIdentifier = $"{componentNamespace}.Can_{tweenerName}_TweenTag";
            string tweenDataIdentifier = $"{componentNamespace}.{tweenerName}_TweenData";

            string sourceCode = $@"
using System;
using Unity.Entities;
using TweenLib;

namespace {tweenerNamespace}
{{
    public partial struct {tweenerName}
    {{
        [Unity.Burst.BurstCompile]
        public struct TweenBuilder : ITweenBuilder<{targetIdentifier}, {canTweenTagIdentifier}, {tweenDataIdentifier}>
        {{
            private float baseSpeed;
            private {targetIdentifier} target;

            public static TweenBuilder Create() => new();

            [Unity.Burst.BurstCompile]
            public TweenBuilder WithBaseSpeed(in float baseSpeed)
            {{
                this.baseSpeed = baseSpeed;
                return this;
            }}

            [Unity.Burst.BurstCompile]
            public TweenBuilder WithTarget(in {targetIdentifier} target)
            {{
                this.target = target;
                return this;
            }}

            [Unity.Burst.BurstCompile]
            public void Build(ref {tweenDataIdentifier} tweenData, in EnabledRefRW<{canTweenTagIdentifier}> canTweenTag)
            {{
                tweenData.BaseSpeed = this.baseSpeed;
                tweenData.Target = this.target;
                canTweenTag.ValueRW = true;

            }}

        }}            

    }}  
}}
";

            context.AddSource($"{tweenerName}.g.cs", sourceCode);

        }

    }

}