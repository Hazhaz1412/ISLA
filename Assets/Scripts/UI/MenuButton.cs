using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

namespace MadKnight.UI
{
    /// <summary>
    /// Script cho từng button trong menu với hiệu ứng hover
    /// </summary>
    public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI buttonText;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.6f, 0.6f, 0.6f, 0.7f); // Màu xám mờ
        [SerializeField] private Color hoverColor = Color.white; // Màu trắng khi hover
        [SerializeField] private Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.3f); // Màu xám đậm khi disabled
        
        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private float scaleOnHover = 1.1f;
        
        [Header("Blood/Ink Drip Effect V2")]
        [SerializeField] private bool enableBloodDrip = true;
        [SerializeField] private BloodDripEffect bloodDripEffect;
        
        [Header("Sound")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;
        
        [Header("Events")]
        public UnityEvent onClick;
        
        private Vector3 originalScale;
        private Color targetColor;
        private bool isInteractable = true;
        private AudioSource audioSource;
        
        private void Awake()
        {
            if (buttonText == null)
            {
                buttonText = GetComponent<TextMeshProUGUI>();
            }
            
            originalScale = transform.localScale;
            targetColor = normalColor;
            buttonText.color = normalColor;
            
            // Setup AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            
            // Setup Blood Drip Effect V2
            if (enableBloodDrip && bloodDripEffect == null)
            {
                bloodDripEffect = gameObject.AddComponent<BloodDripEffect>();
            }
        }
        
        private void Update()
        {
            // Smooth color transition
            buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.deltaTime * fadeSpeed);
            
            // Smooth scale transition
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * fadeSpeed);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            targetColor = hoverColor;
            transform.localScale = originalScale * scaleOnHover;
            
            // Start blood drip effect V2
            if (enableBloodDrip && bloodDripEffect != null)
            {
                bloodDripEffect.StartDrip();
            }
            
            // Play hover sound
            if (hoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            targetColor = normalColor;
            transform.localScale = originalScale;
            
            // Stop blood drip effect V2
            if (enableBloodDrip && bloodDripEffect != null)
            {
                bloodDripEffect.StopDrip();
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            // Play click sound
            if (clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }
            
            // Invoke event
            onClick?.Invoke();
        }
        
        /// <summary>
        /// Set button có thể tương tác hay không
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            targetColor = interactable ? normalColor : disabledColor;
            
            if (!interactable)
            {
                transform.localScale = originalScale;
            }
        }
        
        /// <summary>
        /// Set màu custom
        /// </summary>
        public void SetColors(Color normal, Color hover, Color disabled)
        {
            normalColor = normal;
            hoverColor = hover;
            disabledColor = disabled;
            targetColor = isInteractable ? normalColor : disabledColor;
        }
    }
}
