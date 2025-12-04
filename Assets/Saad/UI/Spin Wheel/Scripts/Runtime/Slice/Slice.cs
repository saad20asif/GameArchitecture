
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
        // [SerializeField] private float maxScale = 5.0f; 
        // [SerializeField] private float minScale = 4.0f; 

        [SerializeField] private float paddingFromEdge = 10f; // Padding from slice edges
        [SerializeField] private float sizeMultiplier = 2.0f;

        public void SetSliceData(int multiplierCount)
        {
            this.multiplierCount = multiplierCount;
        }

        public void SetSliceUI(Sprite sliceSprite , Sprite coinSprite , Color color )
        {
            //sliceImage.sprite = sliceSprite;
            sliceImage.color = color;
            // Color sliceColor = sliceImage.color;
            //sliceColor.a = 1.0f;
            coinImage.sprite =  coinSprite;
            multiplierText.text = "x " + multiplierCount.ToString();
        }
        public void AdjustLayout()
        {
            RectTransform sliceRectTransform = sliceImage.GetComponent<RectTransform>();
            RectTransform textRectTransform = multiplierText.GetComponent<RectTransform>();
            RectTransform coinRectTransform = coinImage.GetComponent<RectTransform>();

            float fillAmount = sliceImage.fillAmount;
            
            // Calculate the center angle of the filled slice
            // Unity's radial fill starts at top (90 degrees) and goes clockwise
            float fillAngleDegrees = fillAmount * 360f;
            
            // The center of the slice is at half the fill angle
            // Starting from top (90 degrees) going clockwise (negative rotation)
            float sliceCenterAngle = 90f - (fillAngleDegrees / 2f);
            
            // Convert to radians for calculation
            float sliceCenterRad = sliceCenterAngle * Mathf.Deg2Rad;
            
            // Calculate the radius for positioning (2/3 of the image radius works well)
            float radius = Mathf.Min(sliceRectTransform.rect.width, sliceRectTransform.rect.height) / 2f;
            float positionRadius = radius * 0.5f; // Position at 50% of radius
            
            // Calculate the center position of the slice
            Vector2 sliceCenter = new Vector2(
                Mathf.Cos(sliceCenterRad) * positionRadius,
                Mathf.Sin(sliceCenterRad) * positionRadius
            );
            
        
            float arcLength = 2f * Mathf.PI * positionRadius * fillAmount;
            
            // Calculate the radial depth available (from center to edge)
            float radialDepth = radius - positionRadius;
            
            // Get the combined size of text and coin
            float textWidth = textRectTransform.rect.width;
            float textHeight = textRectTransform.rect.height;
            float coinWidth = coinRectTransform.rect.width;
            float coinHeight = coinRectTransform.rect.height;
            
            // Total width needed (text + spacing + coin)
            float totalWidth = textWidth + spacing + coinWidth;
            float maxHeight = Mathf.Max(textHeight, coinHeight);
            
            // Calculate scale to fit within arc length (width constraint)
            float scaleByWidth = (arcLength - paddingFromEdge * 2f) / totalWidth;
            
            // Calculate scale to fit within radial depth (height constraint)
            float scaleByHeight = (radialDepth * 2f - paddingFromEdge * 2f) / maxHeight;
            
            // Use the smaller scale to ensure both constraints are met
            float scale = Mathf.Min(scaleByWidth, scaleByHeight) * sizeMultiplier;
            
            //Clamp to reasonable values
            scale = Mathf.Clamp(scale, 1.0f, 5.0f);
            // float baseScale = fillAmount * sizeMultiplier;
            
            // // Ensure minimum visibility for very small fills
            // if (fillAmount < 0.1f)
            // {
            //     baseScale = 0.1f * sizeMultiplier;
            // }
            
            // float scale = baseScale;

            // Apply scale
            textRectTransform.localScale = Vector3.one * scale;
            coinRectTransform.localScale = Vector3.one * scale;

            Vector2 coinPosition = sliceCenter;

            
            Vector2 textPosition = sliceCenter + sliceCenter * 0.5f;
            
            textRectTransform.anchoredPosition = textPosition;
            coinRectTransform.anchoredPosition = coinPosition;
            
            textRectTransform.localRotation = Quaternion.Euler(0, 0, 330);
            coinRectTransform.localRotation = Quaternion.Euler(0, 0, 330);
            

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
