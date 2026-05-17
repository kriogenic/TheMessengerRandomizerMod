using MessengerRando.Archipelago;
using MessengerRando.RO;
using MessengerRando.Utils;
using Mod.Courier;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MessengerRando.Scripts
{
    /// <summary>
    /// A MonoBehaviour added by SpriteReplacer with the LocationRO given
    /// Checks for the collection of the rando item pointed to in the LocationRO and sets the gameobject this script is attached to, to be inactive.
    /// </summary>
    public class HasRandoItem : MonoBehaviour
    {
        public float checkDelay = 3.0f;

        public long randoItemCheck;
        public long randoCheck;

        public RandoItemRO RIRO;

        Coroutine CR;
        void OnEnable()
        {
            if (CR != null)
            {
                StopCoroutine(CR);
            }

            CR = StartCoroutine(CheckForIDRoutine());
        }

        void Start()
        {
            if (CR != null)
            {
                StopCoroutine(CR);
            }

            CR = StartCoroutine(CheckForIDRoutine());
        }

        IEnumerator CheckForIDRoutine()
        {
            bool found = false;
            while (found == false)
            {
                CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Debugging IDS: randoItemCheck:{randoItemCheck} randoCheck:{randoCheck}");

                if (RandomizerStateManager.HasCompletedCheck(randoCheck))
                {
                    CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"BIG WINS!");
                    found = true;
                }
                //if (ArchipelagoClient.ServerData.ReceivedItems.ContainsKey(randoItemLocation))
                //{
                //    CourierLogger.Log(RandomizerConstants.LOGGER_TAG, $"Rando item {randoItemLocation} has been collected.");
                //    found = false;
                //}
                yield return new WaitForSeconds(checkDelay);
            }
            this.gameObject.SetActive(false);
        }
    }
}