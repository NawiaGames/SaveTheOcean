using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
using GameLib.UI;
using GameLib.InputSystem;

namespace TutorialSystem
{
    public abstract class TutorialLogic : MonoBehaviour
    {
        [SerializeField] protected TutorialSequence[] tutorialSequence = new TutorialSequence[]{};

        public void SetSender(Transform transform){
            foreach (var tutorial in tutorialSequence)
                if (tutorial.sender == null) tutorial.sender = transform;
        }

        public void ActivateTutorial(){
            this.enabled = true;
            TutorialManger.Instance?.RequestTutorial(tutorialSequence);
        }
        public void ProgressTutorial(){
            TutorialManger.Instance?.ProgressTutorial();
            if (!TutorialManger.IsTutorialActive){
                this.enabled = false;
            }   
        }
    }
}