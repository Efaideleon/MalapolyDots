using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Tests;

public class HouseTrackerTests : ECSTestsFixture
{
    private EntityManager entityManager;
    private World testWorld;

    [SetUp]
    public override void Setup()
    {
        testWorld = new World("Test world");
        entityManager = testWorld.EntityManager;

        testWorld.CreateSystem<MonopolyTrackerSystem>();
        var systemHandle = testWorld.GetExistingSystem<MonopolyTrackerSystem>();
        testWorld.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(systemHandle);
    }

    [TearDown]
    public override void TearDown()
    {
        testWorld.Dispose();
    }

    [Test]
    public void No_Monopoly_If_No_Onwer_At_First_And_Then_One_Property_Is_Bought()
    {
        var brownProperty1 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty2 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var ownerComponent = new OwnerComponent { ID = 0 };
        var noOwnerComponent = new OwnerComponent { ID = -1 };
        var brownColorComponent = new ColorCodeComponent { Value = PropertyColor.Brown };
        var houseBuyable = new MonopolyFlagComponent { State = false };

        entityManager.SetComponentData(brownProperty1, noOwnerComponent);
        entityManager.SetComponentData(brownProperty1, brownColorComponent);
        entityManager.SetComponentData(brownProperty1, houseBuyable);

        entityManager.SetComponentData(brownProperty2, noOwnerComponent);
        entityManager.SetComponentData(brownProperty2, brownColorComponent);
        entityManager.SetComponentData(brownProperty2, houseBuyable);

        testWorld.Update();

        var houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        var houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        Assert.AreEqual(false, houseBuyable1.State);
        Assert.AreEqual(false, houseBuyable2.State);

        entityManager.SetComponentData(brownProperty1, ownerComponent);
        testWorld.Update();

        houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        var house1Owner = entityManager.GetComponentData<OwnerComponent>(brownProperty1);
        Assert.AreEqual(0, house1Owner.ID);
        Assert.AreEqual(false, houseBuyable1.State);
        Assert.AreEqual(false, houseBuyable2.State);
    }

    [Test]
    public void No_Monopoly_If_Noone_Owns_A_Property()
    {
        var brownProperty1 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty2 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var noOwnerComponent = new OwnerComponent { ID = -1 };
        var brownColorComponent = new ColorCodeComponent { Value = PropertyColor.Brown };
        var houseBuyable = new MonopolyFlagComponent { State = false };

        entityManager.SetComponentData(brownProperty1, noOwnerComponent);
        entityManager.SetComponentData(brownProperty1, brownColorComponent);
        entityManager.SetComponentData(brownProperty1, houseBuyable);

        entityManager.SetComponentData(brownProperty2, noOwnerComponent);
        entityManager.SetComponentData(brownProperty2, brownColorComponent);
        entityManager.SetComponentData(brownProperty2, houseBuyable);

        testWorld.Update();

        var houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        var houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        Assert.AreEqual(false, houseBuyable1.State);
        Assert.AreEqual(false, houseBuyable2.State);
    }

    [Test]
    public void No_Monopoly_If_Only_Own_One_Out_Of_Two_Properties()
    {
        var brownProperty1 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty2 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var ownerComponent = new OwnerComponent { ID = 1 };
        var noOwnerComponent = new OwnerComponent { ID = -1 };
        var brownColorComponent = new ColorCodeComponent { Value = PropertyColor.Brown };
        var houseBuyable = new MonopolyFlagComponent { State = false };

        entityManager.SetComponentData(brownProperty1, ownerComponent);
        entityManager.SetComponentData(brownProperty1, brownColorComponent);
        entityManager.SetComponentData(brownProperty1, houseBuyable);

        entityManager.SetComponentData(brownProperty2, noOwnerComponent);
        entityManager.SetComponentData(brownProperty2, brownColorComponent);
        entityManager.SetComponentData(brownProperty2, houseBuyable);

        testWorld.Update();

        var houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        var houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        Assert.AreEqual(false, houseBuyable1.State);
        Assert.AreEqual(false, houseBuyable2.State);
    }

    [Test]
    public void Monopoly_If_AllProperties_Have_Same_Owner()
    {
        var brownProperty1 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty2 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty3 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var ownerComponent = new OwnerComponent { ID = 1 };
        var brownColorComponent = new ColorCodeComponent { Value = PropertyColor.Brown };
        var houseBuyable = new MonopolyFlagComponent { State = false };

        testWorld.Update();

        entityManager.SetComponentData(brownProperty1, ownerComponent);
        entityManager.SetComponentData(brownProperty1, brownColorComponent);
        entityManager.SetComponentData(brownProperty1, houseBuyable);

        entityManager.SetComponentData(brownProperty2, ownerComponent);
        entityManager.SetComponentData(brownProperty2, brownColorComponent);
        entityManager.SetComponentData(brownProperty2, houseBuyable);

        entityManager.SetComponentData(brownProperty3, ownerComponent);
        entityManager.SetComponentData(brownProperty3, brownColorComponent);
        entityManager.SetComponentData(brownProperty3, houseBuyable);

        testWorld.Update();

        var houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        var houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        var houseBuyable3 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty3);

        Assert.AreEqual(true, houseBuyable1.State);
        Assert.AreEqual(true, houseBuyable2.State);
        Assert.AreEqual(true, houseBuyable3.State);
    }

    [Test]
    public void NoMonopoly_ForAll_If_The_FirstProperty_Has_NoOwner_But_TheRest_Have_SameOwner()
    {
        var brownProperty1 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty2 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty3 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var noOwnerComponent = new OwnerComponent { ID = PropertyConstants.Vacant };
        var owner1Component = new OwnerComponent { ID = 1 };
        var brownColorComponent = new ColorCodeComponent { Value = PropertyColor.Brown };
        var houseBuyable = new MonopolyFlagComponent { State = false };

        entityManager.SetComponentData(brownProperty1, noOwnerComponent);
        entityManager.SetComponentData(brownProperty1, brownColorComponent);
        entityManager.SetComponentData(brownProperty1, houseBuyable);

        entityManager.SetComponentData(brownProperty2, owner1Component);
        entityManager.SetComponentData(brownProperty2, brownColorComponent);
        entityManager.SetComponentData(brownProperty2, houseBuyable);

        entityManager.SetComponentData(brownProperty3, owner1Component);
        entityManager.SetComponentData(brownProperty3, brownColorComponent);
        entityManager.SetComponentData(brownProperty3, houseBuyable);

        testWorld.Update();

        var houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        var houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        var houseBuyable3 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty3);

        Assert.AreEqual(false, houseBuyable1.State);
        Assert.AreEqual(false, houseBuyable2.State);
        Assert.AreEqual(false, houseBuyable3.State);
    }

    [Test]
    public void Recalculating_Monopoly_After_AnOwner_Change()
    {
        var brownProperty1 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty2 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var brownProperty3 = entityManager.CreateEntity(
                typeof(OwnerComponent),
                typeof(ColorCodeComponent),
                typeof(MonopolyFlagComponent),
                typeof(PropertySpaceTag)
        );

        var noOwnerComponent = new OwnerComponent { ID = PropertyConstants.Vacant };
        var owner1Component = new OwnerComponent { ID = 1 };
        var brownColorComponent = new ColorCodeComponent { Value = PropertyColor.Brown };
        var houseBuyable = new MonopolyFlagComponent { State = false };

        entityManager.SetComponentData(brownProperty1, noOwnerComponent);
        entityManager.SetComponentData(brownProperty1, brownColorComponent);
        entityManager.SetComponentData(brownProperty1, houseBuyable);

        entityManager.SetComponentData(brownProperty2, owner1Component);
        entityManager.SetComponentData(brownProperty2, brownColorComponent);
        entityManager.SetComponentData(brownProperty2, houseBuyable);

        entityManager.SetComponentData(brownProperty3, owner1Component);
        entityManager.SetComponentData(brownProperty3, brownColorComponent);
        entityManager.SetComponentData(brownProperty3, houseBuyable);

        testWorld.Update();

        var houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        var houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        var houseBuyable3 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty3);

        Assert.AreEqual(false, houseBuyable1.State);
        Assert.AreEqual(false, houseBuyable2.State);
        Assert.AreEqual(false, houseBuyable3.State);

        // Arrange
        entityManager.SetComponentData(brownProperty1, owner1Component);

        // Act
        testWorld.Update();

        houseBuyable1 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty1);
        houseBuyable2 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty2);
        houseBuyable3 = entityManager.GetComponentData<MonopolyFlagComponent>(brownProperty3);

        // Assert
        // Now all the properties should be owned by the same player
        Assert.AreEqual(true, houseBuyable1.State);
        Assert.AreEqual(true, houseBuyable2.State);
        Assert.AreEqual(true, houseBuyable3.State);
    }
}
