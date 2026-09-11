using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.GameSpaces.HouseAuthoring;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;

public class HouseOpacityTest
{
    private World world;
    private EntityManager manager;
    private SimulationSystemGroup group;

    [SetUp]
    public void Setup()
    {
        world = new World("House opacity tests");
        manager = world.EntityManager;
        group = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        group.AddSystemToUpdateList(world.CreateSystem<PurchasePropertyColorSystem>());
    }

    [TearDown]
    public void TearDown() => world.Dispose();

    private Entity CreateProperty(int count)
    {
        var property = manager.CreateEntity(typeof(PropertySpaceTag), typeof(HouseCount),
            typeof(MaterialOverrideColorSlider), typeof(HouseClusterTag), typeof(HouseColoring1),
            typeof(HouseColoring2), typeof(HouseColoring3), typeof(HouseColoring4));
        manager.SetComponentData(property, new HouseCount { Value = count });
        manager.AddBuffer<LinkedEntityGroup>(property).Add(new LinkedEntityGroup { Value = property });
        return property;
    }

    private void Advance(float seconds)
    {
        world.SetTime(new TimeData(world.Time.ElapsedTime + seconds, seconds));
        group.Update();
    }

    [Test]
    public void SequentialPurchasesFadeIndependentlyAndLeaveOtherPropertiesTranslucent()
    {
        var property = CreateProperty(0);
        var other = CreateProperty(0);
        Advance(0.5f);
        Assert.That(manager.GetComponentData<HouseColoring1>(property).Value, Is.Zero);
        manager.SetComponentData(property, new HouseCount { Value = 1 });
        Advance(0.5f);
        Assert.That(manager.GetComponentData<HouseColoring1>(property).Value, Is.EqualTo(0.25f));
        Assert.That(manager.GetComponentData<HouseColoring2>(property).Value, Is.Zero);
        Advance(1.5f);
        manager.SetComponentData(property, new HouseCount { Value = 2 });
        Advance(0.5f);
        Assert.That(manager.GetComponentData<HouseColoring1>(property).Value, Is.EqualTo(1f));
        Assert.That(manager.GetComponentData<HouseColoring2>(property).Value, Is.EqualTo(0.25f));
        Assert.That(manager.GetComponentData<HouseColoring3>(property).Value, Is.Zero);
        Assert.That(manager.GetComponentData<HouseColoring4>(property).Value, Is.Zero);
        Assert.That(manager.GetComponentData<HouseColoring1>(other).Value, Is.Zero);
    }

    [Test]
    public void BuyingFourTogetherFadesAllAndClampsAtFullOpacity()
    {
        var property = CreateProperty(4);
        Advance(1f);
        Assert.That(manager.GetComponentData<HouseColoring1>(property).Value, Is.EqualTo(0.5f));
        Assert.That(manager.GetComponentData<HouseColoring4>(property).Value, Is.EqualTo(0.5f));
        Advance(5f);
        Assert.That(manager.GetComponentData<HouseColoring1>(property).Value, Is.EqualTo(1f));
        Assert.That(manager.GetComponentData<HouseColoring2>(property).Value, Is.EqualTo(1f));
        Assert.That(manager.GetComponentData<HouseColoring3>(property).Value, Is.EqualTo(1f));
        Assert.That(manager.GetComponentData<HouseColoring4>(property).Value, Is.EqualTo(1f));
    }
}
