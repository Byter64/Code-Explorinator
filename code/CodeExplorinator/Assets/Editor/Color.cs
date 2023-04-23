using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeExplorinator
{

    /// <summary>
    /// Contains all colors that are used for coloring specific text parts as constants
    /// </summary>
    public static class Color
    {
        public const string accessebility = "#263cd4";
        public const string modifier = "#263cd4";
        public const string variableName = "#4e4e4e";
        public const string methodName = "#4e4e4e";
        public const string parameterName = "#4e4e4e";
        public const string className = "#1e1e1e";
        public const string rest = "#1e1e1e";
        public const string returnType = "#722fc2";
        public const string parameterType = "#722fc2";
        public const string classType = "#722fc2";
        public const string structType = "#722fc2";
        public const string interfaceType = "#722fc2";
        public const string enumType = "#722fc2";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color">a constant from this class or any other conforming color</param>
        /// <returns>a string with colored rich text</returns>
        public static string ColorText(string text, string color)
        {
            return "<color=" + color + ">" + text + "</color>";
        }
    }
}