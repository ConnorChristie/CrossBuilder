// Decompiled with JetBrains decompiler
// Type: DebHelper.Implementation.Helpers
// Assembly: DebHelper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BBF69DC7-B687-4C0E-817F-F685AFA1F465
// Assembly location: C:\Users\Connor\.nuget\packages\debhelper\1.0.0\lib\net461\DebHelper.dll

using System;
using System.Text;

namespace DebHelper.Implementation
{
  internal static class Helpers
  {
    public static byte[] Read(this byte[] data, int index, int length)
    {
      byte[] numArray = new byte[length];
      Array.Copy((Array) data, index, (Array) numArray, 0, length);
      return numArray;
    }

    public static byte[] Read(this byte[] data, int index)
    {
      int length = data.Length - index;
      return data.Read(index, length);
    }

    public static string ConvertToString(this byte[] data)
    {
      return Encoding.ASCII.GetString(data).Trim();
    }

    public static int ConvertToInt(this byte[] data)
    {
      return int.Parse(data.ConvertToString().Trim());
    }

    public static string ReadString(this byte[] data, int index, int length)
    {
      return data.Read(index, length).ConvertToString();
    }

    public static int ReadInt(this byte[] data, int index, int length)
    {
      return data.Read(index, length).ConvertToInt();
    }
  }
}
