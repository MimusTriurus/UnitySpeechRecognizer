using System.Collections.Generic;
using UnityEngine;
using MultiplatformSpeechRecognizer.SpeechRecognizer;
using MultiplatformSpeechRecognizer.Interfaces;
using GrammarNamespace;
using DictionaryNamespace;

namespace MultiplatformSpeechRecognizerNamespace
{
    /// <summary>
    /// Обёртка для распознавания голоса под различные платформы
    /// </summary>
    public class MultiplatformSpeechRecognizer
    {
        private BaseSpeechRecognizer _speechRecognizer = null;

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="pParent">родительский объект unity на который будет добавлен компонент BaseSpeechRecognizer</param>
        public MultiplatformSpeechRecognizer(MonoBehaviour pParent)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android: pParent.gameObject.AddComponent<AndroidSpeechRecognizer>(); break;
                case RuntimePlatform.WindowsEditor: pParent.gameObject.AddComponent<WindowsSpeechRecognizer>(); break;
                case RuntimePlatform.WindowsPlayer: pParent.gameObject.AddComponent<WindowsSpeechRecognizer>(); break;
                case RuntimePlatform.LinuxPlayer:; break;
            }
            _speechRecognizer = pParent.GetComponent<BaseSpeechRecognizer>();
            if (_speechRecognizer == null)
            {
                Debug.Log("empty component speechRecognizer");
                return;
            }
        }

        #region инициализация
        /// <summary>
        /// инициализация платформозависимого распознавателя голоса 
        /// </summary>
        /// <param name="pLanguage">язык - для выбора директории с языковой моделью и словарём</param>
        /// <param name="pGrammars">массив грамматик со словами</param>
        public void init(string pLanguage, GrammarFileStruct[] pGrammars)
        {
            if (_speechRecognizer == null)
            {
                Debug.Log("speech is empty");
                return;
            }
                
            // все слова в нижний регистр
            foreach (GrammarFileStruct grammar in pGrammars)
            {
                for (int i = 0; i < grammar.words.Length; i++)
                {
                    grammar.words[i] = grammar.words[i].ToLower();
                }
            }

            bool isOk = initFileSystem(pLanguage, pGrammars);
            if (isOk)
                initSpeechRecognizer(pLanguage, pGrammars);
            else
            {
                Debug.Log("error on init file system");
            }
        }
        /// <summary>
        /// формируем иерархию папок, актуальный словарь и файлы грамматики на основании массива структур грамматики
        /// </summary>
        /// <param name="pLanguage">целевой язык</param>
        /// <param name="pGrammars">массив структур грамматики(имя грамматики, массив слов)</param>
        /// <returns></returns>
        private bool initFileSystem(string pLanguage, GrammarFileStruct[] pGrammars)
        {
            string targetPath = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android: targetPath = Application.persistentDataPath; break;
                case RuntimePlatform.WindowsEditor: targetPath = Application.streamingAssetsPath; break;
                case RuntimePlatform.WindowsPlayer: targetPath = Application.streamingAssetsPath; break;
                case RuntimePlatform.LinuxPlayer:; break;
            }
            bool isOk = false;
            isOk = initDictionary(targetPath, pLanguage, pGrammars);
            if (isOk)
            {
                initGrammarFiles(targetPath, pLanguage, pGrammars);
            }
            return isOk;
        }

        private void initSpeechRecognizer(string pLanguage, GrammarFileStruct[] pGrammars)
        {
            _speechRecognizer.initialization(pLanguage, pGrammars);
        }
        /// <summary>
        /// создаём файлы грамматики
        /// </summary>
        /// <param name="pTargetPath">целевая директория куда будут скопированы файлы грамматики</param>
        /// <param name="pLanguage">целевой язык(базовая дирректория)</param>
        /// <param name="pGrammars">массив структур грамматики(имя грамматики, массив слов)</param>
        private void initGrammarFiles(string pTargetPath, string pLanguage, GrammarFileStruct[] pGrammars)
        {
            GrammarFilesCreator grammarFileCreator = new GrammarFilesCreator(pTargetPath, pLanguage);
            grammarFileCreator.createGrammarFiles(pGrammars);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTargetPath">целевая директория куда будут скопирован актуальный словарь</param>
        /// <param name="pLanguage">целевой язык(базовая дирректория)</param>
        /// <param name="pGrammars">массив структур грамматики(имя грамматики, массив слов)</param>
        /// <returns></returns>
        private bool initDictionary(string pTargetPath, string pLanguage, GrammarFileStruct[] pGrammars)
        {
            string sourcePath = Application.streamingAssetsPath;
            DictionaryFileCreator dict = new DictionaryFileCreator(sourcePath, pTargetPath, pLanguage);
            List<string> wordList = new List<string>();
            foreach (GrammarFileStruct grammar in pGrammars)
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
        /// <summary>
        /// устанавливает связь сигнала со слотом получения результатов распознавания
        /// </summary>
        /// <param name="pResultReciever">интерфейсная ссылка на объект приёмник</param>
        public void setResultRecieverMethod(IGetResult pResultReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.recognitionResult += pResultReciever.getResult;
        }
        /// <summary>
        /// устанавливает связь сигнала со слотом получения промежуточных результатов распознавания
        /// </summary>
        /// <param name="pResultReciever">интерфейсная ссылка на объект приёмник</param>
        public void setPartialResultRecieverMethod(IGetPartialResult pResultReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.partialRecognitionResult += pResultReciever.getPartialResult;
        }
        /// <summary>
        /// устанавливает связь сигнала со слотом получения сообщений для вывода в лог
        /// </summary>
        /// <param name="pMessagesReciever">интерфейсная ссылка на объект приёмник</param>
        public void setMessagesFromLogRecieverMethod(IGetLogMessages pMessagesReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.logFromRecognizer += pMessagesReciever.getLogMessages;
        }
        /// <summary>
        /// устанавливает связь сигнала со слотом получения результатов инициализации распознавателя голоса в соответствующей библиотеке
        /// </summary>
        /// <param name="pResultReciever"></param>
        public void setInitResultMethod(IGetSpeechRecognizerInitResult pResultReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.initializationResult += pResultReciever.getSpeechRecognizerInitResult;
        }
        #endregion
        /// <summary>
        /// микрофон на запись - начало распознавания
        /// </summary>
        public void startListening()
        {
            if (_speechRecognizer != null)
                _speechRecognizer.startListening();
        }
        /// <summary>
        /// отключение микрофона - конец распознавания
        /// </summary>
        public void stopListening()
        {
            if (_speechRecognizer != null)
                _speechRecognizer.stopListening();
        }
        /// <summary>
        /// меняем грамматику.меняем набор слов доступных для распознавания
        /// </summary>
        /// <param name="pGrammarName">имя грамматики</param>
        public void switchGrammar(string pGrammarName)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.switchGrammar(pGrammarName);
        }
    }
}
