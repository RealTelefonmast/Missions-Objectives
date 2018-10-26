
namespace StoryFramework
{
    public enum IncidentType
    {
        //Custom Incident Class
        CustomWorker,
        //Spawns rewards with custom conditions when fired
        Reward,
        //Unlocks vanilla research
        Research,
        //Spawns any thing on any tile determined by the position filter
        Appear,
        //Spawns skyfallers when fired
        Skyfaller,
        //Starts a raid
        Raid,
        //Default does nothing
        None
    }
}
