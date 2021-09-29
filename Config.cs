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
         
    }
}
