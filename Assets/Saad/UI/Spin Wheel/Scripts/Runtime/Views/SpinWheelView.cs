using System.Collections.Generic;
using DG.Tweening;
using Blues.Core.Events;
using Blues.Core.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Featrues.SpinWheel
{
    public class SpinWheelView : BaseView<SpinWheelRefs>
    {
        [SerializeField] private Int rewardedIndex;
        [SerializeField] private SpinWheelConfigurations spinWheelConfigurations;
        [SerializeField] private GameEvent wheelDataLoaded;
        [SerializeField] private GameEvent OnSliceSelected;
        [SerializeField] private GameEvent OnSpinEnded;

        [SerializeField] private GameObject _sliceGameObject;
        private void OnEnable()
        {
            Subscribe();
        }
        private void OnDisable()
        {
            UnSubscribe();
        }
        [Button]
        public override void Show()
        { 
            this.gameObject.SetActive(true);
        }
        [Button]
        public override void Hide()
        {
            this.gameObject.SetActive(false);
        }
        private void Subscribe()
        {
            //wheelDataLoaded.Subscribe(UpdateWheelView);
            wheelDataLoaded.Subscribe(InstantiateWheelSlices);
            OnSliceSelected.Subscribe(SpinTheWheel);
        }
        private void UnSubscribe()
        {
            //wheelDataLoaded.UnSubscribe( UpdateWheelView);
            wheelDataLoaded.UnSubscribe(InstantiateWheelSlices);
            OnSliceSelected.UnSubscribe(SpinTheWheel);
        }

        private void UpdateWheelView()
        {
            List <SliceData> SlicesData = spinWheelConfigurations.loadedSpinWheelData.SlicesData;
            for( int i=0; i< SlicesData.Count ; i++)
            {
                ViewRefs.slices[i].SetSliceData(SlicesData[i].multiplier);
                ViewRefs.slices[i].SetSliceUI(spinWheelConfigurations.sliceSprite , spinWheelConfigurations.coinSprite, SlicesData[i].color);
            }
            Debug.Log("Updated Wheel UI");
        }
        public void SpinTheWheel()
        {
            int slicesCount = spinWheelConfigurations.loadedSpinWheelData.SlicesData.Count;
            SpinToSliceTransform( ViewRefs.sliceParent.transform,ViewRefs.slices[rewardedIndex.GetValue()].transform,slicesCount , spinWheelConfigurations.totalIterations , spinWheelConfigurations.spinTime);
        }

        public void SpinToSliceTransform(Transform wheel, Transform slice, int totalSlices, int extraRotations , float duration)
        {
            float sliceAngle = 360f / totalSlices;

            // Local Z rotation of the slice
            float sliceZ = slice.localEulerAngles.z;

            // Center the slice (align needle to slice center)
            float sliceCenterOffset = sliceAngle/2;

            // Total target rotation
            float targetRotation = 360f * extraRotations - (sliceZ - sliceCenterOffset);

            // Animate using DOTween
            wheel.DOLocalRotate(
                new Vector3(0, 0, targetRotation),
                duration,
                RotateMode.FastBeyond360
            ).SetEase(Ease.OutCubic)
            .OnComplete( ()=>{
                OnSpinEnded.Invoke();
            });
        }

        private void InstantiateWheelSlices()
        {
            List <SliceData> SlicesData = spinWheelConfigurations.loadedSpinWheelData.SlicesData;

            float fillAmountPerSlice = 1f / SlicesData.Count;
            float rotationStep = 360f / SlicesData.Count;
            ViewRefs.slices.Clear();
            for (int i = 0; i < SlicesData.Count; i++)
            {
                GameObject sliceObj = Instantiate(_sliceGameObject, ViewRefs.sliceParent);
                RectTransform rt = sliceObj.GetComponent<RectTransform>();
                Slice slice = sliceObj.GetComponent<Slice>();

                slice.sliceImage.type = Image.Type.Filled;
                slice.sliceImage.fillAmount = fillAmountPerSlice;
                
                rt.localRotation = Quaternion.Euler(0, 0, -rotationStep * i);
                slice.SetSliceData(SlicesData[i].multiplier);
                slice.SetSliceUI(spinWheelConfigurations.sliceSprite , spinWheelConfigurations.coinSprite, SlicesData[i].color);
                slice.AdjustLayout();
                ViewRefs.slices.Add(slice);
            }
        }
    }
}
