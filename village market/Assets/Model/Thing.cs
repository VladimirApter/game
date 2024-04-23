using UnityEngine;

namespace Model
{
    public class Thing
    {
        public GameObject ThingObj { get; set; }
        public bool IsCarried { get; set; } = false;
        public bool CanCarried { get; set; } = true;
    }
}