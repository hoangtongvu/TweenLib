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

                    this.GenerateCanTweenTag(context, tweenerName, componentNamespace);
                    this.GenerateTweenData(context, tweenerName, targetTypeName, targetNamespace, componentNamespace);
                    this.GenerateTweenComponentsBakingHelper(context, tweenerName, tweenerNamespace, componentNamespace);
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
using TweenLib;

namespace {componentNamespace}
{{
    public struct {tweenerName}_TweenData : Unity.Entities.IComponentData
    {{
        public int TimerId;
        public float DurationSeconds;
        public {fullIdentifier} Target;

        public bool StartValueInitialized;
        public bool UseCustomStartValue;
        public {fullIdentifier} StartValue;

        public EasingType EasingType;
    }}
}}
";

            context.AddSource($"{tweenerName}_TweenData.cs", sourceCode);

        }

        private void GenerateTweenComponentsBakingHelper(
            GeneratorExecutionContext context
            , string tweenerName
            , string tweenerNamespace
            , string componentNamespace)
        {
            string sourceCode = $@"
using Unity.Entities;
using {componentNamespace};
using TweenLib;

namespace {tweenerNamespace}
{{
    public partial struct {tweenerName}
    {{
        public static void AddTweenComponents(IBaker baker, Entity entity)
        {{
            baker.AddComponent<Can_{tweenerName}_TweenTag>(entity);
            baker.SetComponentEnabled<Can_{tweenerName}_TweenTag>(entity, false);
            baker.AddComponent<{tweenerName}_TweenData>(entity);
        }}
    }}
}}
";

            context.AddSource($"{tweenerName}.ComponentsBakingHelper.cs", sourceCode);

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
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using TweenLib.Utilities.Extensions;
using TweenLib.Timer.Data;
using TweenLib.Timer.Logic;

namespace TweenLib.Systems
{{
    [UpdateInGroup(typeof(TweenLib.TweenSystemGroup))]
    [BurstCompile]
    public partial struct {systemName} : ISystem
    {{
        private EntityQuery query;
        private EntityQuery timerQuery;
        private ComponentTypeHandle<{componentIdentifier}> componentTypeHandle;
        private ComponentTypeHandle<{canTweenTagIdentifier}> canTweenTagTypeHandle;
        private ComponentTypeHandle<{tweenDataIdentifier}> tweenDataTypeHandle;

        [BurstCompile]
        public void OnCreate(ref Unity.Entities.SystemState state)
        {{
            var queryBuilder = new EntityQueryBuilder(Allocator.Temp);
            this.query = queryBuilder
                .WithAllRW<{componentIdentifier}>()
                .WithAllRW<{tweenDataIdentifier}>()
                .WithAll<{canTweenTagIdentifier}>()
                .Build(ref state);

            queryBuilder = new EntityQueryBuilder(Allocator.Temp);
            this.timerQuery = queryBuilder
                .WithAllRW<TimerList>()
                .WithAllRW<TimerIdPool>()
                .Build(ref state);

            this.componentTypeHandle = state.GetComponentTypeHandle<{componentIdentifier}>(false);
            this.canTweenTagTypeHandle = state.GetComponentTypeHandle<{canTweenTagIdentifier}>(false);
            this.tweenDataTypeHandle = state.GetComponentTypeHandle<{tweenDataIdentifier}>(false);

            state.RequireForUpdate(this.query);
            state.RequireForUpdate(this.timerQuery);
        }}

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {{
            this.componentTypeHandle.Update(ref state);
            this.canTweenTagTypeHandle.Update(ref state);
            this.tweenDataTypeHandle.Update(ref state);

            TimerHelper.CompleteDependencesBeforeRW(state.EntityManager);

            state.Dependency = new TweenIJC
            {{
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime,
                TimerList = this.timerQuery.GetSingleton<TimerList>(),
                TimerIdPool = this.timerQuery.GetSingleton<TimerIdPool>(),
                ComponentTypeHandle = this.componentTypeHandle,
                CanTweenTagTypeHandle = this.canTweenTagTypeHandle,
                TweenDataTypeHandle = this.tweenDataTypeHandle,
            }}.ScheduleParallel(this.query, state.Dependency);
                
        }}

 
        [BurstCompile]
        public struct TweenIJC : IJobChunk
        {{
            [Unity.Collections.ReadOnly] public float DeltaTime;
            [NativeDisableParallelForRestriction] public TimerList TimerList;
            [NativeDisableParallelForRestriction] public TimerIdPool TimerIdPool;

            public ComponentTypeHandle<{componentIdentifier}> ComponentTypeHandle;
            public ComponentTypeHandle<{canTweenTagIdentifier}> CanTweenTagTypeHandle;
            public ComponentTypeHandle<{tweenDataIdentifier}> TweenDataTypeHandle;

            [BurstCompile]
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

                    var tweener = new global::{tweenerIdentifier}();

                    var timeCounterSeconds = this.TimerList.Value[tweenData.TimerId];
                    if (timeCounterSeconds.Counter >= tweenData.DurationSeconds)
                    {{
                        // Stop tweening
                        TimerHelper.RemoveTimer(in this.TimerList, in this.TimerIdPool, in tweenData.TimerId);
                        canTweenTag.ValueRW = false;
                        
                        // Finalize the component on tween stop
                        tweener.Tween(
                            ref component
                            , 1f
                            , tweenData.EasingType
                            , in tweenData.StartValue
                            , in tweenData.Target);

                        continue;
                    }}

                    if (!tweenData.StartValueInitialized)
                    {{
                        tweenData.StartValue = tweenData.UseCustomStartValue
                            ? tweenData.StartValue
                            : tweener.GetDefaultStartValue(in component);
                        tweenData.StartValueInitialized = true;
                    }}
    
                    tweener.Tween(
                        ref component
                        , timeCounterSeconds.GetNormalizedTime(tweenData.DurationSeconds)
                        , tweenData.EasingType
                        , in tweenData.StartValue
                        , in tweenData.Target);

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