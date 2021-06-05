//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
using UnityEngine;


namespace DungeonArchitect.Landscape
{
    /// <summary>
    /// The type of the texture defined in the landscape paint settings.  
    /// This determines how the specified texture would be painted in the modified terrain
    /// </summary>
    public enum LandscapeTextureType
    {
        Room,
        Corridor,
        Cliff
    }

    /// <summary>
    /// Data-structure to hold the texture settings.  This contains enough information to paint the texture 
    /// on to the terrain
    /// </summary>
    [System.Serializable]
    public class LandscapeTexture
    {
        public LandscapeTextureType textureType;
        public TerrainLayer terrainLayer;
    }
}
