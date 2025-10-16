using UnityEngine;
using TMPro;
using System.Collections;

namespace MadKnight.UI
{
    /// <summary>
    /// Hiệu ứng typewriter cho text (optional - cho subtitle hoặc dialog)
    /// </summary>
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private AudioClip typingSound;
        
        private string fullText;
        private AudioSource audioSource;
        
        private void Start()
        {
            if (textComponent == null)
            {
                textComponent = GetComponent<TextMeshProUGUI>();
            }
            
            fullText = textComponent.text;
            textComponent.text = "";
            
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = typingSound;
            
            if (playOnStart)
            {
                StartTyping();
            }
        }
        
        public void StartTyping()
        {
            StartCoroutine(TypeText());
        }
        
        private IEnumerator TypeText()
        {
            textComponent.text = "";
            
            foreach (char letter in fullText)
            {
                textComponent.text += letter;
                
                if (typingSound != null && letter != ' ')
                {
                    audioSource.pitch = Random.Range(0.9f, 1.1f);
                    audioSource.PlayOneShot(typingSound, 0.3f);
                }
                
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        
        public void SetText(string newText)
        {
            fullText = newText;
            textComponent.text = "";
        }
    }
}
