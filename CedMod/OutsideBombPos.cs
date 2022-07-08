using System.Collections.Generic;
using UnityEngine;
using CedMod.Commands;

namespace CedMod
{
    /// <summary>
    /// Collection of positions. Used for <see cref="AirstrikeCommand"/>
    /// </summary>
    public static class OutsideRandomAirbombPos
    {
        /// <summary>
        /// Loads the positions.
        /// </summary>
        /// <returns>The positions as <see cref="Vector3"/></returns>
        public static List<Vector3> Load()
        {
            return new List<Vector3>
            {
                new Vector3(Random.Range(175, 182), 984, Random.Range(25, 29)),
                new Vector3(Random.Range(174, 182), 984, Random.Range(36, 39)),
                new Vector3(Random.Range(174, 182), 984, Random.Range(36, 39)),
                new Vector3(Random.Range(166, 174), 984, Random.Range(26, 39)),
                new Vector3(Random.Range(169, 171), 987, Random.Range(9, 24)),
                new Vector3(Random.Range(174, 175), 988, Random.Range(10, -2)),
                new Vector3(Random.Range(186, 174), 990, Random.Range(-1, -2)),
                new Vector3(Random.Range(186, 189), 991, Random.Range(-1, -24)),
                new Vector3(Random.Range(186, 189), 991, Random.Range(-1, -24)),
                new Vector3(Random.Range(185, 189), 993, Random.Range(-26, -34)),
                new Vector3(Random.Range(180, 195), 995, Random.Range(-36, -91)),
                new Vector3(Random.Range(148, 179), 995, Random.Range(-45, -72)),
                new Vector3(Random.Range(118, 148), 995, Random.Range(-47, -65)),
                new Vector3(Random.Range(83, 118), 995, Random.Range(-47, -65)),
                new Vector3(Random.Range(13, 15), 995, Random.Range(-18, -48)),
                new Vector3(Random.Range(84, 88), 988, Random.Range(-67, -70)),
                new Vector3(Random.Range(68, 83), 988, Random.Range(-52, -66)),
                new Vector3(Random.Range(53, 68), 988, Random.Range(-53, -63)),
                new Vector3(Random.Range(12, 49), 988, Random.Range(-47, -66)),
                new Vector3(Random.Range(38, 42), 988, Random.Range(-40, -47)),
                new Vector3(Random.Range(38, 43), 988, Random.Range(-32, -38)),
                new Vector3(Random.Range(-25, 12), 988, Random.Range(-50, -66)),
                new Vector3(Random.Range(-26, -56), 988, Random.Range(-50, -66)),
                new Vector3(Random.Range(-3, -24), 1001, Random.Range(-66, -73)),
                new Vector3(Random.Range(5, 28), 1001, Random.Range(-66, -73)),
                new Vector3(Random.Range(29, 55), 1001, Random.Range(-66, -73)),
                new Vector3(Random.Range(50, 54), 1001, Random.Range(-49, -66)),
                new Vector3(Random.Range(24, 48), 1001, Random.Range(-41, -46)),
                new Vector3(Random.Range(5, 24), 1001, Random.Range(-41, -46)),
                new Vector3(Random.Range(-4, -17), 1001, Random.Range(-41, -46)),
                new Vector3(Random.Range(4, -4), 1001, Random.Range(-25, -40)),
                new Vector3(Random.Range(11, -11), 1001, Random.Range(-18, -21)),
                new Vector3(Random.Range(3, -3), 1001, Random.Range(-4, -17)),
                new Vector3(Random.Range(2, 14), 1001, Random.Range(3, -3)),
                new Vector3(Random.Range(-1, -13), 1001, Random.Range(4, -3))
            };
        }
    }
}