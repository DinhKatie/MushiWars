using ExitGames.Client.Photon;
using UnityEngine;
using System;

public static class PhotonCustomTypes
{
    public static void Register()
    {
        PhotonPeer.RegisterType(typeof(Vector3Int), (byte)'V', SerializeVector3Int, DeserializeVector3Int);
        Debug.Log("Vector3Int custom type registered with Photon.");
    }

    private static short SerializeVector3Int(StreamBuffer outStream, object customObject)
    {
        Vector3Int vector = (Vector3Int)customObject;
        byte[] bytes = new byte[12];
        Buffer.BlockCopy(BitConverter.GetBytes(vector.x), 0, bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(vector.y), 0, bytes, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(vector.z), 0, bytes, 8, 4);
        outStream.Write(bytes, 0, bytes.Length);
        return 12;
    }

    private static object DeserializeVector3Int(StreamBuffer inStream, short length)
    {
        byte[] bytes = new byte[length];
        inStream.Read(bytes, 0, length);
        int x = BitConverter.ToInt32(bytes, 0);
        int y = BitConverter.ToInt32(bytes, 4);
        int z = BitConverter.ToInt32(bytes, 8);
        return new Vector3Int(x, y, z);
    }
}
