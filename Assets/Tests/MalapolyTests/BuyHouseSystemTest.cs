using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;

public class BuyHouseSystemTest : ECSTestsFixture
{
    private World testWorld;
    private EntityManager entityManager;

    [SetUp]
    public override void Setup()
    {
        testWorld = new World("Test world");
        entityManager = testWorld.EntityManager;

        testWorld.CreateSystem<BuyHouseSystem>();
        var systemHandle = testWorld.GetExistingSystem<BuyHouseSystem>();
        testWorld.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(systemHandle);
    }

    [TearDown]
    public override void TearDown()
    {
        testWorld.Dispose();
    }

    [Test]
    public void Adding_Events_To_Event_DynamicBuffer()
    {
        var propertyEntity = entityManager.CreateEntity
        (
            typeof(NameComponent),
            typeof(HouseCount),
            typeof(PropertySpaceTag)
        );

        testWorld.Update();

        var nameComponent = new NameComponent { Value = "Mercado" };
        entityManager.SetComponentData(propertyEntity, nameComponent);

        var entity = entityManager.CreateEntity();
        var eventBuffer = entityManager.AddBuffer<BuyHouseEvent>(entity);

        eventBuffer.Add(new BuyHouseEvent{ property = "Mercado" });
        eventBuffer.Add(new BuyHouseEvent{ property = "Mercado" });
        eventBuffer.Add(new BuyHouseEvent{ property = "Mercado" });
        eventBuffer.Add(new BuyHouseEvent{ property = "Mercado" });

        testWorld.Update();

        var houseCount = entityManager.GetComponentData<HouseCount>(propertyEntity);

        Assert.AreEqual(4, houseCount.Value);
    }
}
