using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

namespace ProgressionBlockRandomizer
{
    public class Config : ModConfig
    {
        public static Config Instance;

        //[Label("Randomize Solid Tiles")]
        //[DefaultValue(true)]
        //public bool SolidBlocks = true;

        //[Label("Randomize Non Solid Tiles")]
        //[DefaultValue(false)]
        //public bool NonSolidBlocks = false;

        [Label("Allow Solid/Nonsolid Tile Mixing")]
        [DefaultValue(false)]
        public bool SolidToNonsolid = false;

        [Label("Prevent Dungeon and Temple Randomization")]
        [DefaultValue(true)]
        public bool PreventDungeonAndTempleRandomize = true;

        [Label("Prevent Dungeon and Temple Spike Randomization")]
        [DefaultValue(true)]
        public bool PreventDungeonAndTempleSpikes = true;

        [Label("Display seed after randomization")]
        [DefaultValue(true)]
        public bool PrintSeed = true;

        //[Label("Randomize Platforms")]
        //[DefaultValue(false)]
        //public bool Platforms = false;

        //[Label("Randomize Walls")]
        //[DefaultValue(true)]
        //public bool Walls = true;

        //[Label("Full Random")]
        //[Tooltip("Allow any Tile to become any other tile")]
        //[DefaultValue(false)]
        [JsonIgnore]
        public bool FullRandom = false;

        public override ConfigScope Mode => ConfigScope.ServerSide;

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            return true;
        }

        public static string GetSeedBits()
        {
            char ToFlag(bool b)
            {
                return b ? '1' : '0';
            }

            return $"{ToFlag(Instance.SolidToNonsolid)}{ToFlag(Instance.PreventDungeonAndTempleRandomize)}{ToFlag(Instance.PreventDungeonAndTempleSpikes)}";
        }
         
    }
}
