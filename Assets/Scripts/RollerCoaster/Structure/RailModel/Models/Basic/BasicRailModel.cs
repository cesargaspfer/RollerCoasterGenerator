using UnityEngine;
using static RailModelProperties;

public class BasicRailModel : RailModel
{
    private RailSurface[] _rs;
    protected override SubRailModel[] GetSubRailModels()
    {
        return new SubRailModel[2]
        {
            new BasicSRMCylinder(),
            new BasicSRMSides(),
        };
    }
}
