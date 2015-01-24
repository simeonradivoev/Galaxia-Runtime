using UnityEngine;
using System.Collections.Generic;

namespace Galaxia
{
    public class CurveRangeAttribute : PropertyAttribute
    {
        public Rect range;

        public CurveRangeAttribute(float x,float y,float width,float height)
        {
            range = new Rect(x,y,width,height);
        }
    }
}
