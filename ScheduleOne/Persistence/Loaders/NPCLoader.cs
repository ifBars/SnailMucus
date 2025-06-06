using System;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders
{
	public class NPCLoader : Loader
	{
		public virtual string NPCType => typeof(NPCData).Name;

		public NPCLoader()
		{
			Singleton<LoadManager>.Instance.NPCLoaders.Add(this);
		}

		public override void Load(string mainPath)
		{
			if (!TryLoadFile(mainPath, "NPC", out var contents))
			{
				Console.LogWarning(GetType()?.ToString() + " could not load NPC file from path: " + mainPath);
				return;
			}
			NPCData data = null;
			try
			{
				data = JsonUtility.FromJson<NPCData>(contents);
			}
			catch (Exception ex)
			{
				Console.LogError(GetType()?.ToString() + " error reading NPCData: " + ex);
				return; // Cannot proceed without NPCData
			}

			if (data == null)
			{
				Console.LogError(GetType()?.ToString() + " failed to deserialize NPCData from path: " + mainPath + ". Contents: " + contents);
				return;
			}

			if (NPCManager.NPCRegistry == null)
            {
                Console.LogError(GetType()?.ToString() + $" NPCRegistry is null. Cannot load NPC {data.Name} (ID: {data.ID}).");
                return;
            }

			NPC nPC = NPCManager.NPCRegistry.FirstOrDefault((NPC x) => x.ID == data.ID);
			if (nPC == null)
			{
				Console.LogWarning(GetType()?.ToString() + $" failed to find NPC with ID: {data.ID} (Name: {data.Name}) in NPCRegistry. Path: {mainPath}");
				return;
			}

			nPC.Load(data, mainPath); // Assuming nPC.Load() itself is robust or its failures are acceptable for now

			if (TryLoadFile(mainPath, "Relationship", out var contents2))
			{
				RelationshipData relationshipData = null;
				try
				{
					relationshipData = JsonUtility.FromJson<RelationshipData>(contents2);
				}
				catch (Exception ex2)
				{
					Console.LogError(GetType()?.ToString() + $" error reading relationship data for NPC {nPC.ID}: {ex2}");
				}
				if (relationshipData != null)
				{
					if (nPC.RelationData == null)
                    {
                        Console.LogWarning(GetType()?.ToString() + $" NPC {nPC.ID} (Name: {nPC.Name}) has null RelationData. Skipping relationship load.");
                    }
                    else
                    {
                        if (!float.IsNaN(relationshipData.RelationDelta) && !float.IsInfinity(relationshipData.RelationDelta))
                        {
                            nPC.RelationData.SetRelationship(relationshipData.RelationDelta);
                        }
                        if (relationshipData.Unlocked)
                        {
                            nPC.RelationData.Unlock(relationshipData.UnlockType, notify: false);
                        }
                    }
				}
			}

			TryLoadInventory(mainPath, nPC); // TryLoadInventory already has some internal robustness by checking file existence

			if (TryLoadFile(mainPath, "Health", out var contents3))
			{
				NPCHealthData nPCHealthData = null;
				try
				{
					nPCHealthData = JsonUtility.FromJson<NPCHealthData>(contents3);
				}
				catch (Exception ex3)
				{
					Console.LogError(GetType()?.ToString() + $" error reading health data for NPC {nPC.ID}: {ex3}");
				}
				if (nPCHealthData != null)
				{
					if (nPC.Health == null)
                    {
                        Console.LogWarning(GetType()?.ToString() + $" NPC {nPC.ID} (Name: {nPC.Name}) has null Health component. Skipping health load.");
                    }
                    else
                    {
                        nPC.Health.Load(nPCHealthData);
                    }
				}
			}

			if (TryLoadFile(mainPath, "MessageConversation", out var contents4))
			{
				MSGConversationData mSGConversationData = null;
				try
				{
					mSGConversationData = JsonUtility.FromJson<MSGConversationData>(contents4);
				}
				catch (Exception ex4)
				{
					Console.LogError(GetType()?.ToString() + $" error reading message data for NPC {nPC.ID}: {ex4}");
				}
				if (mSGConversationData != null)
				{
                    if (nPC.MSGConversation == null)
                    {
                        Console.LogWarning(GetType()?.ToString() + $" NPC {nPC.ID} (Name: {nPC.Name}) has null MSGConversation. Skipping message load.");
                    }
                    else
                    {
                        nPC.MSGConversation.Load(mSGConversationData);
                    }
				}
			}

			if (TryLoadFile(mainPath, "CustomerData", out var contents5))
			{
				ScheduleOne.Persistence.Datas.CustomerData customerData = null;
				try
				{
					customerData = JsonUtility.FromJson<ScheduleOne.Persistence.Datas.CustomerData>(contents5);
				}
				catch (Exception ex5)
				{
					Console.LogError(GetType()?.ToString() + $" error reading customer data for NPC {nPC.ID}: {ex5}");
				}
				if (customerData != null)
                {
                    Customer customerComponent = nPC.GetComponent<Customer>();
                    if (customerComponent != null)
                    {
                        customerComponent.Load(customerData);
                    }
                    else
                    {
                         Console.LogWarning(GetType()?.ToString() + $" NPC {nPC.ID} (Name: {nPC.Name}) does not have a Customer component. Skipping customer data load.");
                    }
                }
			}
		}

		protected void TryLoadInventory(string mainPath, NPC npc)
		{
			if (npc == null)
            {
                Console.LogWarning(GetType()?.ToString() + " TryLoadInventory called with a null NPC for path: " + mainPath);
                return;
            }
			if (!TryLoadFile(mainPath, "Inventory", out var contents))
			{
				// Console.LogWarning(GetType()?.ToString() + $" No inventory file found for NPC {npc.ID} at {mainPath}. This might be normal.");
				return;
			}

            if (npc.Inventory == null)
            {
                Console.LogWarning(GetType()?.ToString() + $" NPC {npc.ID} (Name: {npc.Name}) has null Inventory. Skipping inventory load from {mainPath}.");
                return;
            }
            if (npc.Inventory.ItemSlots == null)
            {
                 Console.LogWarning(GetType()?.ToString() + $" NPC {npc.ID} (Name: {npc.Name}) has null Inventory.ItemSlots. Skipping inventory load from {mainPath}.");
                return;
            }

			ItemInstance[] array = null;
            try
            {
                array = ItemSet.Deserialize(contents);
            }
            catch (Exception ex)
            {
                Console.LogError(GetType()?.ToString() + $" Error deserializing inventory for NPC {npc.ID} from {mainPath}: {ex}");
                return;
            }

            if (array == null)
            {
                Console.LogWarning(GetType()?.ToString() + $" Deserialized inventory is null for NPC {npc.ID} from {mainPath}.");
                return;
            }

			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
				{
                    if (i < npc.Inventory.ItemSlots.Length)
                    {
                        npc.Inventory.ItemSlots[i].SetStoredItem(array[i]);
                    }
                    else
                    {
                        Console.LogWarning(GetType()?.ToString() + $" Inventory slot index {i} out of bounds for NPC {npc.ID} (Name: {npc.Name}). Max slots: {npc.Inventory.ItemSlots.Length}. Skipping item.");
                    }
				}
			}
		}
	}
}
