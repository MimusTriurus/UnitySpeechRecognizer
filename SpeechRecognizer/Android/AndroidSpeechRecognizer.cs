using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarNamespace;
using UnityEngine;
using JavaWrapperMethodNamesNamespace;

namespace MultiplatformSpeechRecognizer.SpeechRecognizer
{
    internal class AndroidSpeechRecognizer : BaseSpeechRecognizer
    {
        /// <summary>
        /// SpeechRecognizer из нативной android библиотеки
        /// </summary>
        private AndroidJavaObject _recognizerActivity = null;
 
        public override void initialization(string pLanguage, GrammarFileStruct[] pGrammars)
        {
            this.logFromRecognizer.Invoke("start initialization");
            this.getBaseGrammar(pGrammars);

            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (unity == null)
            {
                this.logFromRecognizer.Invoke("empty java class");
                return;
            }

            _recognizerActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            if (_recognizerActivity == null)
            {
                this.logFromRecognizer.Invoke("empty java object");
                return;
            }

            #region инициализируем колбэк из jar библиотеки
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_RECIEVER_OBJECT_NAME, this.gameObject.name);
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_LOG_RECIEVER_METHOD_NAME, "onCallbackLogFromLib");
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_RECOGNITION_RESULT_RECIEVER_METHOD, "onRecognitionResult");
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_RECOGNITION_PARTIAL_RESULT_RECEIVER_METHOD, "onRecognitionPartialResult");
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_INITIALIZATION_COMPLETE_METHOD, "onCallbackInitResultFromLib");
            #endregion

            string[] grammar = new string[2];
            foreach (GrammarFileStruct gramm in pGrammars)
            {
                grammar[0] = gramm.name;
                grammar[1] = gramm.name + ".gram";
                _recognizerActivity.Call(JavaWrapperMethodNames.ADD_GRAMMAR_FILE, grammar);
            }
            _recognizerActivity.Call(JavaWrapperMethodNames.SET_BASE_GRAMMAR_FILE, _baseGrammar);
            _recognizerActivity.Call(JavaWrapperMethodNames.RUN_RECOGNIZER_SETUP, pLanguage);
        }

        public override void startListening()
        {
            if (_init)
            {
                _recognizerActivity.Call(JavaWrapperMethodNames.START_LISTENING);
            }
        }

        public override void stopListening()
        {
            if (_init)
            {
                _recognizerActivity.Call(JavaWrapperMethodNames.STOP_LISTENING);
            }
        }

        public override void switchGrammar(string pGrammarName)
        {
            _recognizerActivity.Call(JavaWrapperMethodNames.SWITCH_SEARCH, pGrammarName);
        }

        void Awake()
        {
            BaseSpeechRecognizer._instance = this;
        }
    }
}
