using System;
using System.IO;
using System.Text;
using UnityEngine;
using static RailModelProperties;

public class SaveManager
{
    public struct SavePack
    {
        private RailProps _rp;
        private RailType _type;
        private bool _defined;

        public SavePack(RailProps rp, RailType type)
        {
            _rp = rp;
            _type = type;
            _defined = true;
        }

        public SavePack(Rail rail)
        {
            _rp = rail.rp;
            _type = rail.mp.Type;
            _defined = true;
        }

        public SavePack(string content)
        {
            string[] contents = content.Split(';');
            _rp = new RailProps(float.Parse(contents[0]), float.Parse(contents[1]), float.Parse(contents[2]), float.Parse(contents[3]));
            _type = (RailType) int.Parse(contents[4]);
            _defined = true;
        }

        public override string ToString()
        {
            return _rp + ";" + (int) _type;
        }

        public RailProps rp
        {
            get { return _rp; }
        }

        public RailType Type
        {
            get { return _type; }
        }
    }

    public static string _path = "";
    
    public SaveManager() {
        GetPath();
    }

    public void Test()
    {
        SavePack[] savePack = new SavePack[3] {
            new SavePack(new RailProps(1f, 2f, 3f, 0), RailType.Normal),
            new SavePack(new RailProps(2f, 3f, 4f, 1), RailType.Platform),
            new SavePack(new RailProps(5f, 6f, 7f, 8), RailType.Lever),
        };
        Save("test", savePack);
        SavePack[] savePack2 = Load("test");
        Debug.Log(savePack2);
        for (int i = 0; i < savePack2.Length; i++)
            Debug.Log(savePack2[i]);
    }

    public static string GetPath()
    {
        if(!_path.Equals(""))
            return _path;

        
        _path = Application.dataPath + "/saves/";
        if(!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        return _path;
    } 

    public SavePack[] Load(string fileName)
    {
        string content;
        try
        {
            content = File.ReadAllText(_path + fileName + ".rc");
        }
        catch (Exception ex)
        {
            // TODO: Treat error
            Debug.LogError(ex.ToString());
            return null;
        }
        string[] contents = content.Split('|');
        SavePack[] savePack = new SavePack[contents.Length];
        for (int i = 0; i < savePack.Length; i++)
            savePack[i] = new SavePack(contents[i]);
        return savePack;
    }

    public void Save(string fileName, Rail[] rails)
    {
        SavePack[] savePack = new SavePack[rails.Length];
        for (int i = 0; i < savePack.Length; i++)
            savePack[i] = new SavePack(rails[i]);
        string content = string.Join("|", savePack);
        try
        {
            File.WriteAllText(_path + fileName + ".rc", content);
        }
        catch (Exception ex)
        {
            // TODO: Treat error
            Debug.LogError(ex.ToString());
        }
    }

    public void Save(string fileName, SavePack[] savePack)
    {   
        string content = string.Join("|", savePack);
        try
        {
            File.WriteAllText(_path + fileName + ".rc", content);
        }
        catch (Exception ex)
        {
            // TODO: Treat error
            Debug.LogError(ex.ToString());
        }
    }
}
