using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace ProgressionBlockRandomizer
{
    class RandomizeCommand : ModCommand
    {
        public override string Command => "randworld";
        public override string Description => "Randomize all world blocks. Include a seed as an argument to generate with a specific seed. Note that config options must be the same as well.";
        public override CommandType Type => CommandType.World;
        Regex seedRegex = new Regex(@"^(-?[0-9]+)(\:[01]{3})?$");

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args != null && args.Length > 0)
            {
                string bits = null;
                string seed = null;
                bool Flag(int ind)
                {
                    return bits[ind] != '0';
                }

                if(args[0].Length > 0)
                {
                    var match = seedRegex.Match(args[0]);
                    if (match.Success)
                    {
                        seed = match.Groups[1].Value;
                        if (match.Groups[2].Success)
                        {
                            bits = match.Groups[2].Value.Substring(1);
                        }
                        else
                        {
                            bits = Config.GetSeedBits();
                        }

                        if (int.TryParse(seed, out int result))
                        {
                            ProgressionRandomizerWorld.StartRandomizing(Flag(0), Flag(1), Flag(2), true, result);
                            return;
                        }
                    }
                }

                //Else
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText($"Seed {args[0]} is invalid, please enter a valid seed", 50, 255, 130, false);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral($"Seed {args[0]} is invalid, please enter a valid seed"), new Color(50, 255, 130), -1);
                }
            }
            else //No args
            {
                ProgressionRandomizerWorld.StartRandomizing();
            }
        }
    }
}
