using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MadKnight.UI
{
    /// <summary>
    /// Quick tester for BloodDripEffect V3.0 - BLEEDING TEXT
    /// Attach vào GameObject có TextMeshProUGUI
    /// </summary>
    [RequireComponent(typeof(BloodDripEffect))]
    public class BleedingTextTester : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private KeyCode startKey = KeyCode.Space;
        [SerializeField] private KeyCode stopKey = KeyCode.Escape;
        
        [Header("Auto Test")]
        [SerializeField] private bool autoTest = false;
        [SerializeField] private float testInterval = 3f;
        
        [Header("Preset Modes")]
        [SerializeField] private bool usePreset = false;
        [SerializeField] private PresetMode preset = PresetMode.Horror;
        
        private BloodDripEffect bloodEffect;
        private float autoTimer = 0f;
        private bool isPlaying = false;
        
        public enum PresetMode
        {
            Horror,     // Kinh dị cực độ
            Moderate,   // Vừa phải
            Subtle,     // Nhẹ nhàng
            Extreme     // Quá tải (test performance)
        }
        
        private void Awake()
        {
            bloodEffect = GetComponent<BloodDripEffect>();
            
            if (usePreset)
            {
                ApplyPreset(preset);
            }
        }
        
        private void Update()
        {
            // Manual control
            if (Input.GetKeyDown(startKey))
            {
                Debug.Log("🩸 [Bleeding Test] Starting bleeding effect...");
                bloodEffect.StartDrip();
                isPlaying = true;
            }
            
            if (Input.GetKeyDown(stopKey))
            {
                Debug.Log("🛑 [Bleeding Test] Stopping bleeding effect...");
                bloodEffect.StopDrip();
                isPlaying = false;
            }
            
            // Auto test
            if (autoTest)
            {
                autoTimer += Time.deltaTime;
                
                if (autoTimer >= testInterval)
                {
                    if (isPlaying)
                    {
                        Debug.Log("🔄 [Auto Test] Stopping...");
                        bloodEffect.StopDrip();
                        isPlaying = false;
                    }
                    else
                    {
                        Debug.Log("🔄 [Auto Test] Starting...");
                        bloodEffect.StartDrip();
                        isPlaying = true;
                    }
                    
                    autoTimer = 0f;
                }
            }
        }
        
        private void ApplyPreset(PresetMode mode)
        {
            Debug.Log($"🎨 [Bleeding Test] Applying preset: {mode}");
            
            // Note: Cần expose các field trong BloodDripEffect thành public
            // hoặc tạo method SetPreset() trong BloodDripEffect
            // Đây chỉ là ví dụ
            
            switch (mode)
            {
                case PresetMode.Horror:
                    Debug.Log("   ├── Max Droplets: 12");
                    Debug.Log("   ├── Spawn Interval: 0.1s");
                    Debug.Log("   ├── Glow Intensity: 0.5");
                    Debug.Log("   └── All effects: ENABLED");
                    break;
                    
                case PresetMode.Moderate:
                    Debug.Log("   ├── Max Droplets: 8");
                    Debug.Log("   ├── Spawn Interval: 0.15s");
                    Debug.Log("   ├── Glow Intensity: 0.3");
                    Debug.Log("   └── Splatters: OPTIONAL");
                    break;
                    
                case PresetMode.Subtle:
                    Debug.Log("   ├── Max Droplets: 5");
                    Debug.Log("   ├── Spawn Interval: 0.2s");
                    Debug.Log("   ├── Glow Intensity: 0.2");
                    Debug.Log("   └── Splatters: DISABLED");
                    break;
                    
                case PresetMode.Extreme:
                    Debug.Log("   ├── Max Droplets: 25");
                    Debug.Log("   ├── Spawn Interval: 0.05s");
                    Debug.Log("   ├── Glow Intensity: 0.8");
                    Debug.Log("   └── WARNING: Heavy performance!");
                    break;
            }
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("🩸 BLEEDING TEXT TESTER");
            GUILayout.Space(10);
            
            GUILayout.Label($"Status: {(isPlaying ? "🟢 BLEEDING" : "⚪ STOPPED")}");
            GUILayout.Space(5);
            
            GUILayout.Label($"Controls:");
            GUILayout.Label($"  {startKey} = Start Bleeding");
            GUILayout.Label($"  {stopKey} = Stop Bleeding");
            
            if (autoTest)
            {
                GUILayout.Space(5);
                GUILayout.Label($"Auto Test: ON (every {testInterval}s)");
                float progress = autoTimer / testInterval;
                GUILayout.HorizontalScrollbar(0, progress, 0, 1);
            }
            
            GUILayout.EndArea();
        }
    }
}
