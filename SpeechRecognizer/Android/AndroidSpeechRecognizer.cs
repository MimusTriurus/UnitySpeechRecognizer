using System.Collections.Generic;
using UnityEngine;
using JavaWrapperMethodNamesNamespace;
using System;

internal class AndroidSpeechRecognizer : BaseSpeechRecognizer {
    /// <summary>
    /// объект SpeechRecognizer импортированный из нативной android библиотеки
    /// </summary>
    private AndroidJavaObject _recognizerActivity = null;

    private Dictionary< string, string > _phonesDict = null;
    private GrammarFileStruct[ ] _grammars = null;
    private string _keyword = string.Empty;

    public override void initialization( string language, GrammarFileStruct[] grammars, string keyword ) {
        this.logFromRecognizer.Invoke( "start initialization" );
        this.getBaseGrammar( grammars );

        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        if ( unity == null ) {
            this.logFromRecognizer.Invoke("empty java class");
            this.initResult?.Invoke( false );
            return;
        }

        _recognizerActivity = unity.GetStatic< AndroidJavaObject >("currentActivity");
        if ( _recognizerActivity == null ) {
            this.logFromRecognizer.Invoke( "empty java object" );
            this.initResult?.Invoke( false );
            return;
        }

        #region инициализируем колбэк из jar библиотеки
        _recognizerActivity.CallStatic( JavaWrapperMethodNames.SET_RECIEVER_OBJECT_NAME, this.gameObject.name );
        _recognizerActivity.CallStatic( JavaWrapperMethodNames.SET_LOG_RECIEVER_METHOD_NAME, "onRecieveLogMess");
        _recognizerActivity.CallStatic( JavaWrapperMethodNames.SET_RECOGNITION_RESULT_RECIEVER_METHOD, "onRecognitionResult" );
        _recognizerActivity.CallStatic(JavaWrapperMethodNames.SET_CRASH_MESS_RECIEVER_METHOD, "onError");
        _recognizerActivity.CallStatic( JavaWrapperMethodNames.SET_INITIALIZATION_COMPLETE_METHOD, "onInitResult");
        #endregion
        _recognizerActivity.Call( JavaWrapperMethodNames.SET_BASE_GRAMMAR_FILE, _baseGrammar );
        this.logFromRecognizer.Invoke( JavaWrapperMethodNames.RUN_RECOGNIZER_SETUP );
        _recognizerActivity.Call( JavaWrapperMethodNames.RUN_RECOGNIZER_SETUP, language );

        _grammars = grammars;
        _phonesDict = getWordsPhones( language, grammars, keyword );
        _keyword = keyword;
    }

    protected override void onInitResult( string value )
    {
        onRecieveLogMess( "onInitResult:" + value );
        foreach ( string word in _phonesDict.Keys )
        {
            //this.logFromRecognizer( "add word:" + word + " phones:" + _phonesDict[ word ] );
            _recognizerActivity.Call<bool>( JavaWrapperMethodNames.ADD_WORD_INTO_DICTIONARY, word, _phonesDict[ word ] );
        }

        #region добавляем граматику
        string[ ] grammar = new string[ 2 ];
        foreach ( GrammarFileStruct gramm in _grammars )
        {
            grammar[0] = gramm.name;
            grammar[1] = gramm.toString();
            //onRecieveLogMess( "GRAMM" + gramm.toString( ) );
            _recognizerActivity.Call<bool>( JavaWrapperMethodNames.ADD_GRAMMAR_STRING, grammar[ 0 ], grammar[ 1 ] );
        }
        #endregion

        #region добавляем ключевое слово(ok google) для поиска
        if ( _keyword != string.Empty )
        {
            _recognizerActivity.Call<bool>(JavaWrapperMethodNames.SET_KEYWORD, _keyword );
            this.logFromRecognizer( "add keyword:" + _keyword );
        }
        #endregion

        _init = Boolean.Parse( value );
        BaseSpeechRecognizer._instance.initResult?.Invoke( _init );
    }

    public override void startListening( ) {
        this.logFromRecognizer( "startListening:" + _init.ToString( ) );
        if ( _init ) {
            _recognizerActivity.Call( JavaWrapperMethodNames.START_LISTENING );
        }
    }

    public override void stopListening( ) {
        if ( _init ) {
            _recognizerActivity.Call( JavaWrapperMethodNames.STOP_LISTENING );
        }
    }

    public override void switchGrammar( string grammarName ) {
        _recognizerActivity.Call( JavaWrapperMethodNames.SWITCH_SEARCH, grammarName );
    }

    public override void searchKeyword( ) {
        _recognizerActivity.Call< bool >( JavaWrapperMethodNames.SET_SEARCH_KEYWORD );
    }

    protected override void setKeywordThreshold( double pValue = 10000000000 ) {
            
    }

    void Awake( ) {
        BaseSpeechRecognizer._instance = this;
    }
}
