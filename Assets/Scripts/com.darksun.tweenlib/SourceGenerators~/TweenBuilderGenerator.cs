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
                    string tweenerNamespace = Utilities.GetNamespace(structDeclaration);
                    string tweenerName = structDeclaration.Identifier.ToString();

                    var genericArguments = ((GenericNameSyntax)structDeclaration.BaseList.Types[0].Type) // BUG: This only works if tweener implement only one interface
                        .TypeArgumentList.Arguments;

                    SemanticModel semanticModel = compilation.GetSemanticModel(genericArguments[0].SyntaxTree);

                    Utilities.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[0]
                        , out string componentTypeName, out string componentNamespace);

                    Utilities.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[1]
                        , out string targetTypeName, out string targetNamespace);

                    this.GenerateTweenBuilder(context, tweenerName, tweenerNamespace, targetTypeName, targetNamespace, componentNamespace);

                }

            }
            catch (Exception e)
            {
                File.AppendAllText(LOG_FILE_PATH, $"Source generator error:\n{e}\n");
            }
            
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
using Unity.Burst;
using Unity.Entities;
using TweenLib;
using TweenLib.Commons;
using TweenLib.Utilities;

namespace {tweenerNamespace}
{{
    public partial struct {tweenerName}
    {{
        [BurstCompile]
        public struct TweenBuilder : ITweenBuilder<{targetIdentifier}, {canTweenTagIdentifier}, {tweenDataIdentifier}>
        {{
            private {tweenDataIdentifier} tweenData;

            public static TweenBuilder Create(float durationSeconds, in {targetIdentifier} target) => new(durationSeconds, in target);

            public TweenBuilder(float durationSeconds, in {targetIdentifier} target)
            {{
                this.tweenData = new()
                {{
                    TweenTimer = new()
                    {{
                        DurationSeconds = durationSeconds,
                        LoopCounter = 1,
                        LoopCountLimit = 1,
                    }},
                    Target = target,
                    EasingType = EasingType.Linear,
                }};
                
            }}

            [BurstCompile]
            public TweenBuilder WithStartValue(in {targetIdentifier} startValue)
            {{
	            this.tweenData.StartValue = startValue;
	            this.tweenData.UseCustomStartValue = true;
                return this;
            }}

            [BurstCompile]
            public TweenBuilder WithEase(EasingType easingType)
            {{
	            this.tweenData.EasingType = easingType;
                return this;
            }}

            [BurstCompile]
            public TweenBuilder WithLoops(LoopType loopType, byte loopCount = byte.MinValue)
            {{
	            this.tweenData.TweenTimer.LoopCountLimit = loopCount;
	            this.tweenData.TweenTimer.LoopType = loopType;
                return this;
            }}

            [BurstCompile]
            public void Build(
                ref {tweenDataIdentifier} tweenData
                , in EnabledRefRW<{canTweenTagIdentifier}> canTweenTag)
            {{
                tweenData = this.tweenData;
                canTweenTag.ValueRW = true;
            }}

        }}

    }}

}}
";

            context.AddSource($"{tweenerName}.TweenBuilder.g.cs", sourceCode);

        }

    }

}