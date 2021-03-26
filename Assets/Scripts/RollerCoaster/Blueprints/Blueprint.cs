using System.Collections.Generic;
using static RailModelProperties;

public abstract class Blueprint
{
    public Blueprint () { }
    public abstract float GetProbability(SpaceProps spaceProps, RailPhysics railPhysics);
    public abstract List<string> GetSubtypeNames();
    public abstract Dictionary<string, Dictionary<string, string>> GetParams();
    public abstract List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict);
}
