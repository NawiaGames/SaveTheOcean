using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TutorialSystem
{
    public class TutorialStepChestCollect : TutorialStep
    {
        protected override void OnEnabled()
        {
            //RewardChest.onPoped += MoveToNextStep;
        }
        protected override void OnDisabled()
        {
            //RewardChest.onPoped -= MoveToNextStep;
        }
    }
}
