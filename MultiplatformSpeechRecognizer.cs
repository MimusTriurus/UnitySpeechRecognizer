using System.Collections.Generic;
using UnityEngine;
using MultiplatformSpeechRecognizer.SpeechRecognizer;
using MultiplatformSpeechRecognizer.Interfaces;
using GrammarNamespace;
using DictionaryNamespace;

namespace MultiplatformSpeechRecognizerNamespace
{
    public class MultiplatformSpeechRecognizer
    {
        private delegate void ToLog(string message);
        private event ToLog toLog;

        private BaseSpeechRecognizer _speechRecognizer = null;

        public MultiplatformSpeechRecognizer(MonoBehaviour parent)
        {
            //toLog("try initSpeechRecognizer:" + Application.platform.ToString());
            switch (Application.platform)
            {
                case RuntimePlatform.Android: parent.gameObject.AddComponent<AndroidSpeechRecognizer>(); break;
                case RuntimePlatform.WindowsEditor: parent.gameObject.AddComponent<WindowsSpeechRecognizer>(); break;
                case RuntimePlatform.LinuxPlayer:; break;
            }
            _speechRecognizer = parent.GetComponent<BaseSpeechRecognizer>();
            if (_speechRecognizer == null)
            {
                //toLog("empty component");
                return;
            }
        }

        #region инициализация
        public void init(string language, GrammarFileStruct[] grammars)
        {
            if (_speechRecognizer == null)
                return;

            bool isOk = initFileSystem(language, grammars);
            if (isOk)
                initSpeechRecognizer(language, grammars);
        }

        private bool initFileSystem(string language, GrammarFileStruct[] grammars)
        {
            string targetPath = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android: targetPath = Application.persistentDataPath; break;
                case RuntimePlatform.WindowsEditor: targetPath = Application.streamingAssetsPath; break;
                case RuntimePlatform.LinuxPlayer:; break;
            }

            bool isOk = false;
            isOk = initDictionary(targetPath, language, grammars);
            if (isOk)
            {
                initGrammarFiles(targetPath, language, grammars);
            }
            return isOk;
        }

        private void initSpeechRecognizer(string language, GrammarFileStruct[] grammars)
        {
            _speechRecognizer.initialization(language, grammars);
        }

        private void initGrammarFiles(string targetPath, string language, GrammarFileStruct[] grammars)
        {
            GrammarFilesCreator grammarFileCreator = new GrammarFilesCreator(targetPath, language);
            grammarFileCreator.createGrammarFiles(grammars);
        }

        private bool initDictionary(string targetPath, string language, GrammarFileStruct[] grammars)
        {
            string sourcePath = Application.streamingAssetsPath;
            DictionaryFileCreator dict = new DictionaryFileCreator(sourcePath, targetPath, language);
            List<string> wordList = new List<string>();
            foreach (GrammarFileStruct grammar in grammars)
            {
                foreach (string word in grammar.words)
                {
                    //log.add("word:" + word);
                    if (!wordList.Contains(word))
                        wordList.Add(word);
                }
            }
            dict.initDictionary(wordList);
            return dict.isOK;
        }
        #endregion

        #region определяем методы-приёмники результатов работы библиотеки распознавания
        public void setResultRecieverMethod(IGetResult resultReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.recognitionResult += resultReciever.getResult;
        }

        public void setPartialResultRecieverMethod(IGetPartialResult resultReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.partialRecognitionResult += resultReciever.getPartialResult;
        }

        public void setMessagesFromLogRecieverMethod(IGetLogMessages messagesReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.logFromRecognizer += messagesReciever.getLogMessages;
            this.toLog += messagesReciever.getLogMessages;
        }

        public void setInitResultMethod(IGetSpeechRecognizerInitResult resultReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.initializationResult += resultReciever.getSpeechRecognizerInitResult;
        }
        #endregion

        public void startListening()
        {
            if (_speechRecognizer != null)
                _speechRecognizer.startListening();
        }

        public void stopListening()
        {
            if (_speechRecognizer != null)
                _speechRecognizer.stopListening();
        }

        public void switchGrammar(string grammarName)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.switchGrammar(grammarName);
        }
    }
}
