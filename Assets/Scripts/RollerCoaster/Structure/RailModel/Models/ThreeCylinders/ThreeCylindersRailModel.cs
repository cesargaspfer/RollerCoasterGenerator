using UnityEngine;
using static RailModelProperties;

public class ThreeCylindersRailModel : RailModel
{
    private RailSurface[] _rs;

    protected override SubRailModel[] GetSubRailModels()
    {
        return new SubRailModel[5]
        {
            new ThreeCylindersSRMCylinder(),
            new ThreeCylindersSRMSides(),
            new ThreeCylindersSRMTransversals(),
            new ThreeCylindersSRMCube(),
            new ThreeCylindersSRMCubeSides(),
        };
    }
}
