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

    public static void SaveCoaster(string coasterName, Rail[] rails)
    {
        SetPaths();
        byte[] content = Encode(rails);
        try
        {
            Directory.CreateDirectory(_pathRollerCoaster + "/" + coasterName);
            ScreenCapture.CaptureScreenshot(_pathRollerCoaster + "/" + coasterName + "/Screenshot.png");
            File.WriteAllBytes(_pathRollerCoaster + "/" + coasterName + "/data.bytes", content);
        }
        catch (Exception ex)
        {
            // TODO: Treat error
            Debug.LogError(ex.ToString());
        }
    }

    public static SavePack[] LoadCoaster(string coasterName)
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
            return null;
        }
        return Decode(content);
    }

    public static void SaveBlueprint(string fileName, Rail[] rails)
    {
        byte[] content = Encode(rails);
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

    public static SavePack[] LoadBlueprint(string fileName)
    {
        TextAsset asset = Resources.Load("Blueprints/" + fileName) as TextAsset;
        return Decode(asset.bytes);
    }

    private static byte[] Encode(Rail[] rails)
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

        byte[] content = new byte[rails.Length * 24 + 12];
        AddIntegerToArray(0, content, 0);
        AddIntegerToArray(0, content, 4);
        AddIntegerToArray(rails.Length, content, 8);

        for (int i = 0; i < rails.Length; i++)
        {
            byte[] savePackBin = (new SavePack(rails[i])).ToBinary();
            for (int j = 0; j < savePackBin.Length; j++)
                content[12 + i * 24 + j] = savePackBin[j];
        }
        
        return content;
    }

    public static SavePack[] Decode(byte[] content)
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

        int version = GetIntegerFromArray(content, 0);
        int flags = GetIntegerFromArray(content, 4);
        int railQuantity = GetIntegerFromArray(content, 8);

        SavePack[] savePack = new SavePack[railQuantity];
        for (int i = 0; i < savePack.Length; i++)
            savePack[i] = new SavePack(content, 12 + i * 24);

        return savePack;
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
