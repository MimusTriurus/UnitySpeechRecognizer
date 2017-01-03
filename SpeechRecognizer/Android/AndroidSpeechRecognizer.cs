using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarNamespace;
using UnityEngine;
using JavaWrapperMethodNamesNamespace;

namespace MultiplatformSpeechRecognizer.SpeechRecognizer
{
    public class AndroidSpeechRecognizer : BaseSpeechRecognizer
    {
        private AndroidJavaObject _recognizerActivity = null;

        private const string INIT_IS_OK = "ok";

        private void onCallbackLogFromJavaLib(string message)
        {
            if (this.logFromRecognizer != null)
            {
                this.logFromRecognizer.Invoke(message);
            }
        }

        private void onCallbackInitResultFromJavaLib(string message)
        {
            _init = true;
            if (this.initializationResult != null)
            {
                if (message == INIT_IS_OK)
                    this.initializationResult.Invoke(true); // исправить на фолс
                else
                    this.initializationResult.Invoke(true);
            }
        }

        private void onRecognitionPartialResult(string message)
        {
            if (this.partialRecognitionResult != null)
            {
                this.partialRecognitionResult.Invoke(message);
            }
        }

        private void onRecognitionResult(string message)
        {
            if (this.recognitionResult != null)
            {
                this.recognitionResult.Invoke(message);
            }
        }

        public override void initialization(string language, GrammarFileStruct[] grammars)
        {
            this.logFromRecognizer.Invoke("start initialization");
            this.getBaseGrammar(grammars);

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
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_LOG_RECIEVER_METHOD_NAME, "onCallbackLogFromJavaLib");
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_RECOGNITION_RESULT_RECIEVER_METHOD, "onRecognitionResult");
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_RECOGNITION_PARTIAL_RESULT_RECEIVER_METHOD, "onRecognitionPartialResult");
            _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_INITIALIZATION_COMPLETE_METHOD, "onCallbackInitResultFromJavaLib");
            #endregion

            string[] grammar = new string[2];
            foreach (GrammarFileStruct gramm in grammars)
            {
                grammar[0] = gramm.name;
                grammar[1] = gramm.name + ".gram";
                _recognizerActivity.Call(JavaWrapperMethodNames.ADD_GRAMMAR_FILE, grammar);
            }
            _recognizerActivity.Call(JavaWrapperMethodNames.SET_BASE_GRAMMAR_FILE, _baseGrammar);
            _recognizerActivity.Call(JavaWrapperMethodNames.RUN_RECOGNIZER_SETUP, language);
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

        public override void switchGrammar(string grammarName)
        {
            _recognizerActivity.Call(JavaWrapperMethodNames.SWITCH_SEARCH, grammarName);
        }
    }
}
