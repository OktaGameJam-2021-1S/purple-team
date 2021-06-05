//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
namespace DungeonArchitect.Flow.Impl.SnapGridFlow.Tasks
{
    public interface ISGFLayoutTaskPathBuilder
    {
        string[] GetSnapModuleCategories();
        string[] GetCategoriesAtNode(int pathIndex, int pathLength);
    }
}