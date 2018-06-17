using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using AvailableLanguages;

/// <summary>
/// класс распознавания голоса для Windows x64
/// </summary>
internal class WindowsSpeechRecognizer : BaseSpeechRecognizer {
    private const string DLL_NAME = "SpeechRecognizer";

    #region импортированные из библиотеки статические методы
    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern bool runRecognizerSetup( [ MarshalAs(UnmanagedType.LPStr ) ] string modelPath );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void saveLogIntoFile( [ MarshalAs(UnmanagedType.Bool ) ] bool value);

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern bool addGrammarFile( [ MarshalAs( UnmanagedType.LPStr ) ] string grammarName, [ MarshalAs( UnmanagedType.LPStr ) ] string grammarFile);

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern bool addGrammarString( [ MarshalAs( UnmanagedType.LPStr ) ] string grammarName, [ MarshalAs( UnmanagedType.LPStr ) ] string grammarString );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool addWordIntoDictionary( [ MarshalAs( UnmanagedType.LPStr ) ] string pWord, [ MarshalAs( UnmanagedType.LPStr ) ] string pPhones );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void setBaseGrammar( [ MarshalAs( UnmanagedType.LPStr ) ] string grammarName );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void setKeyword( [ MarshalAs( UnmanagedType.LPStr ) ] string keyword );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void setThreshold( double pThreshold );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void startListeningMic( );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void stopListeningMic( );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void readMicBuffer( );

    [DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl) ]
    private static extern void changeGrammar( [ MarshalAs( UnmanagedType.LPStr ) ] string grammarName );

    [ DllImport( DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl ) ]
    private static extern void setSearchKeyword( );
        
        
    #endregion
    #region колбэки из библиотеки
    [ UnmanagedFunctionPointer( CallingConvention.Cdecl ) ]
    private delegate void FPtr( string str );
    /// <summary>
    /// метод-приёмник результатов распознавания речи из нативной Win библиотеки SpeechRecognizer.dll
    /// </summary>
    /// <param name="t">указатель на метод</param>
    [ DllImport( DLL_NAME ) ]
    private unsafe static extern void setResultReciever( FPtr t );
    /// <summary>
    /// метод-приёмник сообщений отладки для лога из нативной Win библиотеки SpeechRecognizer.dll
    /// </summary>
    /// <param name="t">указатель на метод</param>
    [ DllImport( DLL_NAME ) ]
    private unsafe static extern void setLogMessReciever( FPtr t );
    /// <summary>
    /// метод-приёмник результатов инициализации SpeechRecognizer из нативной Win библиотеки SpeechRecognizer.dll
    /// </summary>
    /// <param name="t">указатель на метод</param>
    [ DllImport( DLL_NAME ) ]
    private unsafe static extern void setCrashReciever( FPtr t );
    #endregion

    public override void initialization( string language, GrammarFileStruct[ ] grammars, string keyword ) {
        setLogMessReciever( this.onRecieveLogMess );
        setResultReciever( this.onRecognitionResult );
        setCrashReciever( this.onError );
        saveLogIntoFile( false );

        this.onRecieveLogMess( "start initialization" );
        bool result = false;
        #region инициализируем SpeechRecognizer
        string destination = Application.streamingAssetsPath + "/acousticModels/" + language + "/";
        result = runRecognizerSetup( destination );
        if ( !result ) {
            this.onError( ERROR_ON_INIT + " " + destination );
            //this.initResult.Invoke( false );
            this.onInitResult( FALSE );
            return;
        }
        #endregion

        #region добавляем слова в словарь
        var phonesDict = getWordsPhones( language, ref grammars, ref keyword );
        if ( phonesDict == null ) {
            this.onError( "error on init dictionary" );
            //this.initResult.Invoke( false );
            this.onInitResult( FALSE );
            return;
        }
        foreach ( string word in phonesDict.Keys ) {
            this.onRecieveLogMess( "add word:" + word + " phones:" + phonesDict[ word ] );
            result = addWordIntoDictionary( word, phonesDict[ word ] );
            if ( !result ) {
                this.onError( ERROR_ON_ADD_WORD + ":" + "[" + word + "] " + "phones:[" + phonesDict[ word ] + "]" );
                //this.initResult.Invoke( false );
                this.onInitResult( FALSE );
                return;
            }
        }
        #endregion

        #region добавляем граматику
        string[ ] grammar = new string[ 2 ];
        foreach ( GrammarFileStruct gramm in grammars ) {
            grammar[ 0 ] = gramm.name;
            grammar[ 1 ] = gramm.toString( );
            this.onRecieveLogMess( "try add grammar" + grammar[ 1 ] );
            result = addGrammarString( grammar[ 0 ], grammar[ 1 ] );
            if ( !result ) {
                this.onError( ERROR_ON_ADD_GRAMMAR + " " + gramm.name );
                //this.initResult.Invoke( false );
                this.onInitResult( FALSE );
                return;
            }
        }
        #endregion
        #region добавляем ключевое слово(ok google) для поиска
        if ( keyword != string.Empty ) {
            this.onRecieveLogMess( "try add keyword:" + keyword );
            setKeyword( keyword );
        }
        #endregion
        //this.initResult.Invoke( true );
        this.onInitResult( TRUE );
    }

    public override void startListening( ) {
        startListeningMic( );
        StartCoroutine( coUpdateWithDelay( _interval ) );
    }

    public override void stopListening( ) {
        StopCoroutine( coUpdateWithDelay( _interval ) );
        stopListeningMic( );
    }

    public override void switchGrammar( string grammarName ) {
        changeGrammar( grammarName );
    }

    public override void searchKeyword( ) {
        setSearchKeyword( );
    }
    /// <summary>
    /// таймер-уведомление о том что пора читать буффер микрофона для распознавания
    /// </summary>
    /// <param name="delayTime">интервал в милисекундах</param>
    /// <returns></returns>
    private IEnumerator coUpdateWithDelay( float delayTime ) {
        float interval = _interval / 1000;
        while ( true ) {
            readMicBuffer( );
            yield return new WaitForSeconds( interval );
        }
    }
    /// <summary>
    /// порог срабатывания ключевого слова
    /// </summary>
        
    void Awake( ) {
        BaseSpeechRecognizer._instance = this;
    }

    protected override void setKeywordThreshold( double pValue = 10000000000 ) {
        setThreshold( pValue );
    }
}
