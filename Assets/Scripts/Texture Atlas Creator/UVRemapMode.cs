using System;

namespace TotalDistraction.TextureAtlasCreator
{
    [Serializable]
    public enum UVRemapMode
    {
        BakeIntoMesh,
        CalculateOnAwake,
        UseInstancedShaders,
    }
}