using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ProgressionBlockRandomizer
{
    class ShowSeedCommandd : ModCommand
    {
        public override string Command => "randseed";
        public override string Description => "Print last seed to chat";
        public override CommandType Type => CommandType.World;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            ProgressionRandomizerWorld.ShowSeed();
        }
    }
}
