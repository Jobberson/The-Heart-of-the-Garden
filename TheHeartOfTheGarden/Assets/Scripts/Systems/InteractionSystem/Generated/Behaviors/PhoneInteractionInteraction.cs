using UnityEngine;
using Snog.InteractionSystem.Core.Interfaces;
using Snog.Audio;

namespace Snog.InteractionSystem.Behaviors
{
    public class PhoneInteractionInteraction : MonoBehaviour, IInteractionBehavior
    {
        public void Execute(GameObject target)
        {
            Debug.Log("PhoneInteraction interaction executed on " + target.name);

            //AudioManager.Instance.PlayMusic("MixdownShort", 3f);
            var phoneAudioSource = target.GetComponentInParent<AudioSource>();
            phoneAudioSource.Stop();

            PostProcessManager.Instance.ApplyColorTint(new(0.9f, 0.95f, 1f), -10, 1, 1);

            var mirrorMove = FindAnyObjectByType<MoveBehindPlayer>();
            if (mirrorMove != null)
                mirrorMove.MoveBehind();
                
            var trigger1 = GameObject.Find("ColorLookAtPhone");
            var trigger2 = GameObject.Find("ColorNotLookAtPhone");
            
            if (trigger1 != null)
                trigger1.SetActive(false);
                
            if (trigger2 != null)
                trigger2.SetActive(false);
        }
    }
}