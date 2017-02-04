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
        private BaseSpeechRecognizer _speechRecognizer = null;

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="parent">родительский объект unity на который будет добавлен компонент BaseSpeechRecognizer</param>
        public MultiplatformSpeechRecognizer(MonoBehaviour parent)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android: parent.gameObject.AddComponent<AndroidSpeechRecognizer>(); break;
                case RuntimePlatform.WindowsEditor: parent.gameObject.AddComponent<WindowsSpeechRecognizer>(); break;
                case RuntimePlatform.WindowsPlayer: parent.gameObject.AddComponent<WindowsSpeechRecognizer>(); break;
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
            {
                logFromRecognizer("speech is empty");
                return;
            }
                
            // все слова в нижний регистр
            foreach (GrammarFileStruct grammar in grammars)
            {
                for (int i = 0; i < grammar.words.Length; i++)
                {
                    grammar.words[i] = grammar.words[i].ToLower();
                }
            }

            bool isOk = initFileSystem(language, grammars);
            if (isOk)
                initSpeechRecognizer(language, grammars);
            else
            {
                logFromRecognizer("error on init file system");
                Debug.Log("error on init file system");
            }
        }
        /// <summary>
        /// формируем иерархию папок, актуальный словарь и файлы грамматики на основании массива структур грамматики
        /// </summary>
        /// <param name="language">целевой язык</param>
        /// <param name="grammars">массив структур грамматики(имя грамматики, массив слов)</param>
        /// <returns></returns>
        private bool initFileSystem(string language, GrammarFileStruct[] grammars)
        {
            string targetPath = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android: targetPath = Application.persistentDataPath; break;
                case RuntimePlatform.WindowsEditor: targetPath = Application.streamingAssetsPath; break;
                case RuntimePlatform.WindowsPlayer: targetPath = Application.streamingAssetsPath; break;
                case RuntimePlatform.LinuxPlayer:; break;
            }
            logFromRecognizer("targetPath:" + targetPath);
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
        /// <summary>
        /// создаём файлы грамматики
        /// </summary>
        /// <param name="targetPath">целевая директория куда будут скопированы файлы грамматики</param>
        /// <param name="language">целевой язык(базовая дирректория)</param>
        /// <param name="grammars">массив структур грамматики(имя грамматики, массив слов)</param>
        private void initGrammarFiles(string targetPath, string language, GrammarFileStruct[] grammars)
        {
            GrammarFilesCreator grammarFileCreator = new GrammarFilesCreator(targetPath, language);
            grammarFileCreator.createGrammarFiles(grammars);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPath">целевая директория куда будут скопирован актуальный словарь</param>
        /// <param name="language">целевой язык(базовая дирректория)</param>
        /// <param name="grammars">массив структур грамматики(имя грамматики, массив слов)</param>
        /// <returns></returns>
        private bool initDictionary(string targetPath, string language, GrammarFileStruct[] grammars)
        {
            string sourcePath = Application.streamingAssetsPath;
            DictionaryFileCreator dict = new DictionaryFileCreator(sourcePath, targetPath, language);
            List<string> wordList = new List<string>();
            foreach (GrammarFileStruct grammar in grammars)
            {
                foreach (string word in grammar.words)
                {
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

        public BaseSpeechRecognizer.ReturnStringValue logFromRecognizer;

        public void setMessagesFromLogRecieverMethod(IGetLogMessages messagesReciever)
        {
            logFromRecognizer += messagesReciever.getLogMessages;
            if (_speechRecognizer != null)
                _speechRecognizer.logFromRecognizer += messagesReciever.getLogMessages;
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
