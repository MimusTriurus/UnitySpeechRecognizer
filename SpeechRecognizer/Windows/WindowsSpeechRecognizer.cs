using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GrammarNamespace;

namespace MultiplatformSpeechRecognizer.SpeechRecognizer
{
    public class WindowsSpeechRecognizer : BaseSpeechRecognizer
    {
        public override void initialization(string language, GrammarFileStruct[] grammars)
        {
            
        }

        public override void startListening()
        {
            
        }

        public override void stopListening()
        {
            
        }

        public override void switchGrammar(string grammarName)
        {

        }

        void Awake()
        {
            Debug.Log("Awake");
        }

        void Start()
        {
            Debug.Log("Start");
        }
    }
}
