using HelloWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.Android;

namespace HelloWorld 
{
    public class MyLameUnityScript
    {
        public delegate string TypName(string aramsamsam);

        TypName meineMethode;

        public int deineMutter = 3, meineMutter, vatte;

        public string HALLO { get; set; }
        // Start is called before the first frame update
        void Start()
        {
            meineMethode += Laina;
            meineMethode.Invoke("Eingabe eins");
            HalloMamie();
            HalloMamie();
            HalloMamie();
            HalloMamie();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void HalloMamie()
        {
            deineMutter = 3 + deineMutter;
        }

        public MyLameUnityScript HallooBawa(int zahl)
        {
            return this;
        }

        public string Laina(string hallole) 
        {
            return hallole;
        }
    }
}

namespace Halloeee
{
    public class BitteHelfenSieMirIchBinInGefahr
    {
        MyLameUnityScript testusSum;
        public extern void Yolol();

        private int GetDeineMAmmer()
        {
            int werrtfeg = 5;
            testusSum.HallooBawa(werrtfeg).HalloMamie();
            return testusSum.deineMutter;
        }
    }
}