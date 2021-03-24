using System.Collections.Generic;
using static RailModelProperties;

public abstract class Blueprint
{
    public Blueprint () { }

    public abstract Dictionary<string, Dictionary<string, string>> GetParams();
    public abstract List<(RailProps, RailType)> GetBlueprint(string type, Dictionary<string, float> dict);
}
