using UnityEngine;
using UnityEngine.UI;

namespace MadKnight.Test
{
    /// <summary>
    /// Debug UI blocking issues
    /// Gắn vào GameObject bất kỳ để check UI nào đang block raycasts
    /// </summary>
    public class UIDebugger : MonoBehaviour
    {
        [ContextMenu("Check UI Blocking")]
        private void CheckUIBlocking()
        {
            Debug.Log("=== CHECKING UI BLOCKING ===");
            
            // Tìm tất cả CanvasGroup
            CanvasGroup[] allCanvasGroups = FindObjectsOfType<CanvasGroup>();
            
            foreach (var cg in allCanvasGroups)
            {
                if (cg.blocksRaycasts)
                {
                    Debug.Log($"✅ BLOCKING: {GetGameObjectPath(cg.gameObject)} | Alpha: {cg.alpha} | Interactable: {cg.interactable}");
                }
                else
                {
                    Debug.Log($"❌ NOT BLOCKING: {GetGameObjectPath(cg.gameObject)} | Alpha: {cg.alpha}");
                }
            }
            
            // Tìm tất cả Image có Raycast Target
            Image[] allImages = FindObjectsOfType<Image>();
            int blockingCount = 0;
            
            foreach (var img in allImages)
            {
                if (img.raycastTarget && img.gameObject.activeInHierarchy)
                {
                    blockingCount++;
                }
            }
            
            Debug.Log($"\n📊 Total Images with Raycast Target: {blockingCount}");
            
            // Check Canvas sorting order
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in allCanvases)
            {
                Debug.Log($"📺 Canvas: {canvas.name} | Sort Order: {canvas.sortingOrder} | Override Sorting: {canvas.overrideSorting}");
            }
            
            Debug.Log("=== CHECK COMPLETE ===");
        }
        
        [ContextMenu("List All Active UI")]
        private void ListActiveUI()
        {
            Debug.Log("=== ACTIVE UI ELEMENTS ===");
            
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.activeInHierarchy && obj.GetComponent<RectTransform>() != null)
                {
                    Debug.Log($"🔵 {GetGameObjectPath(obj)}");
                }
            }
        }
        
        private string GetGameObjectPath(GameObject obj)
        {
            string path = obj.name;
            Transform current = obj.transform.parent;
            
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }
    }
}
