using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static RailModelProperties;

public class SaveManager
{
    public struct SavePack
    {
        private RailProps _rp;
        private RailType _type;
        private bool _isFinalRail;
        private bool _defined;

        public SavePack(RailProps rp, RailType type, bool isFinalRail)
        {
            _rp = rp;
            _type = type;
            _isFinalRail = isFinalRail;
            _defined = true;
        }

        public SavePack(Rail rail)
        {
            _rp = rail.rp;
            _type = rail.mp.Type;
            _isFinalRail = rail.IsFinalRail;
            _defined = true;
        }

        public SavePack(byte[] content, int index)
        {
            float elevation = GetSingleFromArray(content, index);
            float rotation = GetSingleFromArray(content, index + 4);
            float inclination = GetSingleFromArray(content, index + 8);
            float length = GetSingleFromArray(content, index + 12);
            _type = (RailType) GetIntegerFromArray(content, index + 16);
            int flags = GetIntegerFromArray(content, index + 20);
            _isFinalRail = GetBit(content[index + 23], 0);
            _rp = new RailProps(elevation, rotation, inclination, length);
            _defined = true;
        }

        public override string ToString()
        {
            return _rp + ";" + (int) _type + ";" + _isFinalRail;
        }

        public byte[] ToBinary()
        {
            // Encode order:
            //  Rail Properties - RailProperties - 16 bytes
            //  Rail Type - int - 4 bytes
            //  Flags - byte - 4 bytes

            byte[] value = new byte[24];
            AddSingleToArray(_rp.Elevation, value, 0);
            AddSingleToArray(_rp.Rotation, value, 4);
            AddSingleToArray(_rp.Inclination, value, 8);
            AddSingleToArray(_rp.Length, value, 12);
            AddIntegerToArray((int) _type, value, 16);
            byte[] flags = new byte[4];
            flags[3] = SetBit(flags[3], _isFinalRail, 0);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(flags);
            int flagsInt = BitConverter.ToInt32(flags, 0);
            AddIntegerToArray(flagsInt, value, 20);
            return value;
        }

        public RailProps rp
        {
            get { return _rp; }
        }

        public RailType Type
        {
            get { return _type; }
        }

        public bool IsFinalRail
        {
            get { return _isFinalRail; }
        }

        public bool Defined
        {
            get { return _defined; }
        }
    }

    public struct SaveDecorationPack
    {
        private string _name;
        private Vector3 _position;
        private float _rotation;
        private bool _defined;
        private int _bytesCount;

        public SaveDecorationPack(string name, Vector3 position, float rotation)
        {
            _name = name;
            _position = position;
            _rotation = rotation;
            _bytesCount = 20 + Encoding.UTF8.GetByteCount(_name);
            _defined = true;
        }

        public SaveDecorationPack(byte[] content, int index)
        {
            int rawNameByteCount = GetIntegerFromArray(content, index);
            _name = Encoding.UTF8.GetString(content, index + 4, rawNameByteCount);
            index += 4 + rawNameByteCount;
            float x = GetSingleFromArray(content, index);
            float y = GetSingleFromArray(content, index + 4);
            float z = GetSingleFromArray(content, index + 8);
            _position = new Vector3(x, y, z);
            _rotation = GetSingleFromArray(content, index + 12);
            _bytesCount = 20 + rawNameByteCount;
            _defined = true;
        }

        public override string ToString()
        {
            return _name + ";" + _position + ";" + _rotation;
        }

        public byte[] ToBinary()
        {
            // Encode order:
            //  Rail Properties - RailProperties - 16 bytes
            //  Rail Type - int - 4 bytes
            //  Flags - byte - 4 bytes
            int rawNameByteCount = Encoding.UTF8.GetByteCount(_name);
            byte[] value = new byte[20 + rawNameByteCount];
            AddIntegerToArray(rawNameByteCount, value, 0);
            Encoding.UTF8.GetBytes(_name, 0, _name.Length, value, 4);
            AddSingleToArray(_position.x, value, rawNameByteCount + 4);
            AddSingleToArray(_position.y, value, rawNameByteCount + 8);
            AddSingleToArray(_position.z, value, rawNameByteCount + 12);
            AddSingleToArray(_rotation, value, rawNameByteCount + 16);
            return value;
        }

        public String name
        {
            get { return _name; }
        }

        public Vector3 Position
        {
            get { return _position; }
        }

        public float Rotation
        {
            get { return _rotation; }
        }

        public (string, Vector3, float) Tuple
        {
            get { return (_name, _position, _rotation); }
        }

        public int BytesCount
        {
            get { return _bytesCount; }
        }

        public bool Defined
        {
            get { return _defined; }
        }
    }

    public static string _pathRollerCoaster = "";
    public static string _pathResources = "";
    
    public SaveManager() {
        SetPaths();
    }

    public static void SetPaths()
    {
        if(!_pathRollerCoaster.Equals(""))
            return;

        _pathRollerCoaster = Application.dataPath + "/Saves/";
        if(!Directory.Exists(_pathRollerCoaster))
            Directory.CreateDirectory(_pathRollerCoaster);

        _pathResources = Application.dataPath + "/Resources/";
        if (!Directory.Exists(_pathResources))
            Directory.CreateDirectory(_pathResources);
            
        _pathResources = Application.dataPath + "/Resources/Blueprints/";
        if (!Directory.Exists(_pathResources))
            Directory.CreateDirectory(_pathResources);
    }

    public static bool SaveCoaster(string coasterName, Rail[] rails, (string, Vector3, float)[] decorativeObjects)
    {
        SetPaths();
        byte[] content = Encode(rails, decorativeObjects);
        try
        {
            Directory.CreateDirectory(_pathRollerCoaster + "/" + coasterName);
            ScreenCapture.CaptureScreenshot(_pathRollerCoaster + "/" + coasterName + "/Screenshot.png");
            File.WriteAllBytes(_pathRollerCoaster + "/" + coasterName + "/data.bytes", content);
            return true;
        }
        catch (Exception ex)
        {
            // TODO: Treat error
            Debug.LogError(ex.ToString());
            return false;
        }
    }

    public static (SavePack[], (string, Vector3, float)[]) LoadCoaster(string coasterName)
    {
        SetPaths();
        byte[] content;
        try
        {
            content = File.ReadAllBytes(_pathRollerCoaster + "/" + coasterName + "/data.bytes");
        }
        catch (Exception ex)
        {
            // TODO: Treat error
            Debug.LogError(ex.ToString());
            return (null, null);
        }
        return Decode(content);
    }

    public static void SaveBlueprint(string fileName, Rail[] rails)
    {
        byte[] content = Encode(rails, null);
        try
        {
            File.WriteAllBytes(_pathResources + fileName + ".bytes", content);
        }
        catch (Exception ex)
        {
            // TODO: Treat error
            Debug.LogError(ex.ToString());
        }
    }

    public static (SavePack[], (string, Vector3, float)[]) LoadBlueprint(string fileName)
    {
        TextAsset asset = Resources.Load("Blueprints/" + fileName) as TextAsset;
        return Decode(asset.bytes);
    }

    public static bool CoasterExists(string coasterName)
    {
        if (!Directory.Exists(_pathRollerCoaster + "/" + coasterName)) return false;
        if (!File.Exists(_pathRollerCoaster + "/" + coasterName + "/data.bytes")) return false;
        if (!File.Exists(_pathRollerCoaster + "/" + coasterName + "/Screenshot.png")) return false;
        return true;
    }
    
    public static (string[], Sprite[]) LoadCoastersNamesAndImages()
    {
        SetPaths();
        List<string> coastersNames = new List<string>();
        List<Sprite> coastersScreenshots = new List<Sprite>();
        foreach(string dir in Directory.GetDirectories(_pathRollerCoaster))
        {
            if (!File.Exists(dir + "/Screenshot.png")) continue;
            if (!File.Exists(dir + "/data.bytes")) continue;

            string coasterName = dir.Substring(_pathRollerCoaster.Length);

            Texture2D screenshotTexture;
            byte[] textureData;

            textureData = File.ReadAllBytes(dir + "/Screenshot.png");
            screenshotTexture = new Texture2D(2, 2);
            if (!screenshotTexture.LoadImage(textureData))
                continue;

            Sprite coasterScreenshot = Sprite.Create(screenshotTexture, new Rect(0, 0, screenshotTexture.width, screenshotTexture.height), new Vector2(0, 0), 100);

            coastersNames.Add(coasterName);
            coastersScreenshots.Add(coasterScreenshot);
        }
        return (coastersNames.ToArray(), coastersScreenshots.ToArray());
    }

    public static Sprite[] LoadCoastersImages()
    {
        SetPaths();
        List<Sprite> coastersScreenshots = new List<Sprite>();
        foreach (string dir in Directory.GetDirectories(_pathRollerCoaster))
        {
            if (!File.Exists(dir + "/Screenshot.png")) continue;
            if (!File.Exists(dir + "/data.bytes")) continue;

            Texture2D screenshotTexture;
            byte[] textureData;

            textureData = File.ReadAllBytes(dir + "/Screenshot.png");
            screenshotTexture = new Texture2D(2, 2);
            if (!screenshotTexture.LoadImage(textureData))
                continue;

            Sprite coasterScreenshot = Sprite.Create(screenshotTexture, new Rect(0, 0, screenshotTexture.width, screenshotTexture.height), new Vector2(0, 0), 100);

            coastersScreenshots.Add(coasterScreenshot);
        }
        return coastersScreenshots.ToArray();
    }

    public static Sprite LoadCoasterImage(string coasterName)
    {
        SetPaths();
        if (!File.Exists(_pathRollerCoaster + "/" + coasterName + "/Screenshot.png")) return null;

        Texture2D screenshotTexture;
        byte[] textureData;

        textureData = File.ReadAllBytes(_pathRollerCoaster + "/" + coasterName + "/Screenshot.png");
        screenshotTexture = new Texture2D(2, 2);
        if (!screenshotTexture.LoadImage(textureData))
            return null;

        Sprite coasterScreenshot = Sprite.Create(screenshotTexture, new Rect(0, 0, screenshotTexture.width, screenshotTexture.height), new Vector2(0, 0), 100);

        return coasterScreenshot;
    }

    public static string[] LoadCoastersNames()
    {
        SetPaths();
        List<string> coastersNames = new List<string>();
        List<Sprite> coastersScreenshots = new List<Sprite>();
        foreach (string dir in Directory.GetDirectories(_pathRollerCoaster))
        {
            if (!File.Exists(dir + "/Screenshot.png")) continue;
            if (!File.Exists(dir + "/data.bytes")) continue;

            string coasterName = dir.Substring(_pathRollerCoaster.Length);

            coastersNames.Add(coasterName);
        }
        return coastersNames.ToArray();
    }

    private static byte[] Encode(Rail[] rails, (string, Vector3, float)[] decorativeObjects)
    {
        // Encode order:
        // Save Version - int - 4 bytes
        // Flags - byte - 4 bytes
        // Rail Quantity - int - 4 bytes
        // {
        //  Rail Properties - RailProperties - 16 bytes
        //  Rail Type - int - 4 bytes
        //  Flags - byte - 4 bytes
        // }
        // Decorative Quantity - int - 4 bytes
        // {
        //  Name's Bytes Count  - int - 4 bytes
        //  Name - string - ? bytes
        //  Position - Vector3 - 12 bytes
        //  Rotation - int - 4 bytes
        // }
        int contentSize = rails.Length * 24 + 16;
        if (decorativeObjects != null)
        {
            int rawNameLength = 0;
            foreach((string objectName, _, _) in decorativeObjects)
            {
                rawNameLength += Encoding.UTF8.GetByteCount(objectName);
            }
            contentSize += decorativeObjects.Length * 20 + rawNameLength;
        }

        byte[] content = new byte[contentSize];

        AddIntegerToArray(1, content, 0);
        byte[] flags = new byte[4];
        flags[3] = SetBit(flags[3], decorativeObjects != null, 0);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(flags);
        int flagsInt = BitConverter.ToInt32(flags, 0);
        AddIntegerToArray(flagsInt, content, 4);

        AddIntegerToArray(rails.Length, content, 8);

        for (int i = 0; i < rails.Length; i++)
        {
            byte[] savePackBin = (new SavePack(rails[i])).ToBinary();
            for (int j = 0; j < savePackBin.Length; j++)
                content[12 + i * 24 + j] = savePackBin[j];
        }


        if (decorativeObjects != null)
        {
            int index = rails.Length * 24 + 12;
            AddIntegerToArray(decorativeObjects.Length, content, index);
            index += 4;

            for (int i = 0; i < decorativeObjects.Length; i++)
            {
                (string objectName, Vector3 position, float rotation) = decorativeObjects[i];
                byte[] saveDecPackBin = (new SaveDecorationPack(objectName, position, rotation)).ToBinary();
                for (int j = 0; j < saveDecPackBin.Length; j++)
                    content[index + j] = saveDecPackBin[j];
                index += saveDecPackBin.Length;
            }
        }
        
        return content;
    }

    public static (SavePack[], (string, Vector3, float)[]) Decode(byte[] content)
    {
        // Dencode order:
        // Save Version - int - 4 bytes
        // Flags - byte - 4 bytes
        // Rail Quantity - int - 4 bytes
        // {
        //  Rail Properties - RailProperties - 16 bytes
        //  Rail Type - int - 4 bytes
        //  Flags - byte - 4 bytes
        // }
        // Decorative Quantity - int - 4 bytes
        // {
        //  Name's Bytes Count  - int - 4 bytes
        //  Name - string - ? bytes
        //  Position - Vector3 - 12 bytes
        //  Rotation - int - 4 bytes
        // }

        int version = GetIntegerFromArray(content, 0);
        int flags = GetIntegerFromArray(content, 4);
        int railQuantity = GetIntegerFromArray(content, 8);

        SavePack[] savePack = new SavePack[railQuantity];
        for (int i = 0; i < savePack.Length; i++)
            savePack[i] = new SavePack(content, 12 + i * 24);

        (string, Vector3, float)[] decorativeObjects = null;

        if(GetBit(content[4 + 3], 0))
        {
            int index = 12 + railQuantity * 24;
            int decorativeObjectsQuantity = GetIntegerFromArray(content, index);
            index += 4;

            decorativeObjects = new (string, Vector3, float)[decorativeObjectsQuantity];
            for (int i = 0; i < decorativeObjects.Length; i++)
            {
                SaveDecorationPack saveDecPackBin = new SaveDecorationPack(content, index);
                decorativeObjects[i] = saveDecPackBin.Tuple;
                index += saveDecPackBin.BytesCount;
            }
        }

        return (savePack, decorativeObjects);
    }

    // ------------------------- Binary Operations ------------------------- //

    private static void AddIntegerToArray(int data, byte[] array, int index)
    {
        byte[] dataBytes = BitConverter.GetBytes(data);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(dataBytes);
        array[index] = dataBytes[0];
        array[index + 1] = dataBytes[1];
        array[index + 2] = dataBytes[2];
        array[index + 3] = dataBytes[3];
    }

    private static void AddSingleToArray(float value, byte[] array, int index)
    {
        byte[] valueBytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(valueBytes);
        array[index] = valueBytes[0];
        array[index + 1] = valueBytes[1];
        array[index + 2] = valueBytes[2];
        array[index + 3] = valueBytes[3];
    }

    private static int GetIntegerFromArray(byte[] array, int index)
    {
        byte[] dataBytes = Get4Bytes(array, index);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(dataBytes);
        
        int data = BitConverter.ToInt32(dataBytes, 0);
        return data;
    }

    private static float GetSingleFromArray(byte[] array, int index)
    {
        byte[] dataBytes = Get4Bytes(array, index);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(dataBytes);

        float data = BitConverter.ToSingle(dataBytes, 0);
        return data;
    }

    private static bool GetBit(byte data, int position)
    {
        return (data & (1 << position)) != 0;
    }

    private static byte SetBit(byte data, bool value, int position)
    {
        byte mask = (byte)(1 << position);
        if (value)
            // set to 1
            data |= (byte)mask;
        else
            // Set to zero
            data &= (byte)(~mask);
        return data;
    }

    private static byte[] Get4Bytes(byte[] data, int index)
    {
        return new byte[4] { data[index], data[index + 1], data[index + 2], data[index + 3] };
    }
}
