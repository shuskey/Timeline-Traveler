using System;
using UnityEngine;

namespace Assets.Scripts.DataProviders
{
    public class DataProviderBase
    {
        protected int StringToNumberProtected(string stringToConvert, string description)
        {
            int intToReturn = 0;
            try
            {
                intToReturn = Int32.Parse(stringToConvert);
            }
            catch (Exception)
            {
                Debug.Log($"For {description}, attempted to convert {stringToConvert} to an integer, but failed.  Will return 0.");
            }
            return intToReturn;
        }
    }
}
