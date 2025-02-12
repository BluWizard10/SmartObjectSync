﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon
{
    [RequireComponent(typeof(SmartObjectSync))]
    public class AutoRespawn : SmartObjectSyncListener
    {
        [System.NonSerialized]
        public SmartObjectSync sync;
        public override void OnChangeOwner(SmartObjectSync s, VRCPlayerApi oldOwner, VRCPlayerApi newOwner)
        {
            
        }
        public UdonBehaviour[] respawnEventListeners;
        public string respawnEventName;
        public bool ignorePhysicsEvents = true;
        public float respawnCooldown = 30f;
        [System.NonSerialized]
        private float lastSleepTime = 0f;
        public override void OnChangeState(SmartObjectSync s, int oldState, int newState)
        {
            if (s != sync)
            {
                return;
            }
            if (newState == SmartObjectSync.STATE_SLEEPING || (newState == SmartObjectSync.STATE_FALLING && s.rigid.isKinematic))
            {
                if (lastSleepTime < 0)
                {
                    lastSleepTime = Time.realtimeSinceStartup;
                    SendCustomEventDelayedSeconds(nameof(Respawn), respawnCooldown);
                }
                else
                {
                    lastSleepTime = Time.realtimeSinceStartup;
                }
            } else if (ignorePhysicsEvents && (newState == SmartObjectSync.STATE_FALLING || newState == SmartObjectSync.STATE_INTERPOLATING || newState == SmartObjectSync.STATE_TELEPORTING))
            {
                if (lastSleepTime < 0)
                {
                    lastSleepTime = Time.realtimeSinceStartup;
                    SendCustomEventDelayedSeconds(nameof(Respawn), respawnCooldown);
                }
                else
                {
                    lastSleepTime = Time.realtimeSinceStartup;
                }
            }
            else
            {
                lastSleepTime = -1001f;
            }
        }

        void Start()
        {
            sync = GetComponent<SmartObjectSync>();
            sync.AddListener(this);
        }

        public void Respawn()
        {
            if (!sync.IsLocalOwner())
            {
                return;
            }
            if (lastSleepTime > 0)
            {
                if (lastSleepTime + respawnCooldown - 0.01f < Time.realtimeSinceStartup)//0.01f for safety against floating point errors
                {
                    sync.Respawn();
                    foreach (UdonBehaviour udon in respawnEventListeners)
                    {
                        if (Utilities.IsValid(udon))
                        {
                            udon.SendCustomEvent(respawnEventName);
                        }
                    }
                }
                else
                {
                    SendCustomEventDelayedSeconds(nameof(Respawn), lastSleepTime + respawnCooldown - Time.realtimeSinceStartup);
                }
            }
        }
    }
}