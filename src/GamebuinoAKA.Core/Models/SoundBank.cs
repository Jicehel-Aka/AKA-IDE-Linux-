using System.Collections.Generic;

namespace GamebuinoAKA.Core.Models
{
    public class SoundBank
    {
        public int Version { get; set; } = 1;
        public List<SoundAsset> Assets { get; set; } = new List<SoundAsset>();
        public List<string> CustomThemes { get; set; } = new List<string>();
    }
}
