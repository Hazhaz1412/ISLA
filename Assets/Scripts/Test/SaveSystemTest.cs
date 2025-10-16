using UnityEngine;
using MadKnight.Save;

namespace MadKnight.Test
{
    /// <summary>
    /// Test SaveSystem - Verify all methods exist
    /// </summary>
    public class SaveSystemTest : MonoBehaviour
    {
        [ContextMenu("Test SaveSystem")]
        private void TestSaveSystem()
        {
            // Auto Save
            SaveSystem.CreateAutoSave();
            bool hasFile = SaveSystem.HasAnySaveFile();
            MadKnight.Save.PlayerSaveData autoSave = SaveSystem.LoadAutoSave();
            
            // Manual Slots
            if (autoSave != null)
            {
                SaveSystem.SaveToSlot(autoSave, 1);
            }
            
            MadKnight.Save.PlayerSaveData slot1 = SaveSystem.LoadFromSlot(1);
            bool isUsed = SaveSystem.IsSlotUsed(1);
            SaveSlotInfo info = SaveSystem.GetSlotInfo(1);
            
            // Copy
            SaveSystem.CopyAutoSaveToSlot(1);
            SaveSystem.CopySlotToAutoSave(1);
            
            // Delete
            SaveSystem.DeleteAutoSave();
            SaveSystem.DeleteSlot(1);
            SaveSystem.DeleteAllSaves();
            
            Debug.Log("[SaveSystemTest] All methods exist and compile successfully!");
        }
    }
}
