using System.Collections.Generic;
using UnityEngine;

namespace Featrues.SpinWheel
{
    [System.Serializable]
    public class SpinWheelData 
    {
        public int currentCoins;
        public List <SliceData> SlicesData; 

        public SpinWheelData()
        {
            
        }
        public SpinWheelData( SpinWheelJsonFormat spinWheelJsonFormat)
        {
            this.currentCoins = spinWheelJsonFormat.currentCoins;
            SlicesData =  new List<SliceData>();
            foreach( JsonSliceFormat slicedata in spinWheelJsonFormat.SlicesData )
            {
                SlicesData.Add(new SliceData(slicedata));
            }
        }

        public void ConvertToSpinWheelData(SpinWheelJsonFormat spinWheelJsonFormat )
        {
            this.currentCoins = spinWheelJsonFormat.currentCoins;
            SlicesData.Clear();
            SlicesData =  new List<SliceData>();
            foreach( JsonSliceFormat slicedata in spinWheelJsonFormat.SlicesData )
            {
                SlicesData.Add(new SliceData(slicedata));
            }
        }
    }

    [System.Serializable]
    public class SpinWheelJsonFormat
    {
        public int currentCoins;
        public List<JsonSliceFormat> SlicesData;
        public SpinWheelJsonFormat()
        {
            
        }
        public SpinWheelJsonFormat(SpinWheelData spinWheelData)
        {
            currentCoins = spinWheelData.currentCoins;
            SlicesData = new List<JsonSliceFormat>();
            foreach( SliceData slicedata in spinWheelData.SlicesData )
            {
                SlicesData.Add(new JsonSliceFormat(slicedata));
            }
        }
        public void DebugData()
        {
            Debug.Log("Current Coins : " + currentCoins);
            foreach( JsonSliceFormat slicedata in SlicesData )
            {
                slicedata.DebugData();
            }
        }
    }
}
