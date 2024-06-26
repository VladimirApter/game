using UnityEngine;

namespace Model
{
    public class Seed : Thing
    {
        public static GameObject SeedPrefab { get; set; }
        public int FramesToGrow { get; set; } = 600;
        public Seedbed Seedbed { get; set; }
        public Seedbed[] Seedbeds { get; set; }

        public bool IsPlanted
        {
            get => Seedbed != null;
            set { }
        }
        public bool IsPlantedOnSeedbeds
        {
            get => Seedbeds != null;
            set { }
        }

        public bool IsGrowing { get; set; }
        public int GrowingFramesCount { get; set; }
    }
}