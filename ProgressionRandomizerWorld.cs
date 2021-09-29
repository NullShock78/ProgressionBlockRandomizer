﻿using Microsoft.Xna.Framework;
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

        private static Dictionary<ushort, ushort> solidDict = new Dictionary<ushort, ushort>();
        private static Dictionary<ushort, ushort> nonSolidDict = new Dictionary<ushort, ushort>();
        private static Dictionary<ushort, ushort> wallDict = new Dictionary<ushort, ushort>();

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
        static HashSet<ushort> skipFraming = new HashSet<ushort>();
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

            TileID.PlanterBox,
            TileID.WoodenBeam,
            TileID.VineFlowers,
            TileID.VineRope,
            TileID.Vines,
            TileID.Rope,
            TileID.SilkRope,
            TileID.GeyserTrap,
            TileID.Traps

        };


        private static ProgressionRandomizerWorld instance;
        public ProgressionRandomizerWorld() : base()
        {
            instance = this;
            BossesDefeatedCheck();
        }

        public static bool StartRandomizing()
        {
            if (!instance.updating)
            {
                instance.startUpdate = true;
                return true;
            }
            return false;
        }

        private static void EndRandomizing()
        {
            instance.updating = false;
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

            if (!Config.Instance.FullRandom && !Config.Instance.SolidToNonsolid)
            {
                foreach (var t in nonSolid) skipHash.Add(t);
            }

            if(Config.Instance.SolidToNonsolid || Config.Instance.FullRandom)
            {
                tilesA.Add(TileID.Bubble);
                tilesA.Add(TileID.MetalBars);
                tilesA.Add(TileID.LivingFire);
                tilesA.Add(TileID.LivingFrostFire);
                tilesA.Add(TileID.LivingIchor);
                tilesA.Add(TileID.LivingCursedFire);
                tilesA.Add(TileID.LivingDemonFire);
                tilesA.Add(TileID.LivingUltrabrightFire);
                tilesA.Add(TileID.Cobweb);
                //tilesA.Add(TileID.WoodenBeam);
            }

            if (!Config.Instance.FullRandom) 
                foreach (var t in platforms) skipHash.Add(t);
            skipFraming = skipHash;
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
            tilesA.Add(TileID.Dirt);
            tilesA.Add(TileID.ClayBlock);
            tilesA.AddRange(Pass(TileID.Sets.Falling, skipHash));

            //tilesA.AddRange(Pass(TileID.Sets.HellSpecial, skipHash));
            //tilesA.AddRange(Pass(TileID.Sets.GrassSpecial, skipHash));
            //tilesA.AddRange(Pass(TileID.Sets.JungleSpecial, skipHash));

            tilesA = tilesA.Select(x => x).Distinct().ToList();
            

            //tilesA.AddRange(Pass(TileID.Sets.NotReallySolid, skipHash));


            //tilesA.AddRange(Pass(TileID.Sets.GeneralPlacementTiles, skipHash));
            //tilesA.AddRange(Pass(Main.tileMoss, skipHash));
            //tilesA.AddRange(Pass(Main.tileSolid, skipHash));
            //tilesA.AddRange(Pass(Main.tileMoss, skipHash)); 
            //tilesA.AddRange(Pass(Main.tileSolidTop, skipHash));
            //tilesA.AddRange(Pass(Main.tileFlame, skipHash));
            //tilesA.AddRange(Pass(Main.tileStone, skipHash));
            List<ushort> tilesB = new List<ushort>();
            tilesB.AddRange(tilesA);


            //List<ushort> wallsA = new List<ushort>();
            //Main.wall
            //tilesA.AddRange(Pass(Main.tile));

            //Shuffle Legacy
            for (int i = 0; i < tilesB.Count; i++)
            {
                int indB = WorldGen.genRand.Next(tilesB.Count);
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

                //Replace tiles
                for (int x = 1; x < w - 1; x++)
                {
                    for (int y = 1; y < h - 1; y++)
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
                for (int i = 1; i < Main.maxTilesX - 1; i++)
                {
                    for (int j = 1; j < Main.maxTilesY - 1; j++)
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
            }
            finally
            {
                EndRandomizing();
            }
        }

    }
}
