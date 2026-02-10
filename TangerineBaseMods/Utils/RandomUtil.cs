using UnityEngine;

namespace TangerineBaseMods.Utils
{
    public static class RandomUtil
    {
        public struct WeightedValue
        {
            public int Id;
            public float Weight;
        }
        
        // return Id of chosen weighted value
        public static int Range(params WeightedValue[] weights)
        {
            if (weights.Length == 0)
                throw new System.ArgumentException("At least one range must be included.");

            int i;
            float w;
            float total = 0f;
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i].Weight;
                if (float.IsPositiveInfinity(w)) return weights[i].Id;
                else if (w >= 0f && !float.IsNaN(w)) total += w;
            }

            if (total == 0f)
                return weights[Random.Range(0, weights.Length)].Id;

            float r = Random.value;
            float s = 0f;

            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i].Weight;
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / total;
                if (s > r) return weights[i].Id;
            }

            //should only get here if last element had a zero weight, and the r was large
            i = weights.Length - 1;
            while (i > 0 && weights[i].Weight <= 0f) i--;
            return weights[i].Id;
        }

        // return index of chosen weighted value
        public static int Range(params float[] weights)
        {
            if (weights.Length == 0)
                throw new System.ArgumentException("At least one range must be included.");

            int i;
            float w;
            float total = 0f;
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsPositiveInfinity(w)) return i;
                else if (w >= 0f && !float.IsNaN(w)) total += w;
            }

            if (total == 0f)
                return Random.Range(0, weights.Length);

            float r = Random.value;
            float s = 0f;

            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;

                s += w / total;
                if (s > r) return i;
            }

            //should only get here if last element had a zero weight, and the r was large
            i = weights.Length - 1;
            while (i > 0 && weights[i] <= 0f) i--;
            return i;
        }
    }
}
