using System.Collections.Generic;
using UnityEngine;
using MultiplatformSpeechRecognizer.SpeechRecognizer;
using MultiplatformSpeechRecognizerNamespace.Interfaces;
using GrammarNamespace;
using DictionaryNamespace;

using System.Linq;
using System.Reflection;
using System.Resources;

namespace MultiplatformSpeechRecognizerNamespace
{
    /// <summary>
    /// доступные языковые модели
    /// </summary>
    public enum Language
    {
        eng,
        ru,
        ger,
        esp,
        fr,
        it
    }
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
        /// <param name="pKeyword">ключевое слово инициирующее поиск (ok google)</param>
        /// <param name="pThreshold">порог срабатывания ключеового слова</param>
        public void init(Language pLanguage = Language.eng, GrammarFileStruct[] pGrammars = null, string pKeyword = "", double pThreshold = 1e+10f)
        {
            //readDictionaryFromResources("");
            if (_speechRecognizer == null)
                return;
            if (pGrammars == null)
                return;
            if (pGrammars.Length == 0)
                return;
            // все слова в нижний регистр
            foreach (GrammarFileStruct grammar in pGrammars)
            {
                for (int i = 0; i < grammar.words.Length; i++)
                {
                    grammar.words[i] = grammar.words[i].ToLower();
                }
            }
            _speechRecognizer.keywordThreshold = pThreshold;
            bool isOk = initFileSystem(pLanguage.ToString(), pGrammars, pKeyword.ToLower());
            if (isOk)
                initSpeechRecognizer(pLanguage.ToString(), pGrammars, pKeyword.ToLower());
            else
            {
                Debug.Log("error on init file system");
            }
        }

        private void readDictionaryFromResources(string language)
        {
            ResourceManager rm = new ResourceManager("MultiplatformSpeechRecognizer.Dictionaries", Assembly.GetExecutingAssembly());
            if (rm != null)
            {
                //rm.GetObject("EngDictionary");
                //resources.GetStream("EngDictionary");

                string dataText = rm.GetString("EngDictionary");
                Dictionary<string, string> transriptionContainer = dataText.TrimEnd('\n').Split('\n').ToDictionary(item => item.Split(' ')[0], item => item.ToString());
                log.getLogMessages("word count:" + transriptionContainer.Count);
                log.getLogMessages("found resources");
            }
            else
                log.getLogMessages("no found resources");
        }

        /// <summary>
        /// формируем иерархию папок, актуальный словарь и файлы грамматики на основании массива структур грамматики
        /// </summary>
        /// <param name="pLanguage">целевой язык</param>
        /// <param name="pGrammars">массив структур грамматики(имя грамматики, массив слов)</param>
        /// <param name="pKeyword">ключевое слово инициирующее поиск (ok google)</param>
        /// <returns></returns>
        private bool initFileSystem(string pLanguage, GrammarFileStruct[] pGrammars, string pKeyword = "")
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
            isOk = initDictionary(targetPath, pLanguage, pGrammars, pKeyword);
            if (isOk)
            {
                initGrammarFiles(targetPath, pLanguage, pGrammars);
            }
            return isOk;
        }

        private void initSpeechRecognizer(string pLanguage, GrammarFileStruct[] pGrammars, string pKeyword = "")
        {
            _speechRecognizer.initialization(pLanguage, pGrammars, pKeyword);
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
        /// <param name="pKeyword">ключевое слово инициирующее поиск (ok google)</param>
        /// <returns></returns>
        private bool initDictionary(string pTargetPath, string pLanguage, GrammarFileStruct[] pGrammars, string pKeyword = "")
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
            if (pKeyword != string.Empty)
                wordList.Add(pKeyword);
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
            log = pMessagesReciever;// удалить
            if (_speechRecognizer != null)
                _speechRecognizer.logFromRecognizer += pMessagesReciever.getLogMessages;
        }
        IGetLogMessages log = null; // удалить
        /// <summary>
        ///  устанавливает связь сигнала со слотом получения сообщений об ошибках для вывода в лог
        /// </summary>
        /// <param name="pCrashMessReciever">интерфейсная ссылка на объект приёмник</param>
        public void setCrashMessagesRecieverMethod(IGetCrashMessages pCrashMessReciever)
        {
            if (_speechRecognizer != null)
                _speechRecognizer.errorMessage += pCrashMessReciever.getCrashMessages;
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
