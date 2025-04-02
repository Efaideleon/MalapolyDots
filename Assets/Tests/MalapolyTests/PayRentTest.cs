#if UNITY_EDITOR
using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;

public class RentCalculatorSystemTests : ECSTestsFixture
{
    private World testWorld;
    private EntityManager entityManager;

    [SetUp]
    public override void Setup() // override?
    {
        // Create a new test world and get its EntityManager.
        testWorld = new World("Test World");
        entityManager = testWorld.EntityManager;
        
        // Create the system from the test world.
        testWorld.CreateSystem<RentCalculatorSystem>();
        var systemHandle = testWorld.GetExistingSystem<RentCalculatorSystem>();
        testWorld.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(systemHandle);
    }

    [TearDown]
    public override void TearDown()
    {
        if (testWorld != null && testWorld.IsCreated)
        {
            testWorld.Dispose();
            World.DefaultGameObjectInjectionWorld = null;
        }
    }

    [Test]
    public void RentIsCalculated_WhenOwnerIsNotVacant()
    {
        // Create an entity with the required components.
        Entity entity = entityManager.CreateEntity(
            typeof(RentComponent),
            typeof(OwnerComponent)
        );

        // Add the dynamic buffer for BaseRentBuffer.
        DynamicBuffer<BaseRentBuffer> baseRents = entityManager.AddBuffer<BaseRentBuffer>(entity);

        // Setup test data:
        // Set OwnerComponent to a non-vacant ID (e.g., 1).
        entityManager.SetComponentData(entity, new OwnerComponent { ID = 1 });
        // Initialize the rent to 0.
        entityManager.SetComponentData(entity, new RentComponent { Value = 0 });
        // Add a base rent value to the buffer.
        baseRents.Add(new BaseRentBuffer { Value = 1000 });

        // Run a world update so the system's OnUpdate is called.
        testWorld.Update();

        // Verify the RentComponent was updated to match the first base rent.
        RentComponent rentAfterUpdate = entityManager.GetComponentData<RentComponent>(entity);
        Assert.AreEqual(1000, rentAfterUpdate.Value);
    }

    [Test]
    public void RentIsNotCalculated_WhenOwnerIsVacant()
    {
        // Create an entity with the required components.
        Entity entity = entityManager.CreateEntity(
            typeof(RentComponent),
            typeof(OwnerComponent)
        );

        // Add the dynamic buffer for BaseRentBuffer.
        DynamicBuffer<BaseRentBuffer> baseRents = entityManager.AddBuffer<BaseRentBuffer>(entity);

        // Setup test data:
        // Set OwnerComponent to vacant using PropertyConstants.
        entityManager.SetComponentData(entity, new OwnerComponent { ID = PropertyConstants.Vacant });
        // Initialize the rent to 0.
        entityManager.SetComponentData(entity, new RentComponent { Value = 0 });
        // Add a base rent value.
        baseRents.Add(new BaseRentBuffer { Value = 1000 });

        // Run a world update so the system's OnUpdate is called.
        testWorld.Update();

        // Verify the RentComponent remains unchanged because the owner is vacant.
        RentComponent rentAfterUpdate = entityManager.GetComponentData<RentComponent>(entity);
        Assert.AreEqual(0, rentAfterUpdate.Value);
    }
}
#endif
