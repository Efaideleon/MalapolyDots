using Assets.Common;
using DOTS.GamePlay;
using DOTS.GamePlay.CameraSystems;
using NUnit.Framework;
using UnityEngine;

public class GameSettingsTests
{
    bool hadSave;
    string saved;
    float audioVolume;

    [SetUp]
    public void CapturePreferences()
    {
        hadSave = PlayerPrefs.HasKey(GamePreferences.StorageKey);
        saved = PlayerPrefs.GetString(GamePreferences.StorageKey, "");
        audioVolume = AudioListener.volume;
    }

    [TearDown]
    public void RestorePreferences()
    {
        if (hadSave) PlayerPrefs.SetString(GamePreferences.StorageKey, saved);
        else PlayerPrefs.DeleteKey(GamePreferences.StorageKey);
        PlayerPrefs.Save();
        GamePreferences.Reload();
        AudioListener.volume = audioVolume;
    }

    [TestCase("")]
    [TestCase("{}")]
    [TestCase("invalid json")]
    public void MissingOrCorruptSettingsUsePlayableDefaults(string json)
    {
        var data = GamePreferencesData.FromJson(json);
        Assert.AreEqual(1f, data.Volume);
        Assert.AreEqual(1f, data.CameraSensitivity);
        Assert.AreEqual(1f, data.ZoomSensitivity);
        Assert.IsFalse(data.Muted);
        Assert.IsFalse(data.ReducedCameraMotion);
    }

    [Test]
    public void LoadingPartialSettingsPreservesDefaultsAndAnIntentionalZeroVolume()
    {
        var data = GamePreferencesData.FromJson("{\"Volume\":0,\"Muted\":true}");
        Assert.AreEqual(0, data.Volume);
        Assert.IsTrue(data.Muted);
        Assert.AreEqual(1, data.CameraSensitivity);
        Assert.AreEqual(1, data.ZoomSensitivity);
    }

    [Test]
    public void SavedSettingsRestoreAudioAndCameraPreferences()
    {
        GamePreferences.ResetDefaults();
        var data = GamePreferences.Current;
        data.Volume = 0.35f;
        data.Muted = true;
        data.CameraSensitivity = 1.8f;
        data.ZoomSensitivity = 0.6f;
        data.InvertVertical = true;
        data.ReducedCameraMotion = true;
        GamePreferences.Save();
        data.Volume = 1f;
        data.Muted = false;
        GamePreferences.Reload();
        Assert.AreEqual(0.35f, GamePreferences.Current.Volume);
        Assert.IsTrue(GamePreferences.Current.Muted);
        Assert.AreEqual(0, AudioListener.volume);
        Assert.AreEqual(1.8f, GamePreferences.Current.CameraSensitivity);
        Assert.AreEqual(0.6f, GamePreferences.Current.ZoomSensitivity);
        Assert.IsTrue(GamePreferences.Current.InvertVertical);
        Assert.IsTrue(GamePreferences.Current.ReducedCameraMotion);
    }

    [Test]
    public void MutingAndUnmutingRetainsTheChosenVolume()
    {
        GamePreferences.Current.Volume = 0.4f;
        GamePreferences.Current.Muted = true;
        GamePreferences.Apply();
        Assert.AreEqual(0, AudioListener.volume);
        Assert.AreEqual(0.4f, GamePreferences.Current.Volume);
        GamePreferences.Current.Muted = false;
        GamePreferences.Apply();
        Assert.AreEqual(0.4f, AudioListener.volume);
    }

    [Test]
    public void InvalidPreferencesAreClampedToUsableRanges()
    {
        var data = new GamePreferencesData { Volume = -10, CameraSensitivity = 100, ZoomSensitivity = float.NaN };
        data.Validate();
        Assert.AreEqual(0, data.Volume);
        Assert.AreEqual(2.5f, data.CameraSensitivity);
        Assert.AreEqual(1f, data.ZoomSensitivity);
    }

    [Test]
    public void DefaultResetIsPersisted()
    {
        GamePreferences.Current.Muted = true;
        GamePreferences.Current.CameraSensitivity = 2f;
        GamePreferences.ResetDefaults();
        GamePreferences.Reload();
        Assert.IsFalse(GamePreferences.Current.Muted);
        Assert.AreEqual(1f, AudioListener.volume);
        Assert.AreEqual(1f, GamePreferences.Current.CameraSensitivity);
    }

    [Test]
    public void RotationSensitivityScalesBothAxes()
    {
        var slow = new BoardCameraView();
        var fast = new BoardCameraView();
        slow.Rotate(new Vector2(40, 20), new GamePreferencesData { CameraSensitivity = 0.5f });
        fast.Rotate(new Vector2(40, 20), new GamePreferencesData { CameraSensitivity = 2f });
        Assert.That(fast.Yaw, Is.EqualTo(slow.Yaw * 4).Within(0.001));
        Assert.That(fast.Pitch, Is.EqualTo(slow.Pitch * 4).Within(0.001));
    }

    [Test]
    public void InvertedRotationChangesOnlyTheVerticalDirection()
    {
        var regular = new BoardCameraView();
        var inverted = new BoardCameraView();
        regular.Rotate(new Vector2(40, 20), new GamePreferencesData());
        inverted.Rotate(new Vector2(40, 20), new GamePreferencesData { InvertVertical = true });
        Assert.AreEqual(regular.Yaw, inverted.Yaw);
        Assert.AreEqual(regular.Pitch, -inverted.Pitch);
    }

    [Test]
    public void ZoomSensitivityChangesHowFarTheSameGestureMoves()
    {
        var slow = new BoardCameraView();
        var fast = new BoardCameraView();
        slow.Zoom(0.9f, 0.5f);
        fast.Zoom(0.9f, 2f);
        Assert.That(fast.Distance, Is.LessThan(slow.Distance));
    }

    [Test]
    public void ExtremeGesturesKeepTheCameraWithinItsLimits()
    {
        var view = new BoardCameraView();
        view.Rotate(new Vector2(100000, 100000), new GamePreferencesData());
        Assert.That(view.Yaw, Is.InRange(-180f, 180f));
        Assert.AreEqual(30, view.Pitch);
        view.Zoom(0.001f, 2f);
        Assert.AreEqual(0.7f, view.Distance);
        view.Zoom(10000f, 2f);
        Assert.AreEqual(1.8f, view.Distance);
        view.Zoom(float.NaN, 1f);
        Assert.AreEqual(1.8f, view.Distance);
    }

    [Test]
    public void ResetViewRestoresTheOriginalCameraOffset()
    {
        var view = new BoardCameraView();
        var baseline = new Vector3(-4.4f, 12f, -14f);
        view.Rotate(new Vector2(100, 50), new GamePreferencesData());
        view.Zoom(1.2f, 1f);
        Assert.AreNotEqual(baseline, view.Offset(baseline));
        view.Reset();
        Assert.That(Vector3.Distance(baseline, view.Offset(baseline)), Is.LessThan(0.0001f));
    }

    [TestCase(true, false, "will be lost")]
    [TestCase(false, true, "everyone")]
    [TestCase(false, false, "cannot rejoin")]
    public void QuitConfirmationExplainsTheCorrectConsequence(bool solo, bool host, string message)
    {
        StringAssert.Contains(message, GameSettingsController.QuitMessage(solo, host));
    }
}
