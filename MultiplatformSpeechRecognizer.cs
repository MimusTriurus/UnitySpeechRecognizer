using UnityEngine;
using AvailableLanguages;

/// <summary>
/// Обёртка для распознавания голоса под различные платформы
/// </summary>
public class MultiplatformSpeechRecognizer {
    private BaseSpeechRecognizer _speechRecognizer = null;
    /// <summary>
    /// конструктор
    /// </summary>
    /// <param name="parent">родительский объект unity на который будет добавлен компонент BaseSpeechRecognizer</param>
    public MultiplatformSpeechRecognizer( MonoBehaviour parent ) {
        switch ( Application.platform ) {
            case RuntimePlatform.Android: parent.gameObject.AddComponent<AndroidSpeechRecognizer>( ); break;
            case RuntimePlatform.WindowsEditor: parent.gameObject.AddComponent<WindowsSpeechRecognizer>( ); break;
            case RuntimePlatform.WindowsPlayer: parent.gameObject.AddComponent<WindowsSpeechRecognizer>( ); break;
            case RuntimePlatform.LinuxPlayer: parent.gameObject.AddComponent<WindowsSpeechRecognizer>( ); break;
        }
        Debug.Log( "Platform:" + Application.platform );
        _speechRecognizer = parent.GetComponent<BaseSpeechRecognizer>( );
        if ( _speechRecognizer == null ) {
            Debug.Log( "empty component speechRecognizer" );
            return;
        }
    }
    /// <summary>
    /// инициализация платформозависимого распознавателя голоса 
    /// </summary>
    /// <param name="language">язык - для выбора директории с языковой моделью и словарём</param>
    /// <param name="grammars">массив грамматик со словами</param>
    /// <param name="keyword">ключевое слово инициирующее поиск (ok google)</param>
    /// <param name="threshold">порог срабатывания ключеового слова</param>
    public void init( string language = Language.en_US, GrammarFileStruct[ ] grammars = null, string keyword = "", double threshold = 1e+10f ) {
        //readDictionaryFromResources("");
        if ( _speechRecognizer == null )
            return;
        if ( grammars == null )
            return;
        if ( grammars.Length == 0 )
            return;
        // все слова в нижний регистр
        foreach ( GrammarFileStruct grammar in grammars ) {
            for ( int i = 0; i < grammar.words.Length; i++ ) {
                grammar.words[ i ] = grammar.words[ i ].ToLower( );
            }
        }
        _speechRecognizer.keywordThreshold = threshold;
        initSpeechRecognizer( language, grammars, keyword );
    }

    private void initSpeechRecognizer( string language, GrammarFileStruct[ ] grammars, string keyword ) {
        _speechRecognizer.initialization( language, grammars, keyword );
    }

    #region определяем методы-приёмники результатов работы библиотеки распознавания
    /// <summary>
    /// устанавливает связь сигнала со слотом получения результатов распознавания
    /// </summary>
    /// <param name="resultReciever">интерфейсная ссылка на объект приёмник</param>
    public void setResultRecieverMethod( IGetResult resultReciever ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.recognitionResult += resultReciever.getResult;
    }
    /// <summary>
    /// устанавливает связь сигнала со слотом получения сообщений для вывода в лог
    /// </summary>
    /// <param name="messagesReciever">интерфейсная ссылка на объект приёмник</param>
    public void setMessagesFromLogRecieverMethod( IGetLogMessages messagesReciever ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.logFromRecognizer += messagesReciever.getLogMessages;
    }
    /// <summary>
    ///  устанавливает связь сигнала со слотом получения сообщений об ошибках для вывода в лог
    /// </summary>
    /// <param name="crashMessReciever">интерфейсная ссылка на объект приёмник</param>
    public void setCrashMessagesRecieverMethod( IGetCrashMessages crashMessReciever ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.errorMessage += crashMessReciever.getCrashMessages;
    }
    /// <summary>
    /// устанавливает связь сигнала со слотом получения результатов инициализации
    /// </summary>
    /// <param name="initResultReciever"></param>
    public void setInitResultRecieverMethod( IGetInitResult initResultReciever ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.initResult += initResultReciever.initComplete;
    }
    #endregion
    /// <summary>
    /// микрофон на запись - начало распознавания
    /// </summary>
    public void startListening( ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.startListening( );
    }
    /// <summary>
    /// отключение микрофона - конец распознавания
    /// </summary>
    public void stopListening( ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.stopListening( );
    }
    /// <summary>
    /// меняем грамматику.меняем набор слов доступных для распознавания
    /// </summary>
    /// <param name="grammarName">имя грамматики или ключевое слово</param>
    public void switchGrammar( string grammarName ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.switchGrammar( grammarName );
    }
    /// <summary>
    /// инициализируем поиск ключевого слова (OK GOOGLE)
    /// </summary>
    public void searchKeyword( ) {
        if ( _speechRecognizer != null )
            _speechRecognizer.searchKeyword( );
    }
}
