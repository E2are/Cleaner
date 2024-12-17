using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TransitionScreenPackage
{
   public class TransitionScreenManager : MonoBehaviour
   {
      [SerializeField] private Animator _animator;
      
      public delegate void FinishedReveal();
      public FinishedReveal FinishedRevealEvent;
      
      public delegate void FinishedHide();
      public FinishedHide FinishedHideEvent;

        AudioSource AS;
        public AudioClip LoadSoundClip;

        private void Start()
        {
            AS = GetComponent<AudioSource>();
            GraphicRaycaster GR = GetComponent<GraphicRaycaster>();
            if ((GR != null))
            {
                GR.enabled = false;
            }
        }
        public void Reveal()
        {
            _animator.SetTrigger("Reveal");
        }

        public void Hide()
        {
            _animator.SetTrigger("Hide");
        }

        public void PlayLoadSound()
        {
            if (LoadSoundClip != null && AS != null)
            {
                AS.clip = LoadSoundClip;
                AS.Play();
            }
            GraphicRaycaster GR = GetComponent<GraphicRaycaster>();
            if ((GR != null))
            {
                GR.enabled = false;
            }
        }

      public void OnFinishedHideAnimation()
      {
            // Subscribe to this event, if you'd like to know when it gets hidden
            GetComponent<Animator>().SetBool("AnimFullyLoaded", true);
            FinishedHideEvent?.Invoke();
      }
      
      public void OnFinishedRevealAnimation()
      {
            // Subscribe to this event, if you'd like to know when it's revealed
            GetComponent<Animator>().SetBool("AnimFullyLoaded", true);
            GraphicRaycaster GR = GetComponent<GraphicRaycaster>();
            if ((GR != null))
            {
                GR.enabled = true;
            }
            FinishedRevealEvent?.Invoke();
      }
   }
}
