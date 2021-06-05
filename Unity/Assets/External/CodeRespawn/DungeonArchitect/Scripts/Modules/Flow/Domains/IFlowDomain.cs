//$ Copyright 2015-21, Code Respawn Technologies Pvt Ltd - All Rights Reserved $//
namespace DungeonArchitect.Flow.Domains
{
    public interface IFlowDomain
    {
        System.Type[] SupportedTasks { get; }
        string DisplayName { get; }
    }
}