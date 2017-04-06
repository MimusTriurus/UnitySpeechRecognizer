using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Resources;

/// <summary>
/// базовый класс распознования голоса. От него наследуют классы WindowsSpeechRecognizer, AndroidSpeechRecognizer, LinuxSpeechRecognizer
/// </summary>
internal abstract class BaseSpeechRecognizer : MonoBehaviour
{
    protected const string ERROR_ON_INIT = "crash on init ";
    protected const string ERROR_ON_ADD_GRAMMAR = "crash on add grammar ";
    protected const string ERROR_ON_ADD_WORD = "crash on add word into dictionary ";
    protected const string ERROR_ON_SWITCH_GRAMMAR = "crash on switch grammar ";
    protected const string ERROR_ON_START_LISTENING = "crash on start listening ";

    protected abstract void setKeywordThreshold(double pValue = 1e+10f);
    /// <summary>
    /// порог срабатывания при распознавании ключевого слова
    /// </summary>
    public double keywordThreshold
    {
        set { setKeywordThreshold(value); }
    }
    /// <summary>
    /// результат инициализации speechRecognizer
    /// </summary>
    protected bool _init = false;
    /// <summary>
    /// интервал в милисекундах
    /// </summary>
    protected float _interval = 100;
    /// <summary>
    /// если инициализация speechRecognizer успешна
    /// </summary>
    protected const string INIT_IS_OK = "initComplete";
    /// <summary>
    /// статическая ссылка на самого себя чтобы сборщик мусора не уничтожал его
    /// </summary>
    protected static BaseSpeechRecognizer _instance = null;
    /// <summary>
    /// делегат возврата строки (использует сигналы - logFromRecognizer, partialRecognitionResult, recognitionResult)
    /// </summary>
    /// <param name="value"></param>
    public delegate void ReturnStringValue(string value);
    /// <summary>
    /// делегат возврата булева значения (использует сигнал - initializationResult)
    /// </summary>
    /// <param name="value"></param>
    public delegate void ReturnBoolValue(bool value);
    /// <summary>
    /// сигнал с сообщением отладки из библиотеки распознования
    /// </summary>
    public ReturnStringValue logFromRecognizer;
    /// <summary>
    /// сигнал с сообщением об ошибке
    /// </summary>
    public ReturnStringValue errorMessage;
    /// <summary>
    /// сигнал с промежуточным результатом распознавания
    /// </summary>
    public ReturnStringValue partialRecognitionResult;
    /// <summary>
    /// сигнал с результатом распознавания
    /// </summary>
    public ReturnStringValue recognitionResult;
    /// <summary>
    /// сигнал с результатом инициализации распознавателя
    /// </summary>
    public ReturnBoolValue initializationResult;
    /// <summary>
    /// начало записи голоса с микрофона
    /// </summary>
    public abstract void startListening();
    /// <summary>
    /// окончание записи голоса с микрофона
    /// </summary>
    public abstract void stopListening();
    /// <summary>
    /// смена граматики(перечень слов доступных для распознавания)
    /// </summary>
    /// <param name="grammarName">имя грамматики</param>
    public abstract void switchGrammar(string grammarName);

    public abstract void searchKeyword();
    /// <summary>
    /// инициализируем распознаватель голоса
    /// </summary>
    /// <param name="pLanguage">язык - определяет дирректорию с акустической моделью, словарями и файлами граматики</param>
    /// <param name="pGrammars">массив со структурами грамматики(имя грамматики и массив слов)</param>
    /// <param name="pKeyword">ключевое слово</param>
    public abstract void initialization(string pLanguage = "", GrammarFileStruct[] pGrammars = null, string pKeyword = "");

    #region baseGrammar
    /// <summary>
    /// имя файла грамматики поумолчанию
    /// </summary>
    protected string _baseGrammar = string.Empty;
    /// <summary>
    /// переопределяет файл грамматики поумолчанию
    /// </summary>
    /// <param name="grammarName">имя файла грамматики</param>
    public void setBaseGrammarName(string grammarName)
    {
        _baseGrammar = grammarName;
    }
    /// <summary>
    /// устанавливает файл грамматики поумолчанию равным первому элементу из массива структур грамматики
    /// </summary>
    /// <param name="grammars">массив со структурами грамматики(имя грамматики и массив слов)</param>
    protected void getBaseGrammar(GrammarFileStruct[] grammars)
    {
        if (_baseGrammar == string.Empty)
        {
            _baseGrammar = grammars[0].name;
        }
    }
    #endregion

    /// <summary>
    /// метод-приёмник сообщений отладки из нативной android библиотеки SpeechRecognizer.jar
    /// </summary>
    /// <param name="pMessage"></param>
    protected void onCallbackLogFromLib(string pMessage)
    {
        if (BaseSpeechRecognizer._instance.logFromRecognizer != null)
        {
            BaseSpeechRecognizer._instance.logFromRecognizer(pMessage);
        }
    }
    /// <summary>
    /// метод-приёмник результатов инициализации SpeechRecognizer из нативной android библиотеки SpeechRecognizer.jar
    /// </summary>
    /// <param name="pMessage"></param>
    protected void onCallbackInitResultFromLib(string pMessage)
    {
        logFromRecognizer("result:" + pMessage);
        if (BaseSpeechRecognizer._instance.initializationResult != null)
        {
            if (pMessage == INIT_IS_OK)
            {
                BaseSpeechRecognizer._instance.initializationResult(true); // исправить на фолс
                _init = true;
            }
            else
            {
                BaseSpeechRecognizer._instance.initializationResult(false);
                _init = false;
            }
        }
    }
    /// <summary>
    /// метод-приёмник результатов распознавания из нативной android библиотеки SpeechRecognizer.jar
    /// </summary>
    /// <param name="pMessage"></param>
    protected void onRecognitionResult(string pMessage)
    {
        try
        {
            if (BaseSpeechRecognizer._instance != null)
                BaseSpeechRecognizer._instance.recognitionResult(pMessage);
        }
        catch (System.NullReferenceException e)
        {
            this.logFromRecognizer("error:" + e.Message);
        }
    }
    /// <summary>
    /// получаем актуальный словарь
    /// </summary>
    /// <param name="pLanguage">язык словаря</param>
    /// <param name="pGrammars">список слов для внесения в словарь</param>
    /// <param name="pKeyword">ключевое слово</param>
    /// <returns>актуальный словарь (слово, транскрипция)</returns>
    protected Dictionary<string, string> getWordsPhones(string pLanguage = "", GrammarFileStruct[] pGrammars = null, string pKeyword = "")
    {
        if (pLanguage != string.Empty)
        {
            string dictName = string.Empty;
            switch (pLanguage)
            {
                case "eng": dictName = "EngDictionary"; break;
                case "esp": dictName = "EspDictionary"; break;
                case "fr": dictName = "FrDictionary"; break;
                case "ger": dictName = "GerDictionary"; break;
                case "it": dictName = "ItDictionary"; break;
                case "ru": dictName = "RuDictionary"; break;
            }
            if (dictName != string.Empty)
            {
                Dictionary<string, string> baseDict = readDictionaryFromResources(dictName);
                if (baseDict == null) Debug.Log("getWordsPhones empty dict"); else Debug.Log("getWordsPhones dict");
                return getActualDictionary(ref baseDict, pGrammars, pKeyword);
            }
            else
                return null;
        }
        else
            return null;
    }
    /// <summary>
    /// считывание полного(базового) словаря из ресурсов
    /// </summary>
    /// <param name="pDictName">имя словаря</param>
    /// <returns>словарь (слово, транскрипция)</returns>
    private Dictionary<string, string> readDictionaryFromResources(string pDictName)
    {
        ResourceManager rm = new ResourceManager("MultiplatformSpeechRecognizer.Dictionaries", Assembly.GetExecutingAssembly());
        if (rm != null)
        {
            string dataText = rm.GetString(pDictName);
            Dictionary<string, string> transriptionContainer = dataText.TrimEnd('\n').Split('\n').ToDictionary(item => item.Split(' ')[0], item => item.Remove(0, item.IndexOf(" ") + 1));
            return transriptionContainer;
        }
        else
            return null;
    }
    /// <summary>
    /// формируем актуальный словарь
    /// </summary>
    /// <param name="pDict">полный(базовый) словарь </param>
    /// <param name="pGrammars">слова для внесения в актуальный словарь</param>
    /// <param name="pKeyword">ключевое слово</param>
    /// <returns>актуальный словарь со словами из GrammarFileStruct</returns>
    private Dictionary<string, string> getActualDictionary(ref Dictionary<string, string> pDict, GrammarFileStruct[] pGrammars = null, string pKeyword = "")
    {
        Dictionary<string, string> actualDict = new Dictionary<string, string>();
        foreach (GrammarFileStruct grammar in pGrammars)
        {
            foreach (string word in grammar.words)
            {
                if (pDict.ContainsKey(word))
                {
                    if (!actualDict.ContainsKey(word))
                    {
                        actualDict.Add(word, pDict[word]);
                        Debug.Log("add word:" + word + " phones:" + pDict[word]);
                    }
                    else
                        this.errorMessage("dictionary already contains word [" + word + "]");
                }
                else
                    this.errorMessage("word [" + word + "] not found");
            }
        }
            
        if ((pDict.ContainsKey(pKeyword)) && (!actualDict.ContainsKey(pKeyword)))
            actualDict.Add(pKeyword, pDict[pKeyword]);
        else
            this.errorMessage("keyword [" + pKeyword + "] not found");
            
        return actualDict;
    }
}
