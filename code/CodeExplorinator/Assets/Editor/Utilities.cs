using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeExplorinator
{
    public static class Utilities
    {

#if UNITY_EDITOR
        public const string pathroot = @"Packages/com.code.explorinator";

#else
        public const string pathroot = @"Assets\";

#endif
    }
}

