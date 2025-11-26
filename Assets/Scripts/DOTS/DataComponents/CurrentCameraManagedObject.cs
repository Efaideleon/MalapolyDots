
using Unity.Entities;
using UnityEngine;

#nullable enable
namespace DOTS.DataComponents
{
    public class CurrentCameraMangedObject : IComponentData
    {
        public Camera? Camera;
    }
}
