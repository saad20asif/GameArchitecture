using UnityEngine;

namespace Featrues.SpinWheel
{
    [System.Serializable]
    public class SliceData
    {
        public int multiplier;
        public float probablity;
        public Color color;

        public SliceData()
        {
            
        }
        public SliceData(JsonSliceFormat jsonSliceFormat)
        {
            multiplier = jsonSliceFormat.multiplier;
            probablity = jsonSliceFormat.probablity;
            if (!ColorUtility.TryParseHtmlString("#" + jsonSliceFormat.colorHex, out color))
            {
                color = Color.white; // fallback
                Debug.LogWarning("Invalid color hex: " + jsonSliceFormat.colorHex);
            }
        }
        public void DebugData()
        {
            Debug.Log("Current Multiplier : " + multiplier);
            Debug.Log("Current Probablity : " + probablity);
        }
    }

    [System.Serializable]
    public class JsonSliceFormat

    {
        public int multiplier;
        public float probablity;
        public string  colorHex;

        public JsonSliceFormat()
        {
            
        }
        public JsonSliceFormat( SliceData sliceData)
        {
            multiplier = sliceData.multiplier;
            probablity = sliceData.probablity;
            colorHex =  ColorUtility.ToHtmlStringRGBA(sliceData.color);
        }

        public void DebugData()
        {
            Debug.Log("Current Multiplier : " + multiplier);
            Debug.Log("Current Probablity : " + probablity);
            Debug.Log("Hex Color value : " + colorHex);
        }
    }
}