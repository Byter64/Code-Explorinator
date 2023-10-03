using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeExplorinator
{
    public class BackendProgram
    {
        /*
        public BackendProgram()
        {
            FileScanner.ScanAllFiles();
        }
        */

        [MenuItem("Window/Code Explorinator")]
        public static void Init()
        {
            FileScanner.ScanAllFiles();
        }
        
        
        
        public void GetAllPlaceholderNames()
        {
            
        }
        
        public State state
        {
            get;
            private set;
        }
        
        public record State{
            
        }

        public record Input
        {
            //referenztiefe placeholder
            public int searchRadius = 3;
            //fokusklasse
            //oder fokusklassen

        }
    }
}


