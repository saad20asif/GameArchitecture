
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Featrues.SpinWheel
{
    public class Slice : MonoBehaviour
    {
        public Image sliceImage;
        public Image coinImage;
        public TMP_Text multiplierText;
        public int multiplierCount; 

        [Header("Settings")]
        [SerializeField] private float spacing = 10f;

        [SerializeField] private float paddingFromEdge = 10f; 
        [SerializeField] private float sizeMultiplier = 2.0f;

        public void SetSliceData(int multiplierCount)
        {
            this.multiplierCount = multiplierCount;
        }

        public void SetSliceUI(Sprite sliceSprite , Sprite coinSprite , Color color )
        {
          
            sliceImage.color = color;
            coinImage.sprite =  coinSprite;
            multiplierText.text = "x " + multiplierCount.ToString();
        }
        public void AdjustLayout()
        {
            RectTransform sliceRectTransform = sliceImage.GetComponent<RectTransform>();
            RectTransform textRectTransform = multiplierText.GetComponent<RectTransform>();
            RectTransform coinRectTransform = coinImage.GetComponent<RectTransform>();

            float fillAmount = sliceImage.fillAmount;
            
            
            float fillAngleDegrees = fillAmount * 360f;
            float sliceCenterAngle = 90f - (fillAngleDegrees / 2f);
            
           
            float sliceCenterRad = sliceCenterAngle * Mathf.Deg2Rad;
            
            float radius = Mathf.Min(sliceRectTransform.rect.width, sliceRectTransform.rect.height) / 2f;
            float positionRadius = radius * 0.5f;
            
           
            Vector2 sliceCenter = new Vector2(
                Mathf.Cos(sliceCenterRad) * positionRadius,
                Mathf.Sin(sliceCenterRad) * positionRadius
            );

            float totalAngle = 360f * fillAmount;
        
            float sliceWidth = Mathf.Sin(totalAngle * Mathf.Deg2Rad * 0.5f) * radius * 2f;
            float scale = Mathf.Clamp(sliceWidth / 150f, 0.1f, 1.0f);

            textRectTransform.localScale = Vector3.one * scale;
            coinRectTransform.localScale = Vector3.one * scale;

            Vector2 coinPosition = sliceCenter;
            Vector2 textPosition = sliceCenter + sliceCenter * 0.5f;
            
            textRectTransform.anchoredPosition = textPosition;
            coinRectTransform.anchoredPosition = coinPosition;

            Vector2 dirToCenter = -textRectTransform.anchoredPosition;

            // Calculate rotation
            float angle = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;

            // Apply rotation
            textRectTransform.localRotation = Quaternion.Euler(0f, 0f, angle +90);
        }
        // Helper method to calculate slice center for visualization
        private Vector2 GetSliceCenter()
        {
            RectTransform sliceRectTransform = sliceImage.GetComponent<RectTransform>();
            float fillAmount = sliceImage.fillAmount;
            float fillAngleDegrees = fillAmount * 360f;
            float sliceCenterAngle = 90f - (fillAngleDegrees / 2f);
            float sliceCenterRad = sliceCenterAngle * Mathf.Deg2Rad;
            
            float radius = Mathf.Min(sliceRectTransform.rect.width, sliceRectTransform.rect.height) / 2f;
            float positionRadius = radius * 0.5f;
            
            return new Vector2(
                Mathf.Cos(sliceCenterRad) * positionRadius,
                Mathf.Sin(sliceCenterRad) * positionRadius
            );
        }
    }
}
