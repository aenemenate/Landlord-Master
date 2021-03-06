﻿using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace Landlord
{
    
    class Player : Creature
    {
        private bool pausePathing = false;
        private int dangerCount = 0;
        private List<Creature> localCreatures = new List<Creature>();

        // CONSTRUCTORS

        public Player (Block[] map, Point position, Point worldIndex, int currentFloor, int sightDist, string name, string gender, DietType diet, string faction, Class uclass, byte graphic = 1,
            bool solid = true, bool opaque = true) : base (map, position, worldIndex, currentFloor, sightDist, graphic, name, gender, diet, faction, solid, opaque)
        {
            ForeColor = Color.AntiqueWhite;

            BlueprintPouch bpPouch = new BlueprintPouch( false );
            RecipePouch craftPouch = new RecipePouch( false );
            Inventory.Add( bpPouch );
            Inventory.Add( craftPouch );
            Inventory.Add( new Axe( true ) { WeaponName = "pickaxe", Material = Material.Copper } );
            Inventory.Add( new Torch(false) );
            foreach (Item i in Inventory)
                i.DetermineDurability();

            Gold = 550;
            Class = uclass;
            DetermineStats();
            DetermineEquipment();
        }

        public Player() : base()
        {
        }


        // FUNCTIONS
        public override void DetermineStats()
        {
            Stats = new Stats(Class);
        }

        public override void HandleVisibility()
        {
            localCreatures = new List<Creature>();

            RayCaster.SetAllInvis(Position, SightDist + 5, WorldIndex, CurrentFloor);

            Block[] blocks = CurrentFloor >= 0 ? Program.WorldMap[WorldIndex.X, WorldIndex.Y].Dungeon.Floors[CurrentFloor].Blocks : Program.WorldMap[WorldIndex.X, WorldIndex.Y].Blocks;
            Tile[] tiles = CurrentFloor >= 0 ? Program.WorldMap[WorldIndex.X, WorldIndex.Y].Dungeon.Floors[CurrentFloor].Floor : Program.WorldMap[WorldIndex.X, WorldIndex.Y].Floor;
            int width = Program.WorldMap[WorldIndex.X, WorldIndex.Y].Width;
            int localDangerCount = 0;
            foreach (Point point in VisiblePoints) {
                blocks[point.X * width + point.Y].Visible = true;
                blocks[point.X * width + point.Y].Explored = true;
                if (blocks[point.X * width + point.Y].Opaque == false) {
                    tiles[point.X * width + point.Y].Visible = true;
                    tiles[point.X * width + point.Y].Explored = true;
                }

                //// add items to memory map, remove them if there is a mismatch between memory map and local map
                //if (blocks[point.X * width + point.Y] is Item)
                //    Program.WorldMap[WorldIndex.X, WorldIndex.Y].MemoryMap[point.X * width + point.Y] = blocks[point.X * width + point.Y];
                //else if (Program.WorldMap[WorldIndex.X, WorldIndex.Y].MemoryMap[point.X * width + point.Y] != null)
                //    Program.WorldMap[WorldIndex.X, WorldIndex.Y].MemoryMap[point.X * width + point.Y] = null;
                

                if (blocks[point.X * width + point.Y] is Creature creature) {
                    localCreatures.Add(creature);
                    if (creature is Monster monster && (monster.Faction.Equals(Faction) == false && monster.Alive))
                        localDangerCount++;
                }
            }

            if (localDangerCount > 0) {
                if (dangerCount < localDangerCount)
                    Program.Player.Path = null;
                dangerCount = localDangerCount;
            }
            else
                dangerCount = localDangerCount;
        }

        public override void DetermineAction()
        {
            if (!Alive)
                return;

            if (Program.CurrentState is Play play) {
                if (play.PlayMode == PlayMode.Roguelike) {
                    pausePathing = PlayerInput.HandleInput(true, true);
                    if (!pausePathing)
                        FollowPath();
                }
                else
                    FollowPath();
            }

            
            UpdateFOV();
            HandleVisibility();
        }

        // PROPERTIES
        public int DangerCount
        {
            get { return dangerCount; }
            set { dangerCount = value; }
        }
        public List<Creature> LocalCreatures {
            get { return localCreatures; }
            set { localCreatures = value; }
        }
    }
}