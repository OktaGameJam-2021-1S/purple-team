//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
namespace DungeonArchitect.SxEngine
{
    public class SxTextActor : SxActor
    {
        public SxTextComponent TextComponent;

        public SxTextActor()
        {
            TextComponent = AddComponent<SxTextComponent>();
        }
    }
}