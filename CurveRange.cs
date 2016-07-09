// ----------------------------------------------------------------
// Galaxia
// ©2016 Simeon Radivoev
// Written by Simeon Radivoev (simeonradivoev@gmail.com)
// ----------------------------------------------------------------
using UnityEngine;

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
