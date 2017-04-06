using System.Collections.Generic;
using UnityEngine;
using JavaWrapperMethodNamesNamespace;

internal class AndroidSpeechRecognizer : BaseSpeechRecognizer
{
    /// <summary>
    /// SpeechRecognizer из нативной android библиотеки
    /// </summary>
    private AndroidJavaObject _recognizerActivity = null;
 
    public override void initialization(string pLanguage = "", GrammarFileStruct[] pGrammars = null, string pKeyword = "")
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
        //_recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_RECOGNITION_PARTIAL_RESULT_RECEIVER_METHOD, "onRecognitionPartialResult");
        _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_INITIALIZATION_COMPLETE_METHOD, "onCallbackInitResultFromLib");
        #endregion
        _recognizerActivity.Call(JavaWrapperMethodNames.SET_BASE_GRAMMAR_FILE, _baseGrammar);
        this.logFromRecognizer.Invoke(JavaWrapperMethodNames.RUN_RECOGNIZER_SETUP);
        _recognizerActivity.Call(JavaWrapperMethodNames.RUN_RECOGNIZER_SETUP, pLanguage);
        #region добавляем слова в словарь
        Dictionary<string, string> phonesDict = getWordsPhones(pLanguage, pGrammars, pKeyword);

        foreach (string word in phonesDict.Keys)
        {
            this.logFromRecognizer("add word:" + word + " phones:" + phonesDict[word]);
            _recognizerActivity.Call<bool>(JavaWrapperMethodNames.ADD_WORD_INTO_DICTIONARY, word, phonesDict[word]);
        }
        #endregion
        #region добавляем граматику
        string[] grammar = new string[2];
        foreach (GrammarFileStruct gramm in pGrammars)
        {
            grammar[0] = gramm.name;
            grammar[1] = gramm.toString();
            _recognizerActivity.Call<bool>(JavaWrapperMethodNames.ADD_GRAMMAR_STRING, grammar[0], grammar[1]);
        }
        #endregion
        #region добавляем ключевое слово(ok google) для поиска
        if (pKeyword != string.Empty)
        {
            //setKeyword(pKeyword);
            _recognizerActivity.Call<bool>(JavaWrapperMethodNames.SET_KEYWORD, pKeyword);
            this.logFromRecognizer("add keyword:" + pKeyword);
        }
        else
            Debug.Log("keyword is empty");
        #endregion
    }

    public override void startListening()
    {
        this.logFromRecognizer("startListening:" + _init.ToString());
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

    public override void searchKeyword()
    {
        _recognizerActivity.Call<bool>(JavaWrapperMethodNames.SET_SEARCH_KEYWORD);
    }

    protected override void setKeywordThreshold(double pValue = 10000000000)
    {
            
    }

    void Awake()
    {
        BaseSpeechRecognizer._instance = this;
    }
}
