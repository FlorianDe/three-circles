using System;
using System.Collections.Generic;

namespace Game.Track
{
    [Serializable]
    internal class LevelDto
    {
        public string name;
        public List<float> radii;
    }
}