﻿using System;
using System.Collections.Generic;
using MessengerRando.Utils;
using MessengerRando.RO;


namespace MessengerRando
{
    class RandomizerStateManager
    {
        public static RandomizerStateManager Instance { private set; get; }
        public Dictionary<LocationRO, EItems> CurrentLocationToItemMapping { set; get; }
        public Dictionary<string, string> CurrentLocationDialogtoRandomDialogMapping { set; get; }
        public bool IsRandomizedFile { set; get; }

        private Dictionary<int, SeedRO> seeds;

        private Dictionary<EItems, bool> noteCutsceneTriggerStates;


        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new RandomizerStateManager();
            }
        }

        private RandomizerStateManager()
        {
            //Create initial values for the state machine
            this.seeds = new Dictionary<int, SeedRO>();
            this.ResetCurrentLocationToItemMappings();
            this.initializeCutsceneTriggerStates();
        }

        private void initializeCutsceneTriggerStates()
        {
            noteCutsceneTriggerStates = new Dictionary<EItems, bool>();
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_CHAOS, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_COURAGE, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_HOPE, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_LOVE, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_STRENGTH, false);
            noteCutsceneTriggerStates.Add(EItems.KEY_OF_SYMBIOSIS, false);
        }

        public void AddSeed(int fileSlot, SeedType seedType, int seed)
        {
            seeds[fileSlot] = new SeedRO(seedType, seed);
        }

        public SeedRO GetSeedForFileSlot(int fileSlot)
        {
            SeedRO seed = new SeedRO();

            if (seeds.ContainsKey(fileSlot))
            {
                seed = seeds[fileSlot];
            }
            return seed;
        }

        public void ResetSeedForFileSlot(int fileSlot)
        {
            //Simply keeping resetting logic here in case I want to change it i'll only do so here
            Console.WriteLine($"Resetting file slot '{fileSlot}'");
            if (seeds.ContainsKey(fileSlot))
            {
                seeds[fileSlot] = new SeedRO(SeedType.None, 0);
            }
            Console.WriteLine("File slot reset complete.");
        }

        public bool HasSeedForFileSlot(int fileSlot)
        {
            bool seedFound = false;

            if (this.seeds.ContainsKey(fileSlot) && this.seeds[fileSlot].Seed != 0 && this.seeds[fileSlot].SeedType != SeedType.None)
            {
                seedFound = true;
            }

            return seedFound;
        }

        public void ResetCurrentLocationToItemMappings()
        {
            CurrentLocationToItemMapping = new Dictionary<LocationRO, EItems>();
            this.IsRandomizedFile = false;
        }

        public bool IsNoteCutsceneTriggered(EItems note)
        {
            return this.noteCutsceneTriggerStates[note];
        }

        public void SetNoteCutsceneTriggered(EItems note)
        {
            this.noteCutsceneTriggerStates[note] = true;
        }

        public bool IsSafeTeleportState()
        {
            //Unsafe teleport states are shops/hq/boss fights
            bool isTeleportSafe = true;

            Console.WriteLine($"In ToT HQ: {Manager<TotHQ>.Instance.root.gameObject.activeInHierarchy}");
            Console.WriteLine($"In Shop: {Manager<Shop>.Instance.gameObject.activeInHierarchy}");

            //ToT HQ or Shop
            if (Manager<TotHQ>.Instance.root.gameObject.activeInHierarchy || Manager<Shop>.Instance.gameObject.activeInHierarchy)
            {
                isTeleportSafe = false;
            }

            return isTeleportSafe;
        }

        /// <summary>
        /// Helper method to log out the current mappings all nicely for review
        /// </summary>
        public void LogCurrentMappings()
        {
            Console.WriteLine("----------------BEGIN Current Mappings----------------");
            foreach (LocationRO check in this.CurrentLocationToItemMapping.Keys)
            {
                Console.WriteLine($"Item '{this.CurrentLocationToItemMapping[check]}' is located at Check '{check.LocationName}'");
            }
            Console.WriteLine("----------------END Current Mappings----------------");


            Console.WriteLine("----------------BEGIN Current Dialog Mappings----------------");
            foreach (KeyValuePair<string, string> KVP in CurrentLocationDialogtoRandomDialogMapping)
            {
                Console.WriteLine($"Dialog '{KVP.Value}' is located at Check '{KVP.Key}'");
            }
        }

    }
}
