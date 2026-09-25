using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GamePlay.CharacterAnimationSystems2;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

public class CharacterAnimationTransitionTests
{
    AnimationDataLibrary library;
    [SetUp] public void Setup()
    {
        using var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<AnimationDataBlob>();
        var clips = builder.Allocate(ref root.Clips, 3);
        clips[1] = new AnimationPhaseGroupData { Middle = Clip(1, 59) };
        clips[2] = new AnimationPhaseGroupData { Middle = Clip(61, 120) };
        library = new AnimationDataLibrary { AnimationDataBlobRef = builder.CreateBlobAssetReference<AnimationDataBlob>(Allocator.Persistent) };
    }
    static AnimationData Clip(int start, int end) => new AnimationData { HasClip = true, Loops = true, FrameRate = 30, FrameRange = new FrameRange { Start = start, End = end } };
    [TearDown] public void Cleanup() => library.AnimationDataBlobRef.Dispose();
    void Resolve(ref AnimationStateComponent state, CharacterAnimationEnum desired)
        => new AnimationStateResolverJob().Execute(new DesiredAnimation { Value = desired }, ref state, library);
    [Test] public void FirstPoseSkipsEmptyEntryClip()
    {
        var state = new AnimationStateComponent(); Resolve(ref state, CharacterAnimationEnum.Idle);
        Assert.AreEqual(AnimationPhase.Middle, state.Phase); Assert.AreEqual(1, state.Frame);
    }
    [Test] public void WalkingTransitionNeverSamplesFrameZero()
    {
        var state = new AnimationStateComponent { CurrentAnimation = CharacterAnimationEnum.Idle, Phase = AnimationPhase.Middle, Frame = 20 };
        Resolve(ref state, CharacterAnimationEnum.Walking);
        Assert.AreEqual(CharacterAnimationEnum.Walking, state.CurrentAnimation); Assert.AreEqual(61, state.Frame);
        var sampled = new CurrentFrameVAT(); new FrameSamplingJob().Execute(state, library, ref sampled);
        Assert.AreEqual(61, sampled.Value);
    }
    [Test] public void FinalFrameIsShownBeforeLooping()
    {
        var state = new AnimationStateComponent { CurrentAnimation = CharacterAnimationEnum.Idle, Phase = AnimationPhase.Middle, Frame = 59.5f };
        Resolve(ref state, CharacterAnimationEnum.Idle); Assert.AreEqual(59.5f, state.Frame);
    }
    [Test] public void LoopPreservesFractionalTime()
    {
        var state = new AnimationStateComponent { CurrentAnimation = CharacterAnimationEnum.Idle, Phase = AnimationPhase.Middle, Frame = 61.5f };
        Resolve(ref state, CharacterAnimationEnum.Idle); Assert.AreEqual(2.5f, state.Frame);
    }
    [Test] public void ReturningToCurrentAnimationCancelsStaleRequest()
    {
        var state = new AnimationStateComponent { CurrentAnimation = CharacterAnimationEnum.Idle, PendingAnimation = CharacterAnimationEnum.Walking, Phase = AnimationPhase.Middle, Frame = 25 };
        Resolve(ref state, CharacterAnimationEnum.Idle);
        Assert.AreEqual(CharacterAnimationEnum.None, state.PendingAnimation); Assert.AreEqual(25, state.Frame);
    }
}
