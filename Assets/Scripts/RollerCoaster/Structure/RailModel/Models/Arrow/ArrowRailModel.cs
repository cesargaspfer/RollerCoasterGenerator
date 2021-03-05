using UnityEngine;
using static RailModelProperties;

public class ArrowRailModel : RailModel
{
    private RailSurface[] _rs;
    protected override SubRailModel[] GetSubRailModels()
    {
        return new SubRailModel[2]
        {
            new ArrowSRMCylinder(),
            new ArrowSRMSides(),
        };
    }
}
