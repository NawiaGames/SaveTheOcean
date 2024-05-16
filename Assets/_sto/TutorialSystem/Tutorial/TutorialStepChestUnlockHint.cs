using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TutorialSystem
{
    public class TutorialStepChestUnlockHint : TutorialStep
    {
        protected override void OnEnabled(){
            //RewardChest2.onReward += MoveToNextStep;
        }
        protected override void OnDisabled(){
            //RewardChest2.onReward -= MoveToNextStep;
        }
    }
}
