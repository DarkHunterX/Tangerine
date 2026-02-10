using BepInEx.Configuration;
using System;
using TangerineBaseMods.Config.Types;


namespace TangerineBaseMods.Config.TypeConverters
{
    internal class LoadingGachaTypeConverter : TypeConverter
    {
        public LoadingGachaTypeConverter()
        {
            ConvertToString = delegate (object obj, Type type)
            {
                if (type == typeof(LoadingGacha))
                    return ((LoadingGacha)obj).ToString();

                return new LoadingGacha().ToString();
            };

            ConvertToObject = delegate (string str, Type type)
            {
                if (type == typeof(LoadingGacha))
                    return new LoadingGacha(str);

                return new LoadingGacha();
            };
        }
    }
}