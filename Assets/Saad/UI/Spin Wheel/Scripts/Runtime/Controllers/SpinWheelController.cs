using System.Collections;
using System.Collections.Generic;
using Blues.Core.Events;
using Blues.Core.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Featrues.SpinWheel
{
    public class SpinWheelController : MonoBehaviour
    {
        [SerializeField] private SpinWheelConfigurations spinWheelConfigurations;
        [SerializeField] private Int RewardedIndex;
        [SerializeField] private Int playerCoins;
        [SerializeField] private Int multiplier;
        [SerializeField] private GameEvent OnSpinPressed;
        [SerializeField] private GameEvent OnSpinEnded;
        [SerializeField] private Transform wheelTransform;

        private int _nextRewardedIndex;

        private void OnEnable()
        {
            OnSpinPressed.Subscribe(SpinTheWheel);
            OnSpinEnded.Subscribe(SpinEnded);
        }
        private void OnDisable()
        {
            OnSpinPressed.UnSubscribe(SpinTheWheel);
            OnSpinEnded.UnSubscribe(SpinEnded);
        }

        [Button]
        private void SpinTheWheel()
        {
            List<SliceData> slicesData = spinWheelConfigurations.loadedSpinWheelData.SlicesData;
            _nextRewardedIndex = GetRandomSliceIndex(slicesData);
            slicesData[_nextRewardedIndex].DebugData();
            RewardedIndex.SetValue(_nextRewardedIndex);
        }
        private int GetRandomSliceIndex(List<SliceData> slices)
        {
            if(spinWheelConfigurations.isTesting)
            {
                if(spinWheelConfigurations.stopIndex < spinWheelConfigurations.loadedSpinWheelData.SlicesData.Count)
                {
                    return spinWheelConfigurations.stopIndex;
                }
                else
                {
                    return 0; 
                }
            }
            float totalWeight = 0f;

            // 1. Sum all probabilities
            foreach (var slice in slices)
                totalWeight += slice.probablity;

            // 2. Pick a random value within the total
            float randomValue = Random.Range(0f, totalWeight);

            // 3. Find the slice where randomValue falls
            float cumulativeWeight = 0f;

            for (int i = 0; i < slices.Count; i++)
            {
                cumulativeWeight += slices[i].probablity;

                if (randomValue <= cumulativeWeight)
                    return i;
            }

            // Fallback (should never happen)
            return slices.Count - 1;
        }
        private void SpinEnded()
        {
            multiplier.SetValue(spinWheelConfigurations.loadedSpinWheelData.SlicesData[_nextRewardedIndex].multiplier);
            playerCoins.SetValue(CalculateCoins());
        }

        private int CalculateCoins()
        {
            return playerCoins.GetValue() + spinWheelConfigurations.loadedSpinWheelData.currentCoins * multiplier.GetValue();
            // int currentCoins  = playerCoins.GetValue();
            // return currentCoins == 0 ? multiplier.GetValue() : (currentCoins * multiplier.GetValue());  
        }
    }
}
