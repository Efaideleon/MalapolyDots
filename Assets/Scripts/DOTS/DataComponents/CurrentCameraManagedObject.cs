
using Unity.Entities;
using UnityEngine;

#nullable enable
namespace DOTS.DataComponents
{
    public class CurrentCameraManagedObject : IComponentData
    {
        public Camera? Camera;
    }
}
