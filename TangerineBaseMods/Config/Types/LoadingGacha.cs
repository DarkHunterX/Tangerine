using BepInEx.Configuration;
using System;


namespace TangerineBaseMods.Config.Types
{
    internal struct LoadingGacha
    {
        public int CharRate { get; set; } = 10;
        public int SkinRate { get; set; } = 10;
        public int WepRate { get; set; } = 10;
        public int CardRate { get; set; } = 10;
        public int ChipRate { get; set; } = 10;
        public int ArmorRate { get; set; } = 10;
        public int ItemRate { get; set; } = 10;
        public int SkillRate { get; set; } = 10;
        public int OtherRate { get; set; } = 10;

        public LoadingGacha()
        {      
        }

        public LoadingGacha(string str)
        {
            try
            {
                // remove parentheses
                if (str.StartsWith("(") && str.EndsWith(")"))
                    str = str.Substring(1, str.Length - 2);

                // split values
                var sLoadingGacha = str.Split(", ");

                if (sLoadingGacha.Length == 9)
                {
                    CharRate = int.Parse(sLoadingGacha[0]);
                    SkinRate = int.Parse(sLoadingGacha[1]);
                    WepRate = int.Parse(sLoadingGacha[2]);
                    CardRate = int.Parse(sLoadingGacha[3]);
                    ChipRate = int.Parse(sLoadingGacha[4]);
                    ArmorRate = int.Parse(sLoadingGacha[5]);
                    ItemRate = int.Parse(sLoadingGacha[6]);
                    SkillRate = int.Parse(sLoadingGacha[7]);
                    OtherRate = int.Parse(sLoadingGacha[8]);
                }
                else
                    throw new Exception($"error parsing LoadingGacha string \"{str}\"");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Could not convert string to LoadingGacha: {ex}");
            }
        }

        public int TotalWeight()
        {
            return CharRate + SkinRate + WepRate + CardRate + ChipRate + ArmorRate + ItemRate + SkillRate + OtherRate;
        }

        public override string ToString()
        { 
            return $"({CharRate}, {SkinRate}, {WepRate}, {CardRate}, {ChipRate}, {ArmorRate}, {ItemRate}, {SkillRate}, {OtherRate})";
        }

        public int[] ToIntArray()
        {
            return new int[] { CharRate, SkinRate, WepRate, CardRate, ChipRate, ArmorRate, ItemRate, SkillRate, OtherRate };
        }

        public float[] ToFloatArray()
        {
            return new float[] { (float)CharRate, (float)SkinRate, (float)WepRate, (float)CardRate, (float)ChipRate, (float)ArmorRate, (float)ItemRate, (float)SkillRate, (float)OtherRate };
        }
    }
}