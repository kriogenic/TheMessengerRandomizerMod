using MessengerRando.Archipelago;
using MessengerRando.Exceptions;
using MessengerRando.RO;
using MessengerRando.Scripts;
using Mod.Courier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

//Replacements to implement \ work out
//  - Magic Firefly - Needs a condition to keep original sprite until after fight.
//      Currently always replaces, bug or feature?
//  - Currently use the ingame HasItem script to hide sprites, but that breaks for advanced seeds that introduce time shards.
//      Implement custom HasItem script to check states


//  - TIME SENSE - Pre setup as wingsuit sprite, need to hardcode a replacement.



namespace MessengerRando.Utils
{
    /// <summary>
    /// Class for replacing item sprites using their inventory icon
    /// Actually just hides the real sprites and adds a new child component with a sprite renderer
    /// Future: Do proper replacements
    /// </summary>
    public static class SpriteReplacer
    {

        private const string shardIcon = "\"iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAABJQTFRF5FwQ/KBE+Nh4UDAA////AAAAsL+JBQAAAGBJREFUeNqM0lEOwCAIA1Ba8f5Xnmi2LKFN7CdPQwRjmMQYU6RgylwCDdBCUgJTw6pLqHqCDfZ5Aace+OQPGREdwAOv/IAGUNB7lOzeHZYA4h37Dsys7HRvN2g/g8kjwAAOSQlBHBYXhAAAAABJRU5ErkJggg==\"";
        
        private static readonly Dictionary<EItems, string> customIcons = new Dictionary<EItems, string>
        {
            { EItems.TIME_SHARD, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAABJQTFRF5FwQ/KBE+Nh4UDAA////AAAAsL+JBQAAAGBJREFUeNqM0lEOwCAIA1Ba8f5Xnmi2LKFN7CdPQwRjmMQYU6RgylwCDdBCUgJTw6pLqHqCDfZ5Aace+OQPGREdwAOv/IAGUNB7lOzeHZYA4h37Dsys7HRvN2g/g8kjwAAOSQlBHBYXhAAAAABJRU5ErkJggg==" },
            { EItems.ENEMY_DROP_HP, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAEZJREFUeNpiYMANmLAAXOJgGfpJMDISkGBkhLDQJRhhBESCEQpAQiAVjFgkwMKMGEbBhLHbwYjLVfQPK6pI4EwMOABAgAEAKMIDj7Zbu/sAAAAASUVORK5CYII=" },
            { EItems.MONEY_WRENCH, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAxQTFRFCgAEUDAA////AAAA0Q8HNwAAAGZJREFUeNp00gEOgDAIA8BS/v9nZYoTVroYk94UlwgbAjMXCagN7yxpwAG+vkHUiJsCn4EaCEhALAhwrhEV+AYNuE9RIPd2+O0u0PoE1vckxNe3PuG51BMajsl7OOAnjD/DkEuAAQBeogVxolbkygAAAABJRU5ErkJggg==" },
            { EItems.QUARBLE_DISCOUNT_50, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAEVJREFUeNpiYMANmLAAXOJgmUEhwcgIQtgkwAhdAqycEUKi6kBIoBsFEmFEWIImwcSI1VWM2F2FrH7wBCJxkY4n7QAEGAA3sAOZNnPKtQAAAABJRU5ErkJggg==" },
            { EItems.ENEMY_DROP_MANA, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAEZJREFUeNpiYMANmLAAXOJgGfpJMDISkGBkhLDQJRhhBESCEQpAQiAVjFgkwMKMGEbBhLHbwYjLVfQPK6pI4EwMOABAgAEAKMIDj7Zbu/sAAAAASUVORK5CYII=" },
            { EItems.DAMAGE_REDUCTION, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAD1JREFUeNpiYMANmLAAXOJgGVpIMAIBFglGKMAmASWIlkChiJIg3Q5czmVCESfG52AZmgc7UZGOJ+0ABBgAH7QDhVBPRzYAAAAASUVORK5CYII=" },
            { EItems.CHECKPOINT_UPGRADE, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAEtJREFUeNq00kEOABAQA8Ct/z+arKBEsQe96aQOwkwnbaJ6l3EA9gCQEAAsT1dFobbdGoAyL5b+fpVXUCAXZRJZnJ79H8jPIJIFGAARWQN//Di4vAAAAABJRU5ErkJggg==" },
            { EItems.HEART_CONTAINER, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAENJREFUeNpiYMANmLAAXOJgGQSHkRG7BCMjkgySBCMjsgxRRuGWwOkqkp1LqgQjI4pHiJCgouVk+ZxYCZyJAQcACDAALyIDlSwXn/UAAAAASUVORK5CYII=" }, //Rejuv Spirit
            { EItems.MAP_POWER_SEAL_TOTAL, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAE9JREFUeNqk0u0KABAUA1C77//Q8j3Ziqz82Ym4ScknRFxf5QIADQAJQWmXLGjdFIaWp1tJAPazCIqFgZA7Bp3Ql3g5z+Rz7N9gP4NJFmAAE00Dex5bWXsAAAAASUVORK5CYII=" },
            { EItems.MAP_TIME_WARP, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAExJREFUeNpiYMANmLAAXOJgGepLMAIRI5jC0IEkjCTBCBFkhMkh6WCEE8QZhc9yXP7AaQcjGGC1HEmcOAkmJHEiJSiOD5yJAQcACDAAPOcDmyVkolkAAAAASUVORK5CYII=" }, //Rejuv Spirit
            { EItems.POTION_FULL_HEAL_AND_HP_UPGRADE, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAFRJREFUeNqkktsKACAIQ1v//9GBGk1wXWgPlh7RxFrT6oVU3MhygBoARAgATAJ4gK2DSA1jhwEuMu+pFL/t0OMwhwBAGuQCvPfYAbmPbyA/g9AQYAD+PwN1PMrCmQAAAABJRU5ErkJggg==" }, //Rejuv Spirit
            { EItems.NONE, "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAAlQTFRFUDAA////AAAAhFmRegAAAEJJREFUeNpiYMANmLAAXOJgGcISjGCAIcEIA2gSUBESJJhQ5ImXYMQhgdW5yOZgSjBRSwJn6FLRKGIlcCYGHAAgwAAZ9AOL+HZjvQAAAABJRU5ErkJggg==" }
        };

        /// <summary>
        /// Determines which level is loaded and what sprites need replacing for each level
        /// </summary>
        /// <param name="LI">Level Initializer</param>
        static void LevelInitializer_Rando(LevelInitializer LI)
        {

            if (LevelManager.Instance != null)
            {
                switch (LevelManager.Instance.GetCurrentLevelEnum())
                {

                    case ELevel.Level_01_NinjaVillage:
                        break;
                    case ELevel.Level_02_AutumnHills:
                        ReplaceNotes(EItems.KEY_OF_HOPE);
                        break;
                    case ELevel.Level_03_ForlornTemple:
                        ReplaceDemonCrown();
                        break;
                    case ELevel.Level_04_Catacombs:
                        ReplacePhobekin(EItems.NECROPHOBIC_WORKER);
                        ReplaceRuxxtinTomb();
                        break;
                    case ELevel.Level_04_C_RiviereTurquoise:
                        ReplaceFirefly();
                        break;
                    case ELevel.Level_05_A_HowlingGrotto:
                        break;
                    case ELevel.Level_05_B_SunkenShrine:
                        ReplaceNotes(EItems.KEY_OF_LOVE);
                        ReplaceInWorldItem(EItems.MOON_CREST);
                        ReplaceInWorldItem(EItems.SUN_CREST);
                        ReplaceInWorldItem(EItems.MAGIC_BOOTS);
                        break;
                    case ELevel.Level_06_A_BambooCreek:
                        ReplacePhobekin(EItems.CLAUSTROPHOBIC_WORKER);
                        break;
                    case ELevel.Level_07_QuillshroomMarsh:
                        ReplaceInWorldItem(EItems.SEASHELL);
                        break;
                    case ELevel.Level_08_SearingCrags:
                        ReplaceNotes(EItems.KEY_OF_STRENGTH);
                        ReplacePhobekin(EItems.PYROPHOBIC_WORKER);
                        ReplaceFlowerBed();
                        break;
                    case ELevel.Level_09_A_GlacialPeak:
                        break;
                    case ELevel.Level_09_B_ElementalSkylands:
                        ReplaceNotes(EItems.KEY_OF_SYMBIOSIS);
                        break;
                    case ELevel.Level_10_A_TowerOfTime:
                        break;
                    case ELevel.Level_11_A_CloudRuins:
                        ReplacePhobekin(EItems.ACROPHOBIC_WORKER);
                        break;
                    case ELevel.Level_12_UnderWorld:
                        ReplaceNotes(EItems.KEY_OF_CHAOS);
                        break;
                    case ELevel.Level_14_CorruptedFuture:
                        ReplaceNotes(EItems.KEY_OF_COURAGE);
                        break;
                }
                //Replace all powerseals
                ReplacePowerSeals();
                foreach (LocationRO randoItemLocation in ItemsAndLocationsHandler.ArchipelagoLocations)
                {
                    //Console.WriteLine($"Checking location {randoItemLocation.LocationName} for sprite replacement");
                    if (ItemsAndLocationsHandler.LocationsLookup.TryGetValue(randoItemLocation, out long randoLoc))
                    {
                        long itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoLoc].First().Value[0];
                        RandoItemRO RIRO;
                        if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
                        {
                            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                            return;
                        }

                        if(RIRO.Item != EItems.TIME_SHARD)
                            Console.WriteLine($"Location {randoItemLocation.PrettyLocationName} has item {RIRO.Item} mapped to it.");
                    }
                }
            }
        }


        //Hooks both Init and ReInit to the same function to check and modify sprites if required
        //At this time unsure if REINIT is required.
        internal static void LevelInitializer_InitDone(On.LevelInitializer.orig_InitDone orig, LevelInitializer self)
        {
            orig(self);
            LevelInitializer_Rando(self);
        }


        internal static void Cutscene_OnCutsceneDone(On.Cutscene.orig_OnCutsceneDone orig, Cutscene self)
        {
            orig(self);

            if (Level.Instance != null)
                Level.Instance.UpdateObjectActivators();
        }
        //We also hook Necrophobic Cutscene, even though it inherits and calls Cutscene_OnCutsceneDone, it adds the phobekin afterwards meaning we need to refresh the sprite visibility
        internal static void NecrophobicWorkerCutscene_OnCutsceneDone(On.NecrophobicWorkerCutscene.orig_OnCutsceneDone orig, NecrophobicWorkerCutscene self)
        {
            orig(self);

            if (Level.Instance != null)
                Level.Instance.UpdateObjectActivators();
        }

        /// <summary>
        /// Replaces a power seal for advanced seeds
        /// </summary>
        public static void ReplacePowerSeals()
        {

            //Find the level first and loop its room keys.
            //Find the level setup object and look for the roomKey
            GameObject levelSetup = GameObject.Find("/LevelSetup");
            Level level = levelSetup.GetComponent<Level>();
            LevelRoomDictionary LRD = level.LevelRooms;
            foreach (KeyValuePair<string, LevelRoom> RoomData in LRD)
            {
                foreach (LocationRO randoItemLocation in ItemsAndLocationsHandler.ArchipelagoLocations)
                {

                    if (randoItemLocation.LocationName.Equals(RoomData.Key))
                    {


                        long randoLoc;

                        if (!ItemsAndLocationsHandler.LocationsLookup.TryGetValue(randoItemLocation, out randoLoc))
                        {
                            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for Power Seal - ({RoomData.Key})");
                            continue;
                        }
                        if (RandomizerStateManager.HasCompletedCheck(randoLoc))
                        {
                            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for Power Seal - ({RoomData.Key}");
                            continue;
                        }


                        foreach (GameObject go in RoomData.Value.roomObjects)
                        {

                            if (go.name.Equals("PowerSeal"))
                            {

                                Color tr = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                                SpriteRenderer SR = go.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();

                                go.transform.GetChild(1).GetChild(1).GetComponent<SpriteRenderer>().color = tr;
                                SR.color = tr;


                                long itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoLoc].First().Value[0];
                                RandoItemRO RIRO;
                                if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
                                {
                                    CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                                    return;
                                }


                                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing sprite for Power Seal {RoomData.Key} - ({randoLoc}) with item {RIRO.Item}");
                                GameObject newSprite = replaceSprite(SR, RIRO.Item, new Vector3(0.0f, 0.0f, 0.0f));
                                AddHasItem(newSprite, randoLoc);
                            }
                        }


                    }
                }
            }
        }


        ///// <summary>
        ///// Replaces an In World Items sprite
        ///// </summary>
        ///// <param name="whichItem">which item we are replacing</param>
        public static void ReplaceInWorldItem(EItems whichItem)
        {
            long randoItemCheck;
            if (!RandomizerStateManager.Instance.IsLocationRandomized(whichItem, out randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for {whichItem}");
                return;
            }
            if (RandomizerStateManager.HasCompletedCheck(randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for {whichItem}");
                return;
            }


            GameObject rootItem = null;
            foreach (AwardItemCutscene AIC in Component.FindObjectsOfType<AwardItemCutscene>())
            {
                if (AIC.item.Equals(whichItem))
                {
                    rootItem = AIC.transform.root.gameObject;
                    break;
                }
            }


            if (whichItem.Equals(EItems.SEASHELL))
            {
                TakeSeaShellCutscene TSSC = Component.FindObjectOfType<TakeSeaShellCutscene>();
                rootItem = TSSC.transform.root.gameObject;
            }


            if (rootItem == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"We failed to get the root gameobject for {whichItem}");
                return;
            }




            var itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoItemCheck].First().Value[0];
            RandoItemRO RIRO;
            if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                return;
            }


            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing sprite for {whichItem} with item {RIRO.Item}");


            EItems itemIcon = RIRO.Item;
            if (InventoryManager.Instance.GetItemDefinition(itemIcon) == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not get Item Definition for Item {itemIcon}");
                return;
            }

            ItemDefinition itemDef = InventoryManager.Instance.GetItemDefinition(itemIcon);

            Texture2D inventoryTex;
            if (itemDef != null && itemDef.itemIcon != null)
            {
                inventoryTex = itemDef.itemIcon;
            }
            else
            {
                inventoryTex = CreateTextureFromBase64(itemIcon);
            }


            Sprite inventorySprite = Sprite.Create(inventoryTex, new Rect(0.0f, 0.0f, inventoryTex.width, inventoryTex.height), new Vector2(0.5f, 0.5f), 20.0f);
            if (inventorySprite == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"We failed to create the sprite for {itemIcon}");
                return;
            }


            Transform artItem = rootItem.transform.Find("Art");

            if (whichItem.Equals(EItems.SEASHELL))
                artItem = rootItem.transform.Find("Art_8");


            if (artItem != null)
            {
                foreach (SpriteRenderer SR in artItem.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (whichItem.Equals(EItems.MAGIC_BOOTS) || whichItem.Equals(EItems.SEASHELL))
                    {
                        if (SR.gameObject.name.Contains("8"))
                        {
                            SR.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            SR.material.shader = Shader.Find("Sprites/8_Bits");
                        }
                        else if (SR.gameObject.name.Contains("16"))
                        {
                            SR.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                            SR.material.shader = Shader.Find("Sprites/16_Bits");
                        }
                    }

                    SR.sprite = inventorySprite;
                }
            }

        }

        ///// <summary>
        ///// Replaces the PowerThistle sprite section of the flower bed
        ///// </summary>
        public static void ReplaceFlowerBed()
        {
            EItems whichItem = EItems.POWER_THISTLE;
            long randoItemCheck;
            if (!RandomizerStateManager.Instance.IsLocationRandomized(whichItem, out randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for {whichItem}");
                return;
            }
            if (RandomizerStateManager.HasCompletedCheck(randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for {whichItem}");
                return;
            }

            SpriteRenderer origPowerThistle = null;
            GameObject origFlowerBed = GameObject.Find("/FlowerBed");
            if (origFlowerBed == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, "Could not find original FlowerBed object");
                return;
            }

            foreach (SpriteRenderer SR in origFlowerBed.GetComponentsInChildren<SpriteRenderer>())
            {
                if (SR.sprite.name.Contains("SearingCrags_16_PowerThistle_"))
                {
                    origPowerThistle = SR;
                    break;
                }
            }
            var itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoItemCheck].First().Value[0];
            RandoItemRO RIRO;
            if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                return;
            }


            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing sprite for {whichItem} with item {RIRO.Item}");


            EItems Key = RIRO.Item;
            replaceSprite(origPowerThistle, Key, new Vector3(-0.75f, 1.0f, 0.0f), true);
        }





        ///// <summary>
        ///// Replaces the Ruxxtin Ammy sprite section of Ruxtins Tomb
        ///// </summary>
        public static void ReplaceRuxxtinTomb()
        {

            EItems whichItem = EItems.RUXXTIN_AMULET;
            long randoItemCheck;
            if (!RandomizerStateManager.Instance.IsLocationRandomized(whichItem, out randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for {whichItem}");
                return;
            }
            if (RandomizerStateManager.HasCompletedCheck(randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for {whichItem}");
                return;
            }

            SpriteRenderer origRuxxtinAmmy = null;
            GameObject origRuxxtinTomb = GameObject.Find("/RuxxtinTomb");
            if (origRuxxtinTomb == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, "Could not find original Ruxxtin Tomb object");
                return;
            }
            foreach (SpriteRenderer SR in origRuxxtinTomb.GetComponentsInChildren<SpriteRenderer>())
            {

                if (SR.sortingOrder == 4)
                {
                    origRuxxtinAmmy = SR;
                    break;
                }
            }
            var itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoItemCheck].First().Value[0];
            RandoItemRO RIRO;
            if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                return;
            }


            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing sprite for {whichItem} with item {RIRO.Item}");


            EItems Key = RIRO.Item;
            replaceSprite(origRuxxtinAmmy, Key, new Vector3(0.0f, 3.0f, 0.0f));
        }



        ///// <summary>
        ///// Replaces the Demon Crown sprite
        ///// </summary>
        public static void ReplaceDemonCrown()
        {
            EItems whichItem = EItems.DEMON_KING_CROWN;
            long randoItemCheck;
            if (!RandomizerStateManager.Instance.IsLocationRandomized(whichItem, out randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for {whichItem}");
                return;
            }
            if (RandomizerStateManager.HasCompletedCheck(randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for {whichItem}");
                return;
            }

            SpriteRenderer origCrownSprite = null;
            GameObject origDemonCrown = GameObject.Find("/BossFight/OutroCutscene/Crown");
            if (origDemonCrown == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, "Could not find original Demon King Crown object");
                return;
            }
            origCrownSprite = origDemonCrown.GetComponent<SpriteRenderer>();
            var itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoItemCheck].First().Value[0];
            RandoItemRO RIRO;
            if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                return;
            }


            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing sprite for {whichItem} with item {RIRO.Item}");


            EItems Key = RIRO.Item;
            replaceSprite(origCrownSprite, Key, new Vector3(0.0f, 0.0f, 0.0f));
        }





        ///// <summary>
        ///// Replaces the Firefly
        ///// </summary>
        public static void ReplaceFirefly()
        {
            EItems bottleItem = EItems.FAIRY_BOTTLE;
            long randoItemCheck;
            if (!RandomizerStateManager.Instance.IsLocationRandomized(bottleItem, out randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for {bottleItem}");
                return;
            }
            if (RandomizerStateManager.HasCompletedCheck(randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for {bottleItem}");
                return;
            }

            SpriteRenderer origFireflySprite = null;
            //Find original FlowerBed
            GameObject origFireFly = GameObject.Find("/BossFight");
            if (origFireFly == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, "Could not find original Luciole object");
                return;
            }

            origFireflySprite = origFireFly.transform.GetChild(6).GetChild(0).GetChild(1).GetComponent<SpriteRenderer>();
            var itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoItemCheck].First().Value[0];
            RandoItemRO RIRO;
            if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                return;
            }


            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing note sprite for {bottleItem} ({randoItemCheck})with item {RIRO.Item}");


            EItems Key = RIRO.Item;
            replaceSprite(origFireflySprite, Key, new Vector3(0.0f, 0.0f, 0.0f));
        }



        /// <summary>
        /// Replaces the sprite of the specified phobekin with its randomized items inventory icon
        /// </summary>
        /// <param name="whichPhobekin">Which phobekin to replace if a phebekin is found</param>
        public static void ReplacePhobekin(EItems whichPhobekin = EItems.NONE)
        {
            long randoItemCheck;
            if (!RandomizerStateManager.Instance.IsLocationRandomized(whichPhobekin, out randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for {whichPhobekin}");
                return;
            }
            if (RandomizerStateManager.HasCompletedCheck(randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for {whichPhobekin}");
                return;
            }



            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Attempting to replace {whichPhobekin}");
            SpriteRenderer phobekinSpriteRenderer = null;

            switch (whichPhobekin)
            {
                case EItems.PYROPHOBIC_WORKER:
                case EItems.ACROPHOBIC_WORKER:
                case EItems.CLAUSTROPHOBIC_WORKER:
                    CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"SWITCHED");
                    PhobekinCollectCutscene PCC = Component.FindObjectOfType<PhobekinCollectCutscene>();
                    CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Cutscene found: {PCC != null}");
                    //Find the sprite renderer that has the layer of enemy
                    SpriteRenderer[] allRender = PCC.transform.root.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer SR in allRender)
                    {
                        CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Looping Sprite Renderers");
                        if (SR.sortingLayerName.Equals("Enemies"))
                        {
                            phobekinSpriteRenderer = SR;
                            break;
                        }
                    }


                    break;
                case EItems.NECROPHOBIC_WORKER:
                    NecrophobicWorkerCutscene NWC = Component.FindObjectOfType<NecrophobicWorkerCutscene>();
                    phobekinSpriteRenderer = NWC.GetComponentInChildren<SpriteRenderer>();
                    break;


            }
            var itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoItemCheck].First().Value[0];
            RandoItemRO RIRO;
            if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                return;
            }


            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing sprite for {whichPhobekin} with item {RIRO.Item}");


            EItems Key = RIRO.Item;
            replaceSprite(phobekinSpriteRenderer, Key, new Vector3(0.0f, 0.5f, 0.0f));

        }


        /// <summary>
        /// Replaces the given note with its random sprite using its inventory icon from ItemDefinitions
        /// This avoids the need for custom assets to have loaded references of each actual ingame sprite
        /// </summary>
        /// <param name="whichNote">The note we are replacing, this determines which "findnote" case is used</param>
        public static void ReplaceNotes(EItems whichNote = EItems.NONE)
        {

            long randoItemCheck;
            if (!RandomizerStateManager.Instance.IsLocationRandomized(whichNote, out randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for {whichNote}");
                return;
            }
            if (RandomizerStateManager.HasCompletedCheck(randoItemCheck))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Skipping replacing sprite for {whichNote}");
                return;
            }


            AwardNoteCutscene ANC = null;

            switch (whichNote)
            {
                case EItems.KEY_OF_HOPE:
                    GameObject[] AllRoot = SceneManager.GetActiveScene().GetRootGameObjects();
                    foreach (GameObject HECK in AllRoot)
                    {
                        if ("KeyOvHope".Equals(HECK.name) && !HECK.activeInHierarchy)
                        {
                            GameObject.Destroy(HECK);
                            break;
                        }
                    }
                    ANC = Component.FindObjectOfType<AwardNoteCutscene>();
                    break;

                case EItems.KEY_OF_LOVE:
                    GameObject TO = GameObject.Find("/FX_SunMoonFire");
                    if (TO != null)
                    {
                        foreach (Transform dob in TO.transform)
                        {
                            if ("KeyOvLove".Equals(dob.name))
                            {
                                ANC = dob.gameObject.GetComponent<AwardNoteCutscene>();
                                break;
                            }
                        }
                    }
                    break;
                case EItems.KEY_OF_COURAGE:
                    ANC = Component.FindObjectOfType<AwardKeyOfCourageCutscene>();
                    break;
                case EItems.KEY_OF_SYMBIOSIS:
                    ElementalSkylandsBossOutroCutscene ESBOC = Component.FindObjectOfType<ElementalSkylandsBossOutroCutscene>();
                    ANC = ESBOC.awardNoteCutscene;
                    break;
                case EItems.KEY_OF_STRENGTH:
                case EItems.KEY_OF_CHAOS:
                    ANC = Component.FindObjectOfType<AwardNoteCutscene>();
                    break;
                default:
                    break;
            }

            if (ANC == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"We failed to find the root note object for {whichNote}");
                return;
            }

            GameObject noteRoot = ANC.gameObject;

            var itemToUnlock = ArchipelagoClient.ServerData.LocationData[randoItemCheck].First().Value[0];
            RandoItemRO RIRO;
            if (!ItemsAndLocationsHandler.ItemsLookup.TryGetValue(itemToUnlock, out RIRO))
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not find a mapping for item ID {itemToUnlock}");
                return;
            }


            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Replacing note sprite for {whichNote} ({randoItemCheck})with item {RIRO.Item}");


            EItems Key = RIRO.Item;
            if (InventoryManager.Instance.GetItemDefinition(Key) == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Could not get Item Definition for Item {Key}");
                return;
            }
            ItemDefinition itemDef = InventoryManager.Instance.GetItemDefinition(Key);


            Texture2D inventoryTex;
            if (itemDef != null && itemDef.itemIcon != null)
            {
                inventoryTex = itemDef.itemIcon;
            }
            else
            {
                inventoryTex = CreateTextureFromBase64(Key);
            }

            Sprite inventorySprite = Sprite.Create(inventoryTex, new Rect(0.0f, 0.0f, inventoryTex.width, inventoryTex.height), new Vector2(0.5f, 0.5f), 20.0f);
            if (inventorySprite == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"We failed to create the sprite for {Key}");
                return;
            }
            SpriteRenderer SR = noteRoot.GetComponentInChildren<SpriteRenderer>();
            if (SR == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"We failed to get the sprite renderer for the note {whichNote}");
                return;
            }

            SR.sprite = inventorySprite;

            //Notes are always collectable even if normally invisible in 8bit, this makes them visible in all dimensions
            GraphicDimensionSwap GDS = SR.gameObject.AddComponent<GraphicDimensionSwap>();
            GDS.spriteRenderer = SR;

            //WE WANT BOTH DIM PARTICLES!
            ParticleSystemRenderer[] PSA = noteRoot.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (ParticleSystemRenderer PS in PSA)
            {
                GraphicDimensionSwap GDSS = PS.gameObject.AddComponent<GraphicDimensionSwap>();
                GDSS.spriteRenderer = PS;
            }

        }




        /// <summary>
        /// Adds a has item check using a MonoBehaviour and a Coroutine running inside of it
        /// </summary>
        /// <param name="GO">The game object to monitor\add the check to</param>
        /// <param name="location">Current locations data</param>
        /// <param name="RIRO">The RandoItemRO associated with the item</param>
        /// <param name="randoItemCheck">The rando item check ID</param>
        private static void AddHasItem(GameObject GO, long randoItemCheck)
        {
            //Adds a HasItem for the current rando Item
            HasRandoItem NHA = GO.AddComponent<HasRandoItem>();
            //NHA.randoItemCheck = location;
            //NHA.RIRO = RIRO;
            NHA.randoCheck = randoItemCheck;

        }


        /// <summary>
        /// Creates a new sprite of an inventory, visible in both dimensions by default, attached to a parent sprite
        /// </summary>
        /// <param name="itemSR">The SpriteRenderer to take properties from and become the parent</param>
        /// <param name="newItem">The item to have the sprite created of</param>
        /// <param name="localOffset">The offset from the parent when attached</param>
        /// <param name="Only16bit">Should the sprite be only visible in 16bit? (currently used for PowerThistle)</param>
        /// <returns></returns>
        private static GameObject replaceSprite(SpriteRenderer itemSR, EItems newItem, Vector3 localOffset, bool Only16bit = false)
        {
            if (itemSR == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"We could not find a matching Sprite Renderer");
                return null;
            }
            ItemDefinition itemDef = null;
            Texture2D inventoryTex;
            if (newItem != EItems.MONEY_WRENCH && newItem != EItems.MAP_TIME_WARP)
            {
                itemDef = InventoryManager.Instance.GetItemDefinition(newItem);
            }
            if (itemDef != null && itemDef.itemIcon != null)
            {
                inventoryTex = itemDef.itemIcon;
            }
            else
            {
                inventoryTex = CreateTextureFromBase64(newItem);
            }

            Sprite inventorySprite = Sprite.Create(inventoryTex, new Rect(0.0f, 0.0f, inventoryTex.width, inventoryTex.height), new Vector2(0.5f, 0.5f), 20.0f);
            if (inventorySprite == null)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"We failed to create the sprite for {newItem}");
                return null;
            }

            itemSR.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            GameObject newArt = new GameObject();
            SpriteRenderer newSprite = newArt.AddComponent<SpriteRenderer>();
            newSprite.sprite = inventorySprite;
            newSprite.sortingLayerID = itemSR.sortingLayerID;
            newSprite.sortingOrder = itemSR.sortingOrder;
            newSprite.sortingLayerName = itemSR.sortingLayerName;

            if (Only16bit)
            {
                newSprite.material.shader = Shader.Find("Sprites/16_Bits");
            }
            else
            {
                GraphicDimensionSwap GDSS = newSprite.gameObject.AddComponent<GraphicDimensionSwap>();
                GDSS.spriteRenderer = newSprite;
            }

            newArt.transform.SetParent(itemSR.gameObject.transform);
            newArt.transform.localPosition = localOffset;

            return newArt;
        }



        public static Texture2D CreateTextureFromBase64(EItems item)
        {
            //Attempt to load our custom texture
            string base64Data = "";
            if (customIcons.ContainsKey(item))
            {
                base64Data = customIcons[item];
            }
            else
            {
                base64Data = customIcons[EItems.NONE];
            }
            byte[] imageBytes = Convert.FromBase64String(base64Data);

            Texture2D tex = new Texture2D(2, 2);

            if (tex.LoadImage(imageBytes))
            {
                return tex;
            }
            CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Failed to load custom texture icon for item {item}!");
            return null;
        }
    }
}
