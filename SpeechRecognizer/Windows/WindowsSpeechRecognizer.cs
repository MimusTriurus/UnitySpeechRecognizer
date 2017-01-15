using UnityEngine;
using GrammarNamespace;
using System.Runtime.InteropServices;
using System.Collections;

namespace MultiplatformSpeechRecognizer.SpeechRecognizer
{
    public class WindowsSpeechRecognizer : BaseSpeechRecognizer
    {
        private const string DLL_NAME = "SpeechRecognizer";

        #region связь с внешним миром
        private void onCallbackLogFromLib(string message)
        {
            if (this.logFromRecognizer != null)
            {
                this.logFromRecognizer.Invoke(message);
            }
        }

        private void onCallbackInitResultFromLib(string message)
        {
            _init = true;
            
            if (this.initializationResult != null)
            {
                if (message == INIT_IS_OK)
                    this.initializationResult.Invoke(true); // исправить на фолс
                else
                    this.logFromRecognizer(message);
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
            Debug.Log(message);
            /*
            if (this.recognitionResult != null)
            {
                this.recognitionResult.Invoke(message);
            }   
            */ 
        }
        #endregion
        #region импортированные из библиотеки статические методы
        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void runRecognizerSetup([MarshalAs(UnmanagedType.LPStr)] string modelPath);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void saveLogIntoFile([MarshalAs(UnmanagedType.Bool)] bool value);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void addGrammar([MarshalAs(UnmanagedType.LPStr)] string grammarName, [MarshalAs(UnmanagedType.LPStr)] string grammarFile);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void setBaseGrammar([MarshalAs(UnmanagedType.LPStr)] string grammarName);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void startListeningMic();

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void stopListeningMic();

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void readMicBuffer();

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void changeGrammar([MarshalAs(UnmanagedType.LPStr)] string grammarName);
        #endregion
        #region колбэки из библиотеки
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FPtr(string str);
        /// <summary>
        /// метод-приёмник результатов распознавания речи из нативной Win библиотеки SpeechRecognizer.dll
        /// </summary>
        /// <param name="t">указатель на метод</param>
        [DllImport(DLL_NAME)]
        private unsafe static extern void setResultRecieverMethod(FPtr t);
        /// <summary>
        /// метод-приёмник промежуточных резульатов распознавания из нативной Win библиотеки SpeechRecognizer.dll
        /// </summary>
        /// <param name="t">указатель на метод</param>
        [DllImport(DLL_NAME)]
        private unsafe static extern void setPartialResultRecieverMethod(FPtr t);
        /// <summary>
        /// метод-приёмник сообщений отладки для лога из нативной Win библиотеки SpeechRecognizer.dll
        /// </summary>
        /// <param name="t">указатель на метод</param>
        [DllImport(DLL_NAME)]
        private unsafe static extern void setMessagesFromLogRecieverMethod(FPtr t);
        /// <summary>
        /// метод-приёмник результатов инициализации SpeechRecognizer из нативной Win библиотеки SpeechRecognizer.dll
        /// </summary>
        /// <param name="t">указатель на метод</param>
        [DllImport(DLL_NAME)]
        private unsafe static extern void setInitResultMethod(FPtr t);
        #endregion

        public override void initialization(string language, GrammarFileStruct[] grammars)
        {
            setMessagesFromLogRecieverMethod(this.onCallbackLogFromLib);
            setResultRecieverMethod(this.onRecognitionResult);
            setPartialResultRecieverMethod(this.onRecognitionPartialResult);
            setInitResultMethod(this.onCallbackInitResultFromLib);
            saveLogIntoFile(true);

            this.logFromRecognizer.Invoke("start initialization");
            this.getBaseGrammar(grammars);
            string[] grammar = new string[2];
            foreach (GrammarFileStruct gramm in grammars)
            {
                grammar[0] = gramm.name;
                grammar[1] = gramm.name + ".gram";
                addGrammar(grammar[0], grammar[1]); 
            }

            setBaseGrammar(this._baseGrammar);
            string destination = Application.streamingAssetsPath + "/" + language + "/";
            runRecognizerSetup(destination);
        }

        public override void startListening()
        {
            startListeningMic();
            StartCoroutine(coUpdateWithDelay(_interval));
        }

        public override void stopListening()
        {
            StopCoroutine(coUpdateWithDelay(_interval));
            stopListeningMic();
        }

        public override void switchGrammar(string grammarName)
        {
            changeGrammar(grammarName);
        }
        /// <summary>
        /// таймер-уведомление о том что пора читать буффер микрофона для распознавания
        /// </summary>
        /// <param name="delayTime">интервал в милисекундах</param>
        /// <returns></returns>
        private IEnumerator coUpdateWithDelay(float delayTime)
        {
            float interval = _interval / 1000;
            while (true)
            {
                readMicBuffer();
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
