using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;

namespace TweenLibSourceGenerator
{
    [Generator]
    public class BaseGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ITweenerSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            const string ERROR_LOG_FILE_PATH = "C:\\Users\\Administrator\\Desktop\\SourceGenErrors.txt";
            const string DEBUG_LOG_FILE_PATH = "C:\\Users\\Administrator\\Desktop\\SourceGenDebugLogs.txt";
            string errorDebugContent = "";

            try
            {
                if (!(context.SyntaxReceiver is ITweenerSyntaxReceiver receiver))
                    return;

                var compilation = context.Compilation;

                foreach (var structDeclaration in receiver.Syntaxes)
                {
                    string tweenerNamespace = this.GetNamespace(structDeclaration);
                    string tweenerName = structDeclaration.Identifier.ToString();

                    // BUG: Potential bug: Unexpected behaviour if ITweener does not stay at the first place in BaseList of concreted Tweener.
                    var genericArguments = ((GenericNameSyntax)structDeclaration.BaseList.Types[0].Type)
                        .TypeArgumentList.Arguments;

                    SemanticModel semanticModel = compilation.GetSemanticModel(genericArguments[0].SyntaxTree);

                    this.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[0]
                        , out string componentTypeName, out string componentNamespace);

                    this.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[1]
                        , out string targetTypeName, out string targetNamespace);

                    this.GeneratePartialPartTweener(context, tweenerName, tweenerNamespace);
                    this.GenerateCanTweenTag(context, tweenerName, componentNamespace);
                    this.GenerateTweenData(context, tweenerName, targetTypeName, targetNamespace, componentNamespace);
                    this.GenerateTweenSystem(context, tweenerName, tweenerNamespace, componentTypeName, componentNamespace);

                }

            }
            catch (Exception e)
            {
                File.AppendAllText(ERROR_LOG_FILE_PATH, $"Source generator error:\n{e}\n");
                File.AppendAllText(ERROR_LOG_FILE_PATH, $"Debug contents:\n{errorDebugContent}\n");
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

        private void GeneratePartialPartTweener(
            GeneratorExecutionContext context
            , string tweenerName
            , string tweenerNamespace)
        {
            string sourceCode = $@"
using System;
namespace {tweenerNamespace}
{{
    public partial struct {tweenerName}
    {{
        [Unity.Collections.ReadOnly]
        public float DeltaTime;
    }}  
}}
";

            context.AddSource($"{tweenerName}.g.cs", sourceCode);

        }

        private void GenerateCanTweenTag(
            GeneratorExecutionContext context
            , string tweenerName
            , string componentNamespace)
        {
            string sourceCode = $@"
namespace {componentNamespace}
{{
    public struct Can_{tweenerName}_TweenTag : Unity.Entities.IComponentData, Unity.Entities.IEnableableComponent
    {{
    }}
}}
";

            context.AddSource($"Can_{tweenerName}_TweenTag.cs", sourceCode);

        }

        private void GenerateTweenData(
            GeneratorExecutionContext context
            , string tweenerName
            , string targetTypeName
            , string targetNamespace
            , string componentNamespace)
        {
            string fullIdentifier = $"{targetNamespace}.{targetTypeName}";

            string sourceCode = $@"
namespace {componentNamespace}
{{
    public struct {tweenerName}_TweenData : Unity.Entities.IComponentData
    {{
        public float LifeTimeSecond;
        public float BaseSpeed;
        public {fullIdentifier} Target;
    }}
}}
";

            context.AddSource($"{tweenerName}_TweenData.cs", sourceCode);

        }

        private void GenerateTweenSystem(
            GeneratorExecutionContext context
            , string tweenerName
            , string tweenerNamespace
            , string componentTypeName
            , string componentNamespace)
        {
            string tweenerIdentifier = $"{tweenerNamespace}.{tweenerName}";
            string componentIdentifier = $"{componentNamespace}.{componentTypeName}";
            string canTweenTagIdentifier = $"{componentNamespace}.Can_{tweenerName}_TweenTag";
            string tweenDataIdentifier = $"{componentNamespace}.{tweenerName}_TweenData";

            string systemName = $"{tweenerName}_TweenSystem";

            string sourceCode = $@"
using Unity.Entities;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using TweenLib.Utilities.Extensions;

namespace TweenLib.Systems
{{
    [UpdateInGroup(typeof(TweenLib.TweenSystemGroup))]
    [Unity.Burst.BurstCompile]
    public partial struct {systemName} : ISystem
    {{
        private EntityQuery query;
        private ComponentTypeHandle<{componentIdentifier}> componentTypeHandle;
        private ComponentTypeHandle<{canTweenTagIdentifier}> canTweenTagTypeHandle;
        private ComponentTypeHandle<{tweenDataIdentifier}> tweenDataTypeHandle;

        [Unity.Burst.BurstCompile]
        public void OnCreate(ref Unity.Entities.SystemState state)
        {{
            EntityQueryBuilder queryBuilder = new EntityQueryBuilder(Allocator.Temp);

            this.query = queryBuilder
                .WithAllRW<{componentIdentifier}>()
                .WithAllRW<{tweenDataIdentifier}>()
                .WithAll<{canTweenTagIdentifier}>()
                .Build(ref state);

            queryBuilder.Dispose();

            this.componentTypeHandle = state.GetComponentTypeHandle<{componentIdentifier}>(false);
            this.canTweenTagTypeHandle = state.GetComponentTypeHandle<{canTweenTagIdentifier}>(false);
            this.tweenDataTypeHandle = state.GetComponentTypeHandle<{tweenDataIdentifier}>(false);

            state.RequireForUpdate(this.query);
        }}

        [Unity.Burst.BurstCompile]
        public void OnUpdate(ref SystemState state)
        {{
            this.componentTypeHandle.Update(ref state);
            this.canTweenTagTypeHandle.Update(ref state);
            this.tweenDataTypeHandle.Update(ref state);

            state.Dependency = new TweenIJC
            {{
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime,
                ComponentTypeHandle = this.componentTypeHandle,
                CanTweenTagTypeHandle = this.canTweenTagTypeHandle,
                TweenDataTypeHandle = this.tweenDataTypeHandle,
            }}.ScheduleParallel(this.query, state.Dependency);
                
        }}

 
        [Unity.Burst.BurstCompile]
        public struct TweenIJC : IJobChunk
        {{
            [Unity.Collections.ReadOnly] public float DeltaTime;
            public ComponentTypeHandle<{componentIdentifier}> ComponentTypeHandle;
            public ComponentTypeHandle<{canTweenTagIdentifier}> CanTweenTagTypeHandle;
            public ComponentTypeHandle<{tweenDataIdentifier}> TweenDataTypeHandle;

            [Unity.Burst.BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {{
                var canTweenTagEnabledMask_RW = chunk.GetEnabledMask(ref this.CanTweenTagTypeHandle);
                var componentArray = chunk.GetNativeArray(ref this.ComponentTypeHandle);
                var tweenDataArray = chunk.GetNativeArray(ref this.TweenDataTypeHandle);

                var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

                while (enumerator.NextEntityIndex(out var i))
                {{
                    ref var component = ref componentArray.ElementAt(i);
                    ref var tweenData = ref tweenDataArray.ElementAt(i);
                    var canTweenTag = canTweenTagEnabledMask_RW.GetEnabledRefRW<{canTweenTagIdentifier}>(i);

                    var tweener = new global::{tweenerIdentifier}
                    {{
                        DeltaTime = this.DeltaTime,
                    }};

                    if (tweener.CanStop(in component, in tweenData.LifeTimeSecond, in tweenData.BaseSpeed, in tweenData.Target))
                    {{
                        canTweenTag.ValueRW = false;
                        tweenData.LifeTimeSecond = 0f;
                        continue;
                    }}

                    tweener.Tween(ref component, in tweenData.BaseSpeed, in tweenData.Target);
                    tweenData.LifeTimeSecond += this.DeltaTime;

                }}

            }}

        }}

    }}

}}
";

            context.AddSource($"{systemName}.g.cs", sourceCode);

        }

    }

}