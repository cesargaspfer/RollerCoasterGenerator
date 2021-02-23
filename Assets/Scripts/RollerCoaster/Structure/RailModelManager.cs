using UnityEngine;

public class RailModelManager
{
    public static RailModel[] _railModels = null;

    public static RailModel GetRailModel(int id)
    {
        if(_railModels == null)
            GenerateRailModels();

        return _railModels[id];
    }

    private static void GenerateRailModels()
    {
        _railModels = new RailModel[2]
        {
            new BasicRailModel(),
            new ThreeCylindersRailModel(),
        };
    }
}
