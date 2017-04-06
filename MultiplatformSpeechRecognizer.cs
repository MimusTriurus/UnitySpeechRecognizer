using UnityEngine;

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
    /// <summary>
    /// инициализация платформозависимого распознавателя голоса 
    /// </summary>
    /// <param name="pLanguage">язык - для выбора директории с языковой моделью и словарём</param>
    /// <param name="pGrammars">массив грамматик со словами</param>
    /// <param name="pKeyword">ключевое слово инициирующее поиск (ok google)</param>
    /// <param name="pThreshold">порог срабатывания ключеового слова</param>
    public void init(string pLanguage = "eng", GrammarFileStruct[] pGrammars = null, string pKeyword = "", double pThreshold = 1e+10f)
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

        initSpeechRecognizer(pLanguage, pGrammars, pKeyword.ToLower());
    }
    private void initSpeechRecognizer(string pLanguage, GrammarFileStruct[] pGrammars, string pKeyword = "")
    {
        _speechRecognizer.initialization(pLanguage, pGrammars, pKeyword);
    }

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
    /// <param name="pGrammarName">имя грамматики или ключевое слово</param>
    public void switchGrammar(string pGrammarName)
    {
        if (_speechRecognizer != null)
            _speechRecognizer.switchGrammar(pGrammarName);
    }
    /// <summary>
    /// инициализируем поиск ключевого слова (OK GOOGLE)
    /// </summary>
    public void searchKeyword()
    {
        if (_speechRecognizer != null)
            _speechRecognizer.searchKeyword();
    }
}
