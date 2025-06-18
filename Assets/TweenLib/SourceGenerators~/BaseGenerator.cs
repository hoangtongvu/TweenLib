using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Text;

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
                    string tweenerNamespace = Utilities.GetNamespace(structDeclaration);
                    string tweenerName = structDeclaration.Identifier.ToString();

                    // BUG: Potential bug: Unexpected behaviour if ITweener does not stay at the first place in BaseList of concreted Tweener.
                    var genericArguments = ((GenericNameSyntax)structDeclaration.BaseList.Types[0].Type)
                        .TypeArgumentList.Arguments;

                    SemanticModel semanticModel = compilation.GetSemanticModel(genericArguments[0].SyntaxTree);

                    Utilities.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[0]
                        , out string componentTypeName, out string componentNamespace);

                    Utilities.GetNameAndNamespaceOfGenericArgument(semanticModel, genericArguments[1]
                        , out string targetTypeName, out string targetNamespace);

                    this.GenerateCanTweenTag(context, tweenerName, componentNamespace);
                    this.GenerateTweenData(context, tweenerName, targetTypeName, targetNamespace, componentNamespace);
                    this.GenerateTweenComponentsAddingHelper(context, tweenerName, tweenerNamespace, componentNamespace);
                    this.GenerateTweenerStaticMethods(context, structDeclaration, tweenerName, tweenerNamespace);
                    this.GenerateTweenSystem(context, tweenerName, tweenerNamespace, componentTypeName, componentNamespace);

                }

            }
            catch (Exception e)
            {
                File.AppendAllText(ERROR_LOG_FILE_PATH, $"Source generator error:\n{e}\n");
                File.AppendAllText(ERROR_LOG_FILE_PATH, $"Debug contents:\n{errorDebugContent}\n");
            }
            
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
using TweenLib.Commons;
using TweenLib.Utilities;

namespace {componentNamespace}
{{
    public struct {tweenerName}_TweenData : Unity.Entities.IComponentData
    {{
        public TweenTimer TweenTimer;
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

        private void GenerateTweenComponentsAddingHelper(
            GeneratorExecutionContext context
            , string tweenerName
            , string tweenerNamespace
            , string componentNamespace)
        {
            string sourceCode = $@"
using Unity.Entities;
using Unity.Burst;
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

        [BurstCompile]
        public static void AddTweenComponents(in EntityManager em, Entity entity)
        {{
            em.AddComponent<Can_{tweenerName}_TweenTag>(entity);
            em.SetComponentEnabled<Can_{tweenerName}_TweenTag>(entity, false);
            em.AddComponent<{tweenerName}_TweenData>(entity);
        }}

        [BurstCompile]
        public static void AddTweenComponents(in EntityCommandBuffer ecb, Entity entity)
        {{
            ecb.AddComponent<Can_{tweenerName}_TweenTag>(entity);
            ecb.SetComponentEnabled<Can_{tweenerName}_TweenTag>(entity, false);
            ecb.AddComponent<{tweenerName}_TweenData>(entity);
        }}
    }}
}}
";

            context.AddSource($"{tweenerName}.ComponentsBakingHelper.cs", sourceCode);

        }

        private void GenerateTweenerStaticMethods(
            GeneratorExecutionContext context
            , StructDeclarationSyntax tweenerStructDeclaration
            , string tweenerName
            , string tweenerNamespace)
        {
            var sb = new StringBuilder();

            sb.AppendLine("// <auto-generated/>");

            var root = tweenerStructDeclaration.SyntaxTree.GetRoot();
            var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            foreach (var usingDirective in usings)
            {
                sb.AppendLine(usingDirective.ToFullString().Trim());
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {tweenerNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public partial struct {tweenerName}");
            sb.AppendLine("    {");

            foreach (var methodDeclaration in tweenerStructDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                if (methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                    continue;

                var returnType = methodDeclaration.ReturnType.ToString();
                var originalName = methodDeclaration.Identifier.ValueText;
                var staticName = originalName + "_Static";

                var body = methodDeclaration.Body?.ToFullString()?.Trim()
                    ?? $" => {methodDeclaration.ExpressionBody?.Expression.ToFullString().Trim()};";

                var parameterList = methodDeclaration.ParameterList.ToFullString();
                var modifiers = methodDeclaration.Modifiers.ToString();
                modifiers = "static " + modifiers;

                foreach (var attributeList in methodDeclaration.AttributeLists)
                {
                    sb.AppendLine($"        {attributeList.ToFullString().Trim()}");
                }

                sb.AppendLine($"        {modifiers} {returnType} {staticName}{parameterList}");
                sb.AppendLine($"        {body}");
                sb.AppendLine();

            }

            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("}");

            context.AddSource($"{tweenerName}.StaticMethods.cs", sb.ToString());

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
using TweenLib.Commons;
using TweenLib.Utilities.Extensions;

namespace TweenLib.Systems
{{
    [UpdateInGroup(typeof(TweenLib.TweenSystemGroup))]
    [BurstCompile]
    public partial struct {systemName} : ISystem
    {{
        private EntityQuery query;
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

            this.componentTypeHandle = state.GetComponentTypeHandle<{componentIdentifier}>(false);
            this.canTweenTagTypeHandle = state.GetComponentTypeHandle<{canTweenTagIdentifier}>(false);
            this.tweenDataTypeHandle = state.GetComponentTypeHandle<{tweenDataIdentifier}>(false);

            state.RequireForUpdate(this.query);
        }}

        [BurstCompile]
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

 
        [BurstCompile]
        public struct TweenIJC : IJobChunk
        {{
            [Unity.Collections.ReadOnly] public float DeltaTime;

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

                    tweenData.TweenTimer.Tick(in this.DeltaTime);

                    if (!tweenData.TweenTimer.DelayEnded)
                    {{
                        if (!tweenData.TweenTimer.TimeCounterReachedDelayLimit()) continue;
                        tweenData.TweenTimer.DelayEnded = true;
                        tweenData.TweenTimer.ResetTimeCounter();
                    }}
    
                    float finalNormalizedTime = 1f;

                    if (tweenData.TweenTimer.TimedOut())
                    {{
                        tweenData.TweenTimer.IncreaseLoopCounter();
                        tweenData.TweenTimer.ResetTimeCounter();
                        
                        switch (tweenData.TweenTimer.LoopType)
                        {{
                            case LoopType.Restart:
                                finalNormalizedTime = 1f;
                                break;
                            case LoopType.Flip:
                                finalNormalizedTime = 0f;

                                var temp = tweenData.StartValue;
                                tweenData.StartValue = tweenData.Target;
                                tweenData.Target = temp;
                                break;
                            case LoopType.Incremental:
                                finalNormalizedTime = 0f;

                                global::{tweenerIdentifier}.GetDifference_Static(in tweenData.Target, in tweenData.StartValue, out var difference);
                                global::{tweenerIdentifier}.GetSum_Static(in tweenData.StartValue, in difference, out tweenData.StartValue);
                                global::{tweenerIdentifier}.GetSum_Static(in tweenData.Target, in difference, out tweenData.Target);
                                break;
                            case LoopType.Yoyo:
                                finalNormalizedTime = 1 - tweenData.TweenTimer.LoopCounter % 2;
                                tweenData.TweenTimer.ToggleNormalizedTimeDirection();
                                break;
                        }}
                    }}

                    if (!tweenData.TweenTimer.IsInfiniteLoop() && tweenData.TweenTimer.LoopCounterExceeded())
                    {{
                        // Stop tweening
                        canTweenTag.ValueRW = false;
                        
                        // Finalize the component on tween stop
                        global::{tweenerIdentifier}.Tween_Static(
                            ref component
                            , finalNormalizedTime
                            , tweenData.EasingType
                            , in tweenData.StartValue
                            , in tweenData.Target);

                        continue;
                    }}

                    if (!tweenData.StartValueInitialized)
                    {{
                        if (!tweenData.UseCustomStartValue)
                            global::{tweenerIdentifier}.GetDefaultStartValue_Static(in component, out tweenData.StartValue);

                        tweenData.StartValueInitialized = true;
                    }}
    
                    global::{tweenerIdentifier}.Tween_Static(
                        ref component
                        , tweenData.TweenTimer.GetNormalizedTime()
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