using BepInEx.Configuration;
using System;
using System.Globalization;
using UnityEngine;


namespace TangerineBaseMods.Config.TypeConverters
{
    internal class QuaternionTypeConverter : TypeConverter
    {
        public QuaternionTypeConverter()
        {
            ConvertToString = delegate (object obj, Type type)
            {
                if (type == typeof(Quaternion))
                    return ((Quaternion)obj).ToString("F5");

                return new Quaternion().ToString("F5");
            };

            ConvertToObject = delegate (string str, Type type)
            {
                if (type == typeof(Quaternion))
                {
                    // remove parentheses
                    if (str.StartsWith("(") && str.EndsWith(")"))
                        str = str.Substring(1, str.Length - 2);

                    // split values
                    var sQuaternion = str.Split(", ");

                    // parse values
                    return new Quaternion(
                        float.Parse(sQuaternion[0], NumberFormatInfo.InvariantInfo),
                        float.Parse(sQuaternion[1], NumberFormatInfo.InvariantInfo),
                        float.Parse(sQuaternion[2], NumberFormatInfo.InvariantInfo),
                        float.Parse(sQuaternion[3], NumberFormatInfo.InvariantInfo));
                }
                return new Quaternion();
            };
        }
    }

    internal class Vector3TypeConverter : TypeConverter
    {
        public Vector3TypeConverter()
        {
            ConvertToString = delegate (object obj, Type type)
            {
                if (type == typeof(Vector3))
                    return ((Vector3)obj).ToString("F5");

                return new Vector3().ToString("F5");
            };

            ConvertToObject = delegate (string str, Type type)
            {
                if (type == typeof(Vector3))
                {
                    // remove parentheses
                    if (str.StartsWith("(") && str.EndsWith(")"))
                        str = str.Substring(1, str.Length - 2);

                    // split values
                    var sVector3 = str.Split(", ");

                    // parse values
                    return new Vector3(
                        float.Parse(sVector3[0], NumberFormatInfo.InvariantInfo),
                        float.Parse(sVector3[1], NumberFormatInfo.InvariantInfo),
                        float.Parse(sVector3[2], NumberFormatInfo.InvariantInfo));
                }
                return new Vector3();
            };
        }
    }

    internal class ColorTypeConverter : TypeConverter
    {
        public ColorTypeConverter()
        {
            ConvertToString = delegate (object obj, Type type)
            {
                if (type == typeof(Color))
                    return ((Color)obj).ToString();
                
                return new Color().ToString();
            };

            ConvertToObject = delegate (string str, Type type)
            {
                if (type == typeof(Color))
                {
                    // remove parentheses
                    if (str.StartsWith("RGBA(") && str.EndsWith(")"))
                        str = str.Substring(5, str.Length - 6);

                    // split values
                    var sColor = str.Split(", ");

                    // parse values
                    return new Color(
                        float.Parse(sColor[0], NumberFormatInfo.InvariantInfo),
                        float.Parse(sColor[1], NumberFormatInfo.InvariantInfo),
                        float.Parse(sColor[2], NumberFormatInfo.InvariantInfo),
                        float.Parse(sColor[3], NumberFormatInfo.InvariantInfo));
                }
                return new Color();
            };
        }
    }
}