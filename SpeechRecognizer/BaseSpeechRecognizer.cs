using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GrammarNamespace;

namespace MultiplatformSpeechRecognizer.SpeechRecognizer
{
    public abstract class BaseSpeechRecognizer : MonoBehaviour
    {
        protected bool _init = false;

        public delegate void ReturnStringValue(string value);
        public delegate void ReturnBoolValue(bool value);
        // лог из библиотеки распознования
        public ReturnStringValue logFromRecognizer;
        // промежуточный результат распознавания
        public ReturnStringValue partialRecognitionResult;
        // результат распознавания
        public ReturnStringValue recognitionResult;
        // результат инициализации распознавателя
        public ReturnBoolValue initializationResult;

        public abstract void startListening();

        public abstract void stopListening();

        public abstract void switchGrammar(string grammarName);

        public abstract void initialization(string language, GrammarFileStruct[] grammars);

        #region baseGrammar
        protected string _baseGrammar = string.Empty;
        public void setBaseGrammar(string grammarName)
        {
            _baseGrammar = grammarName;
        }

        protected void getBaseGrammar(GrammarFileStruct[] grammars)
        {
            if (_baseGrammar == string.Empty)
            {
                _baseGrammar = grammars[0].name;
            }
        }
        #endregion
    }
}
