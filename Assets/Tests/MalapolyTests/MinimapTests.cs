using DOTS.GamePlay.Minimap;
using NUnit.Framework;
using UnityEngine;

public class MinimapTests
{
    static MinimapViewport CreateView()
    {
        var view = new MinimapViewport(new Vector2(400, 800));
        view.Resize(new Vector2(400, 400));
        return view;
    }

    [TestCase(-100, -200, 0, 1)]
    [TestCase(100, 200, 1, 0)]
    [TestCase(0, 0, 0.5f, 0.5f)]
    public void WorldPositionsUseTheCaptureBoundsAndNorthPointsUp(float x, float z, float u, float v)
    {
        var point = MinimapViewport.WorldToMap(new Vector3(x, 500, z), Rect.MinMaxRect(-100, -200, 100, 200));
        Assert.That(point.x, Is.EqualTo(u).Within(0.0001f));
        Assert.That(point.y, Is.EqualTo(v).Within(0.0001f));
    }

    [Test]
    public void FitPreservesTheImageAspectRatioAndCentersIt()
    {
        var view = CreateView();
        Assert.AreEqual(new Vector2(200, 400), view.ScaledSize);
        Assert.AreEqual(new Vector2(100, 0), view.Origin);
        Assert.AreEqual(new Vector2(200, 200), view.Project(new Vector2(0.5f, 0.5f)));
    }

    [Test]
    public void ZoomKeepsTheMapPointUnderThePointer()
    {
        var view = CreateView();
        view.ZoomAt(3f, new Vector2(200, 200));
        var point = new Vector2(0.55f, 0.45f);
        var anchor = view.Project(point);
        view.ZoomAt(1.5f, anchor);
        Assert.That(Vector2.Distance(anchor, view.Project(point)), Is.LessThan(0.0001f));
    }

    [Test]
    public void DraggingCannotMoveTheImagePastItsEdges()
    {
        var view = CreateView();
        view.ZoomAt(4f, new Vector2(200, 200));
        view.PanBy(new Vector2(10000, -10000));
        Assert.That(view.Origin.x, Is.EqualTo(0).Within(0.0001f));
        Assert.That(view.Origin.y + view.ScaledSize.y, Is.EqualTo(400).Within(0.0001f));
    }

    [Test]
    public void ZoomLimitsAndFullMapPreventAnEmptyViewport()
    {
        var view = CreateView();
        view.ZoomAt(100, new Vector2(200, 200));
        Assert.AreEqual(MinimapViewport.MaxZoom, view.Zoom);
        view.ZoomAt(0.0001f, new Vector2(200, 200));
        Assert.AreEqual(1f, view.Zoom);
        view.PanBy(Vector2.one * 1000);
        Assert.AreEqual(Vector2.zero, view.Pan);
        view.ZoomAt(3, new Vector2(200, 200));
        view.CenterOn(new Vector2(0.6f, 0.6f));
        view.Fit();
        Assert.AreEqual(1f, view.Zoom);
        Assert.AreEqual(Vector2.zero, view.Pan);
    }

    [Test]
    public void FindingAPlayerAndResizingKeepTheSameMapRegionCentered()
    {
        var view = CreateView();
        view.ZoomAt(4, new Vector2(200, 200));
        var player = new Vector2(0.6f, 0.65f);
        view.CenterOn(player);
        Assert.That(Vector2.Distance(view.Project(player), view.ViewportSize * 0.5f), Is.LessThan(0.0001f));
        view.Resize(new Vector2(600, 450));
        Assert.That(Vector2.Distance(view.Project(player), view.ViewportSize * 0.5f), Is.LessThan(0.0001f));
    }

    [Test]
    public void InvalidGesturesDoNotCorruptTheMapTransform()
    {
        var view = CreateView();
        view.ZoomAt(float.NaN, Vector2.zero);
        view.ZoomAt(-1, Vector2.zero);
        view.PanBy(new Vector2(float.PositiveInfinity, 0));
        view.Resize(Vector2.zero);
        Assert.AreEqual(1f, view.Zoom);
        Assert.AreEqual(Vector2.zero, view.Pan);
        Assert.AreEqual(new Vector2(400, 400), view.ViewportSize);
    }
}
