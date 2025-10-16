using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace MadKnight.UI
{
    /// <summary>
    /// BLEEDING TEXT EFFECT - Chữ chảy máu/mực kinh dị
    /// Text bị ăn mòn, mực lan tỏa và chảy xuống như máu
    /// </summary>
    public class BloodDripEffect : MonoBehaviour
    {
        [Header("🎨 Visual Settings")]
        [SerializeField] private Color bloodColor = new Color(0.08f, 0, 0, 0.95f); // Đỏ đen
        [SerializeField] private Color glowColor = new Color(0.3f, 0, 0, 0.6f); // Ánh đỏ
        [SerializeField] private float glowPulseSpeed = 2f;
        [SerializeField] private float glowIntensity = 0.5f;
        
        [Header("💧 Dripping Behavior")]
        [SerializeField] private int maxDroplets = 12; // Nhiều giọt nhỏ
        [SerializeField] private float spawnInterval = 0.1f; // Spawn nhanh
        [SerializeField] private float dropletLifetime = 2f;
        [SerializeField] private Vector2 dropletSize = new Vector2(4f, 8f); // Nhỏ, dài
        
        [Header("🌊 Flow Pattern")]
        [SerializeField] private float flowSpeed = 100f;
        [SerializeField] private float gravity = 300f;
        [SerializeField] private float randomSpread = 30f; // Độ lan random
        [SerializeField] private bool enableSplatters = true; // Vệt bắn tung tóe
        
        [Header("✨ Special Effects")]
        [SerializeField] private bool enableCorrosion = true; // Ăn mòn text
        [SerializeField] private float corrosionSpeed = 0.5f;
        [SerializeField] private bool enablePulse = true; // Nhấp nháy
        [SerializeField] private bool enableDistortion = true; // Méo mó
        
        private RectTransform container;
        private TextMeshProUGUI buttonText;
        private List<Droplet> activeDroplets = new List<Droplet>();
        private List<Splatter> splatters = new List<Splatter>();
        private Coroutine effectCoroutine;
        private bool isActive = false;
        
        // Corrosion overlay
        private GameObject corrosionOverlay;
        private Image corrosionImage;
        
        private class Droplet
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public Image image;
            public Vector2 position;
            public Vector2 velocity;
            public float lifetime;
            public float age;
            public float size;
            public bool isSplattered;
        }
        
        private class Splatter
        {
            public GameObject gameObject;
            public Image image;
            public float lifetime;
        }
        
        private void Awake()
        {
            CreateContainer();
            
            // Tìm button text
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                buttonText = GetComponent<TextMeshProUGUI>();
            }
        }
        
        private void CreateContainer()
        {
            if (container == null)
            {
                GameObject containerObj = new GameObject("BleedingEffectContainer");
                containerObj.transform.SetParent(transform, false);
                container = containerObj.AddComponent<RectTransform>();
                container.anchorMin = new Vector2(0.5f, 1f);
                container.anchorMax = new Vector2(0.5f, 1f);
                container.pivot = new Vector2(0.5f, 1f);
                container.anchoredPosition = Vector2.zero;
                container.sizeDelta = new Vector2(300f, 600f);
            }
            
            // Create corrosion overlay
            if (enableCorrosion && corrosionOverlay == null)
            {
                corrosionOverlay = new GameObject("CorrosionOverlay");
                corrosionOverlay.transform.SetParent(transform, false);
                
                RectTransform rt = corrosionOverlay.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                
                corrosionImage = corrosionOverlay.AddComponent<Image>();
                corrosionImage.color = new Color(bloodColor.r, bloodColor.g, bloodColor.b, 0f);
                corrosionImage.raycastTarget = false;
                
                corrosionOverlay.SetActive(false);
            }
        }
        
        public void StartDrip()
        {
            if (isActive) return;
            isActive = true;
            
            if (effectCoroutine != null) StopCoroutine(effectCoroutine);
            effectCoroutine = StartCoroutine(BleedingEffect());
        }
        
        public void StopDrip()
        {
            if (!isActive) return;
            isActive = false;
            
            if (effectCoroutine != null) StopCoroutine(effectCoroutine);
            StartCoroutine(FadeOutEffect());
        }
        
        private IEnumerator BleedingEffect()
        {
            // Activate corrosion
            if (enableCorrosion && corrosionOverlay != null)
            {
                corrosionOverlay.SetActive(true);
                StartCoroutine(CorrosionEffect());
            }
            
            // Pulsing glow
            if (enablePulse && buttonText != null)
            {
                StartCoroutine(PulseEffect());
            }
            
            // Continuous dripping
            float spawnTimer = 0f;
            
            while (isActive)
            {
                spawnTimer += Time.deltaTime;
                
                if (spawnTimer >= spawnInterval && activeDroplets.Count < maxDroplets)
                {
                    SpawnDroplet();
                    spawnTimer = 0f;
                    
                    // Random splatter
                    if (enableSplatters && Random.value > 0.7f)
                    {
                        SpawnSplatter();
                    }
                }
                
                yield return null;
            }
        }
        
        private void SpawnDroplet()
        {
            // Random spawn position từ text
            float randomX = Random.Range(-randomSpread, randomSpread);
            float randomY = Random.Range(-10f, 10f);
            
            GameObject dropletObj = new GameObject("BloodDroplet");
            dropletObj.transform.SetParent(container, false);
            
            RectTransform rt = dropletObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(randomX, randomY);
            
            float size = Random.Range(dropletSize.x, dropletSize.y);
            rt.sizeDelta = new Vector2(size * 0.6f, size);
            
            Image img = dropletObj.AddComponent<Image>();
            img.color = bloodColor;
            img.raycastTarget = false;
            img.sprite = CreateDropletSprite();
            
            Droplet droplet = new Droplet
            {
                gameObject = dropletObj,
                rectTransform = rt,
                image = img,
                position = rt.anchoredPosition,
                velocity = new Vector2(Random.Range(-10f, 10f), -flowSpeed),
                lifetime = dropletLifetime,
                age = 0f,
                size = size,
                isSplattered = false
            };
            
            activeDroplets.Add(droplet);
            StartCoroutine(AnimateDroplet(droplet));
        }
        
        private Sprite CreateDropletSprite()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 center = new Vector2(size / 2f, size / 2f);
                    float distX = Mathf.Abs(x - center.x) / (size / 2f);
                    float distY = Mathf.Abs(y - center.y) / (size / 2f);
                    
                    // Hình giọt nước (oval + đuôi nhọn)
                    float alpha = 0f;
                    if (y < size / 2) // Top half - oval
                    {
                        float dist = Mathf.Sqrt(distX * distX + distY * distY * 0.5f);
                        alpha = 1f - Mathf.Clamp01(dist);
                    }
                    else // Bottom half - tapered
                    {
                        float taper = 1f - ((float)y / size);
                        alpha = (1f - distX) * taper;
                    }
                    
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();
            
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
        
        private IEnumerator AnimateDroplet(Droplet droplet)
        {
            while (droplet.age < droplet.lifetime && isActive)
            {
                // Null check - nếu object bị destroy thì thoát
                if (droplet.gameObject == null || droplet.rectTransform == null || droplet.image == null)
                {
                    activeDroplets.Remove(droplet);
                    yield break;
                }
                
                // Apply gravity
                droplet.velocity.y -= gravity * Time.deltaTime;
                
                // Apply velocity
                droplet.position += droplet.velocity * Time.deltaTime;
                droplet.rectTransform.anchoredPosition = droplet.position;
                
                // Wobble
                float wobble = Mathf.Sin(droplet.age * 10f) * 2f;
                Vector2 pos = droplet.position;
                pos.x += wobble;
                droplet.rectTransform.anchoredPosition = pos;
                
                // Fade based on age
                Color col = droplet.image.color;
                col.a = Mathf.Lerp(bloodColor.a, 0f, droplet.age / droplet.lifetime);
                droplet.image.color = col;
                
                // Shrink slightly
                float sizeMultiplier = Mathf.Lerp(1f, 0.5f, droplet.age / droplet.lifetime);
                droplet.rectTransform.sizeDelta = new Vector2(
                    droplet.size * 0.6f * sizeMultiplier,
                    droplet.size * sizeMultiplier
                );
                
                // Check if hit bottom
                if (droplet.position.y < -400f && !droplet.isSplattered)
                {
                    droplet.isSplattered = true;
                    if (enableSplatters)
                    {
                        CreateImpactSplatter(droplet.position);
                    }
                }
                
                droplet.age += Time.deltaTime;
                yield return null;
            }
            
            // Cleanup
            if (droplet.gameObject != null)
            {
                Destroy(droplet.gameObject);
            }
            activeDroplets.Remove(droplet);
        }
        
        private void SpawnSplatter()
        {
            float randomX = Random.Range(-randomSpread * 1.5f, randomSpread * 1.5f);
            float randomY = Random.Range(-50f, 50f);
            
            GameObject splatterObj = new GameObject("Splatter");
            splatterObj.transform.SetParent(container, false);
            
            RectTransform rt = splatterObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(randomX, randomY);
            rt.sizeDelta = new Vector2(Random.Range(2f, 6f), Random.Range(10f, 30f));
            rt.rotation = Quaternion.Euler(0, 0, Random.Range(-30f, 30f));
            
            Image img = splatterObj.AddComponent<Image>();
            img.color = new Color(bloodColor.r, bloodColor.g, bloodColor.b, bloodColor.a * 0.4f);
            img.raycastTarget = false;
            
            Splatter splatter = new Splatter
            {
                gameObject = splatterObj,
                image = img,
                lifetime = 0f
            };
            
            splatters.Add(splatter);
            StartCoroutine(FadeSplatter(splatter));
        }
        
        private void CreateImpactSplatter(Vector2 position)
        {
            // Tạo nhiều vệt nhỏ bắn tung tóe
            int count = Random.Range(3, 6);
            for (int i = 0; i < count; i++)
            {
                GameObject splatterObj = new GameObject($"ImpactSplatter_{i}");
                splatterObj.transform.SetParent(container, false);
                
                RectTransform rt = splatterObj.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = position;
                rt.sizeDelta = new Vector2(Random.Range(2f, 5f), Random.Range(3f, 8f));
                rt.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                
                Image img = splatterObj.AddComponent<Image>();
                img.color = bloodColor;
                img.raycastTarget = false;
                
                // Animate outward
                Vector2 direction = new Vector2(
                    Random.Range(-1f, 1f),
                    Random.Range(-0.5f, 0.5f)
                ).normalized;
                
                StartCoroutine(AnimateImpact(splatterObj, rt, img, direction));
            }
        }
        
        private IEnumerator AnimateImpact(GameObject obj, RectTransform rt, Image img, Vector2 direction)
        {
            float duration = 0.4f;
            float elapsed = 0f;
            Vector2 startPos = rt.anchoredPosition;
            float distance = Random.Range(15f, 40f);
            
            while (elapsed < duration)
            {
                // Null check - nếu object bị destroy thì thoát
                if (obj == null || rt == null || img == null)
                {
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                rt.anchoredPosition = startPos + direction * distance * t;
                
                Color col = img.color;
                col.a = Mathf.Lerp(bloodColor.a, 0f, t);
                img.color = col;
                
                yield return null;
            }
            
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        private IEnumerator FadeSplatter(Splatter splatter)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            
            // Null check ngay đầu
            if (splatter.image == null)
            {
                if (splatter.gameObject != null)
                {
                    Destroy(splatter.gameObject);
                }
                splatters.Remove(splatter);
                yield break;
            }
            
            Color startColor = splatter.image.color;
            
            while (elapsed < duration)
            {
                // Null check trong loop - nếu bị destroy giữa chừng
                if (splatter.gameObject == null || splatter.image == null)
                {
                    splatters.Remove(splatter);
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                Color col = startColor;
                col.a = Mathf.Lerp(startColor.a, 0f, t);
                splatter.image.color = col;
                
                yield return null;
            }
            
            if (splatter.gameObject != null)
            {
                Destroy(splatter.gameObject);
            }
            splatters.Remove(splatter);
        }
        
        private IEnumerator CorrosionEffect()
        {
            float elapsed = 0f;
            
            while (isActive)
            {
                // Null check - nếu corrosionImage bị destroy
                if (corrosionImage == null)
                {
                    yield break;
                }
                
                elapsed += Time.deltaTime * corrosionSpeed;
                
                // Ăn mòn dần text bằng overlay
                Color col = corrosionImage.color;
                col.a = Mathf.Lerp(0f, 0.3f, Mathf.PingPong(elapsed, 1f));
                corrosionImage.color = col;
                
                yield return null;
            }
        }
        
        private IEnumerator PulseEffect()
        {
            if (buttonText == null)
            {
                yield break;
            }
            
            Color originalColor = buttonText.color;
            
            while (isActive)
            {
                // Null check - nếu buttonText bị destroy
                if (buttonText == null)
                {
                    yield break;
                }
                
                float pulse = Mathf.PingPong(Time.time * glowPulseSpeed, 1f);
                Color pulseColor = Color.Lerp(originalColor, glowColor, pulse * glowIntensity);
                buttonText.color = pulseColor;
                
                yield return null;
            }
            
            // Restore
            if (buttonText != null)
            {
                buttonText.color = originalColor;
            }
        }
        
        private IEnumerator FadeOutEffect()
        {
            // Fade out corrosion
            if (corrosionOverlay != null && corrosionImage != null)
            {
                float elapsed = 0f;
                float duration = 0.5f;
                Color startCol = corrosionImage.color;
                
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    Color col = startCol;
                    col.a = Mathf.Lerp(startCol.a, 0f, elapsed / duration);
                    corrosionImage.color = col;
                    yield return null;
                }
                
                corrosionOverlay.SetActive(false);
            }
            
            // Fade out droplets
            List<Droplet> dropletsToFade = new List<Droplet>(activeDroplets);
            foreach (var droplet in dropletsToFade)
            {
                if (droplet?.gameObject != null)
                {
                    Destroy(droplet.gameObject);
                }
            }
            activeDroplets.Clear();
            
            // Fade out splatters
            List<Splatter> splattersToFade = new List<Splatter>(splatters);
            foreach (var splatter in splattersToFade)
            {
                if (splatter?.gameObject != null)
                {
                    Destroy(splatter.gameObject);
                }
            }
            splatters.Clear();
        }
        
        private void OnDestroy()
        {
            StopAllCoroutines();
            
            foreach (var droplet in activeDroplets)
            {
                if (droplet?.gameObject != null)
                {
                    Destroy(droplet.gameObject);
                }
            }
            
            foreach (var splatter in splatters)
            {
                if (splatter?.gameObject != null)
                {
                    Destroy(splatter.gameObject);
                }
            }
            
            activeDroplets.Clear();
            splatters.Clear();
        }
    }
}
