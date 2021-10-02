using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;

namespace ProgressionBlockRandomizer
{
    class ProgressionRandomizerWorld : ModWorld
    {
        int prevFrameNumDefeated = 0;

        private volatile bool updating = false;
        int worldID = -1;
        bool startUpdate = false;

        private static Dictionary<ushort, ushort> swapDict = new Dictionary<ushort, ushort>();

        //For future use
        private static Dictionary<ushort, ushort> solidDict = new Dictionary<ushort, ushort>();
        private static Dictionary<ushort, ushort> nonSolidDict = new Dictionary<ushort, ushort>();
        private static Dictionary<ushort, ushort> wallDict = new Dictionary<ushort, ushort>();

        private static Random seedRand = new Random();
        private static Random rand = new Random();
        static bool useSeedForRand = false;
        static int randSeed = 0;

        static bool SolidToNonsolid = false;
        static bool PreventDungeonAndTempleRandomize = true;
        static bool PreventDungeonAndTempleSpikes = true;

        //private static bool[] solidOverride;
        //private static bool[] nonSolid;
        //private static bool[] platformArr;

        //Todo: remove unnecessary tile ids
        static HashSet<ushort> platforms = new HashSet<ushort>()
        {
            //TODO: platform swapping with other platforms through style, I think
            TileID.Platforms,

            TileID.TeamBlockBluePlatform,
            TileID.TeamBlockGreenPlatform,
            TileID.TeamBlockRedPlatform,
            TileID.TeamBlockYellowPlatform,
            TileID.TeamBlockWhitePlatform,
            TileID.TeamBlockPinkPlatform,
        };

        static HashSet<ushort> nonSolid = new HashSet<ushort>()
        {
            TileID.MetalBars,
            TileID.LivingFire,
            TileID.LivingFrostFire,
            TileID.LivingIchor,
            TileID.LivingCursedFire,
            TileID.LivingDemonFire,
            TileID.LivingUltrabrightFire,
            TileID.MetalBars,
        };

        //Todo: remove unnecessary tile ids
        static HashSet<ushort> forceSkip = new HashSet<ushort>()
        {
            //Boulder breaks Framing with stack overflows for some reason?????
            TileID.Boulder,
            TileID.Teleporter,

            TileID.AlphabetStatues,
            TileID.Statues,
            TileID.Books,

            TileID.OutletPump,
            TileID.LogicGate,

            //TileID.MetalBars,

            TileID.ClosedDoor,
            TileID.OpenDoor,
            TileID.TrapdoorClosed,
            TileID.TrapdoorOpen,
            TileID.TallGateClosed,
            TileID.TallGateOpen,

            TileID.PixelBox,
            TileID.MagicalIceBlock,

            //Basically platforms and it would be rude
            TileID.PlanterBox,

            TileID.ClayPot,
            TileID.WoodenBeam,
            TileID.VineFlowers,
            TileID.Vines,
            TileID.Rope,
            TileID.VineRope,
            TileID.WebRope,
            TileID.SilkRope,
            TileID.GeyserTrap,
            TileID.Traps,
            TileID.Banners,
            TileID.Chain,
            TileID.CorruptPlants,
            TileID.PlantDetritus,
            TileID.PlanteraBulb,
            TileID.Plants,
            TileID.Plants2,
            TileID.PressurePlates,
            TileID.Saplings,
            TileID.CrimsonVines,
            TileID.HallowedVines,
            TileID.JungleVines,
            TileID.JunglePlants,
            TileID.JunglePlants2,

            TileID.Count, //Mannequin
            471, //Weapon rack
            TileID.Torches,
            TileID.Trees,
            TileID.MushroomTrees,
            TileID.Pots,
            TileID.Books,
            TileID.PiggyBank,
        };


        private static ProgressionRandomizerWorld instance;
        public ProgressionRandomizerWorld() : base()
        {
            instance = this;
            BossesDefeatedCheck();
        }
        public static bool StartRandomizing()
        {
            return StartRandomizing(Config.Instance.SolidToNonsolid, Config.Instance.PreventDungeonAndTempleRandomize, Config.Instance.PreventDungeonAndTempleSpikes, false);
        }


        public static bool StartRandomizing(bool solidToNonSolid, bool preventDungeonAndTempleRandomize, bool preventDungeonAndTempleSpikes, bool useSeed = false, int seed = 0)
        {
            if (!instance.updating && !instance.startUpdate)
            {
                SolidToNonsolid = solidToNonSolid;
                PreventDungeonAndTempleRandomize = preventDungeonAndTempleRandomize;
                PreventDungeonAndTempleSpikes = preventDungeonAndTempleSpikes;

                //Generate seed
                if (useSeed)
                {
                    useSeedForRand = useSeed;
                    randSeed = seed;
                    rand = new Random(seed);
                }
                else
                {
                    //BitConverter with a byte array so max int value is possible, rand.Next()'s max is exclusive
                    byte[] bytes = new byte[4];
                    seedRand.NextBytes(bytes);
                    randSeed = BitConverter.ToInt32(bytes, 0);

                    rand = new Random(randSeed);
                }

                instance.startUpdate = true;
                return true;
            }
            else
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText($"World is already randomizing, please wait", 50, 255, 130, false);
                }
                else if(Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral($"World is already randomizing, please wait"), new Color(50, 255, 130), -1);
                }
                return false;
            }

        }

        private static void EndRandomizing()
        {
            instance.updating = false;
        }

        public static void ShowSeed()
        {
            char ToFlag(bool b)
            {
                return b ? '1' : '0';
            }
            string seedString = $"Seed: {randSeed}:{ToFlag(SolidToNonsolid)}{ToFlag(PreventDungeonAndTempleRandomize)}{ToFlag(PreventDungeonAndTempleSpikes)}";
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(seedString, 255, 255, 50, false);
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(seedString), new Color(255, 255, 50), -1);
            }
        }


        //TODO: config for bosses
        private bool BossesDefeatedCheck()
        {
            int num = 0;
            if (NPC.downedBoss1) num++;
            if (NPC.downedBoss2) num++;
            if (NPC.downedBoss3) num++;
            if (NPC.downedGoblins) num++;
            if (NPC.downedSlimeKing) num++;
            if (NPC.downedQueenBee) num++;
            if (Main.hardMode) num++;
            if (NPC.downedMechBoss1) num++;
            if (NPC.downedMechBoss2) num++;
            if (NPC.downedMechBoss3) num++;
            if (NPC.downedPirates) num++;
            if (NPC.downedFishron) num++;
            if (NPC.downedPlantBoss) num++;
            if (NPC.downedGolemBoss) num++;
            if (NPC.downedMartians) num++;
            if (NPC.downedAncientCultist) num++;
            if (NPC.downedMoonlord) num++;

            bool changed = prevFrameNumDefeated != num;
            if(changed) prevFrameNumDefeated = num;
            return changed;
        }

        public override void PostUpdate()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                //If the world changed (different world opened in same session), we don't want to randomize it, so update number of bosses defeated and skip the rest
                if (Main.worldID != worldID)
                {
                    worldID = Main.worldID;
                    BossesDefeatedCheck();
                    return;
                }
                
                //If a new boss has been defeated, start randomizer
                if (!updating && !startUpdate && BossesDefeatedCheck())
                {
                    StartRandomizing();
                }

                //To make sure it is not called on a multiplayer client, we start from here
                if (startUpdate)
                {
                    startUpdate = false;
                    updating = true;
                    RandomizeWorld();
                }
            }
        }

        //TODO: include mod config for keeping dungeon/temple untouched
        //TODO: Wall randomization
        private static void RandomizeDictionary()
        {

            swapDict.Clear();
            solidDict.Clear();
            nonSolidDict.Clear();
            wallDict.Clear();

            List<ushort> tilesA = new List<ushort>();
            HashSet<ushort> skipHash = new HashSet<ushort>();

            if (!Config.Instance.FullRandom && !SolidToNonsolid)
            {
                foreach (var t in nonSolid) skipHash.Add(t);
            }

            if(SolidToNonsolid || Config.Instance.FullRandom)
            {
                tilesA.Add(TileID.Bubble);
                tilesA.Add(TileID.MetalBars);
                tilesA.Add(TileID.LivingFire);
                tilesA.Add(TileID.LivingFrostFire);
                tilesA.Add(TileID.LivingIchor);
                tilesA.Add(TileID.LivingCursedFire);
                tilesA.Add(TileID.LivingDemonFire);
                tilesA.Add(TileID.LivingUltrabrightFire);
                //tilesA.Add(TileID.Cobweb);
            }

            //Not implemented yet, platforms need styles
            if (!Config.Instance.FullRandom)
            {
                foreach (var t in platforms) 
                { 
                    skipHash.Add(t);
                }
            }

            
            if (PreventDungeonAndTempleRandomize)
            {
                skipHash.Add(TileID.BlueDungeonBrick);
                skipHash.Add(TileID.GreenDungeonBrick);
                skipHash.Add(TileID.PinkDungeonBrick);
                skipHash.Add(TileID.LihzahrdBrick);
            }

            if (PreventDungeonAndTempleSpikes)
            {
                skipHash.Add(TileID.Spikes);
                skipHash.Add(TileID.WoodenSpikes);
            }

            //tilesA.AddRange(Pass(Main.tileSolid, skipHash));
            tilesA.AddRange(Pass(Main.tileSolid, skipHash));

            tilesA.AddRange(Pass(TileID.Sets.Stone, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Snow, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Ices, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Ore, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Grass, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Mud, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Hallow, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Corrupt, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Crimson, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.IcesSnow, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.IcesSlush, skipHash));
            tilesA.AddRange(Pass(TileID.Sets.Leaves, skipHash));


            //tilesA.AddRange(Pass(TileID.Sets.GeneralPlacementTiles, skipHash));

            tilesA = tilesA.Select(x => x).Distinct().ToList();

            List<ushort> tilesB = new List<ushort>();
            tilesB.AddRange(tilesA);


            //List<ushort> wallsA = new List<ushort>();
            //Main.wall
            //tilesA.AddRange(Pass(Main.tile));

            //Shuffle Legacy
            for (int i = 0; i < tilesB.Count; i++)
            {
                int indB = rand.Next(tilesB.Count);
                var temp = tilesB[indB];
                tilesB[indB] = tilesB[i];
                tilesB[i] = temp;
            }

            for (int i = 0; i < tilesA.Count; i++)
            {
                swapDict[tilesA[i]] = tilesB[i];
            }


            //if (Config.Instance.FullRandom)
            //{
            //    List<ushort> tilesA = new List<ushort>();
            //    tilesA.AddRange(Pass(Main.tileSolid));
            //    List<ushort> tilesB = new List<ushort>();
            //    tilesB.AddRange(tilesA);
            //    //List<ushort> wallsA = new List<ushort>();
            //    //Main.wall
            //    //tilesA.AddRange(Pass(Main.tile));

            //    //Shuffle
            //    for (int i = 0; i < tilesB.Count; i++)
            //    {
            //        int indB = WorldGen.genRand.Next(tilesB.Count);
            //        var temp = tilesB[indB];
            //        tilesB[indB] = tilesB[i];
            //        tilesB[i] = temp;
            //    }

            //    for (int i = 0; i < tilesA.Count; i++)
            //    {
            //        swapDict[tilesA[i]] = tilesB[i];
            //    }

            //}
            //else if (Config.Instance.SolidToNonsolid)
            //{
            //    List<ushort> tilesA = new List<ushort>();
            //    tilesA.AddRange(Pass(Main.tileSolid));
            //}
            //else
            //{
            //    if (Config.Instance.SolidBlocks)
            //    {

            //    }

            //    if (Config.Instance.NonSolidBlocks)
            //    {

            //    }

            //}

        }

        private static List<ushort> Pass(bool[] s, HashSet<ushort> skip = null)
        {
            List<ushort> tilesA = new List<ushort>();

            for (ushort i = 0; (i < s.Length) && (i < ushort.MaxValue - (ushort)1); i++)
            {
                if (!forceSkip.Contains(i) && !(skip?.Contains(i)??false) && s[i])
                {
                    tilesA.Add((ushort)i);
                }
            }
            return tilesA;
        }

        private static ushort Swap(ushort prevType)
        {
            if (swapDict.TryGetValue(prevType, out ushort typeToSwapTo))
            {
                return typeToSwapTo;
            }
            else
            {
                return prevType;
            }
        }

        private static bool RandomizeWorld()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                RandomizeDictionary();
                Task.Run(async () =>
                {
                    await RandomizeTask();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private static async Task RandomizeTask()
        {
            try
            {
                //Wait for locks to be released
                while (Main.serverGenLock || WorldGen.saveLock)
                {
                    await Task.Yield();
                }

                var h = Main.tile.GetLength(1);
                var w = Main.tile.GetLength(0);
                WorldGen.noLiquidCheck = true;
                WorldGen.noTileActions = true;
                WorldGen.noMapUpdate = true;
                WorldGen.saveLock = true;

                const int MARGIN = 1;

                //Replace tiles
                for (int x = MARGIN; x < w - MARGIN; x++)
                {
                    for (int y = MARGIN; y < h - MARGIN; y++)
                    {
                        var tile = Framing.GetTileSafely(x, y);
                        if (tile != null && tile.active() && tile.liquid == 0)
                        {
                            var type = tile.type;
                            var newType = Swap(type);
                            if (type != newType)
                            {
                                Main.tile[x, y].type = newType;
                                //Thread.Yield()
                            }
                        }
                    }
                }

                //Frame tiles
                for (int i = MARGIN; i < Main.maxTilesX - MARGIN; i++)
                {
                    for (int j = MARGIN; j < Main.maxTilesY - MARGIN; j++)
                    {
                        var temp = Framing.GetTileSafely(i, j);
                        if (temp != null && temp.active())
                        {
                            try
                            {
                                WorldGen.TileFrame(i, j, false, true);
                            }
                            catch (NullReferenceException)
                            {
                                //The WorldGen.TileFrame function has some recursion that occasionally throws null reference exceptions, ignore those
                                continue;
                            }
                        }
                    }
                }

                //Not sure if correct order 
                if (Main.netMode == NetmodeID.Server)
                {
                    Netplay.ResetSections();
                }


                ////Not sure if needed 
                //try
                //{
                //    WorldGen.UpdateWorld();
                //}
                //catch (NullReferenceException)
                //{
                //    //UpdateWorld throws null ref exceptions too..
                //}

                WorldGen.noLiquidCheck = false;
                WorldGen.noTileActions = false;
                WorldGen.noMapUpdate = false;
                WorldGen.saveLock = false;

                //NetMessage.SendTileRange(-1, 1, 1, w - 2, h - 2, TileChangeType.None);

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText("World Randomized", 50, 255, 130, false);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("World Randomized"), new Color(50, 255, 130), -1);
                }

                if (Config.Instance.PrintSeed)
                {
                    ShowSeed();
                }

            }
            finally
            {
                EndRandomizing();
            }
        }

    }
}
