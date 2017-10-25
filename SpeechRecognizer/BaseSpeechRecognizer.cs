using UnityEngine;
using System.Collections.Generic;
using AvailableLanguages;
using System;
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

    protected abstract void setKeywordThreshold( double pValue = 1e+10f );
    /// <summary>
    /// порог срабатывания при распознавании ключевого слова
    /// </summary>
    public double keywordThreshold {
        set { setKeywordThreshold( value ); }
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
    /// статическая ссылка на самого себя чтобы сборщик мусора не уничтожал его
    /// </summary>
    protected static BaseSpeechRecognizer _instance = null;
    /// <summary>
    /// сигнал с сообщением отладки из библиотеки распознования
    /// </summary>
    public Action<string> logFromRecognizer;
    /// <summary>
    /// сигнал с сообщением об ошибке
    /// </summary>
    public Action<string> errorMessage;
    /// <summary>
    /// сигнал с результатом распознавания
    /// </summary>
    public Action<string> recognitionResult;
    /// <summary>
    /// сигнал с результатом инициализации объекта 
    /// распознавания голоса
    /// </summary>
    public Action<bool> initResult;
    /// <summary>
    /// начало записи голоса с микрофона
    /// </summary>
    public abstract void startListening( );
    /// <summary>
    /// окончание записи голоса с микрофона
    /// </summary>
    public abstract void stopListening( );
    /// <summary>
    /// смена граматики(перечень слов доступных для распознавания)
    /// </summary>
    /// <param name="grammarName">имя грамматики</param>
    public abstract void switchGrammar( string grammarName );
    /// <summary>
    /// Инициализируем поиск ключевого слова
    /// </summary>
    public abstract void searchKeyword( );
    /// <summary>
    /// инициализируем распознаватель голоса
    /// </summary>
    /// <param name="language">язык - определяет дирректорию с акустической моделью, словарями и файлами граматики</param>
    /// <param name="grammars">массив со структурами грамматики(имя грамматики и массив слов)</param>
    /// <param name="keyword">ключевое слово</param>
    /// <returns>результат инициализации</returns>
    public abstract void initialization( string language, GrammarFileStruct[] grammars, string keyword );

    #region baseGrammar
    /// <summary>
    /// имя файла грамматики поумолчанию
    /// </summary>
    protected string _baseGrammar = string.Empty;
    /// <summary>
    /// переопределяет файл грамматики поумолчанию
    /// </summary>
    /// <param name="grammarName">имя файла грамматики</param>
    public void setBaseGrammarName( string grammarName )
    {
        _baseGrammar = grammarName;
    }
    /// <summary>
    /// устанавливает файл грамматики поумолчанию равным первому элементу из массива структур грамматики
    /// </summary>
    /// <param name="grammars">массив со структурами грамматики(имя грамматики и массив слов)</param>
    protected void getBaseGrammar( GrammarFileStruct[ ] grammars ) {
        if ( _baseGrammar == string.Empty ) {
            _baseGrammar = grammars[ 0 ].name;
        }
    }
    #endregion
    protected virtual void onInitResult( string value ) {
        _init = Boolean.Parse( value );
        BaseSpeechRecognizer._instance.initResult?.Invoke( _init );
    }
    /// <summary>
    /// метод-приёмник сообщений отладки из библиотек распознавания голоса
    /// </summary>
    /// <param name="message"></param>
    protected void onRecieveLogMess( string message ) {
        BaseSpeechRecognizer._instance.logFromRecognizer?.Invoke( message );
    }
    /// <summary>
    /// метод-приёмник результатов распознавания из библиотек распознавания голоса
    /// </summary>
    /// <param name="message"></param>
    protected void onRecognitionResult( string message ) {
        BaseSpeechRecognizer._instance.recognitionResult?.Invoke( message );
    }
    /// <summary>
    /// метод-приёмник ошибок в работе библиотек распознавания голоса
    /// </summary>
    /// <param name="message"></param>
    protected void onError( string message ) {
        BaseSpeechRecognizer._instance.errorMessage?.Invoke( message );
    }
    /// <summary>
    /// получаем актуальный словарь
    /// </summary>
    /// <param name="language">язык словаря</param>
    /// <param name="grammars">список слов для внесения в словарь</param>
    /// <param name="keyword">ключевое слово</param>
    /// <returns>актуальный словарь (слово, транскрипция)</returns>
    protected Dictionary< string, string > getWordsPhones( string language, GrammarFileStruct[ ] grammars, string keyword ) {
        if ( language != string.Empty ) {
            string dictName = string.Empty;
            onRecieveLogMess( "lang:" + language);
            switch ( language ) {
                case Language.en_US: dictName = "EngDictionary"; break;
                case Language.es_ES: dictName = "EspDictionary"; break;
                case Language.fr_FR: dictName = "FrDictionary"; break;
                case Language.de_DE: dictName = "GerDictionary"; break;
                case Language.it_IT: dictName = "ItDictionary"; break;
                case Language.ru_RU: dictName = "RuDictionary"; break;
            }
            if ( dictName != string.Empty ) {
                Dictionary< string, string > baseDict = readDictionaryFromResources( dictName );
                if ( baseDict == null ) Debug.Log( "getWordsPhones empty dict" ); else Debug.Log( "getWordsPhones dict" );
                return getActualDictionary( ref baseDict, grammars, keyword );
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
    /// <param name="dictName">имя словаря</param>
    /// <returns>словарь (слово, транскрипция)</returns>
    private Dictionary< string, string > readDictionaryFromResources( string dictName ) {
        ResourceManager rm = new ResourceManager( "MultiplatformSpeechRecognizer.Dictionaries", Assembly.GetExecutingAssembly( ) );
        if ( rm != null ) {
            string dataText = rm.GetString( dictName );
            Dictionary<string, string> transriptionContainer = dataText.TrimEnd( '\n' ).Split( '\n' ).ToDictionary( item => item.Split( ' ' )[ 0 ], item => item.Remove( 0, item.IndexOf( " " ) + 1 ) );
            return transriptionContainer;
        }
        else
            return null;
    }
    /// <summary>
    /// формируем актуальный словарь
    /// </summary>
    /// <param name="dict">полный(базовый) словарь </param>
    /// <param name="grammars">слова для внесения в актуальный словарь</param>
    /// <param name="keyword">ключевое слово</param>
    /// <returns>актуальный словарь со словами из GrammarFileStruct</returns>
    private Dictionary<string, string> getActualDictionary( ref Dictionary< string, string > dict, GrammarFileStruct[ ] grammars, string keyword ) {
        Dictionary<string, string> actualDict = new Dictionary<string, string>();
        foreach ( GrammarFileStruct grammar in grammars ) {
            foreach ( string word in grammar.words ) {
                if ( dict.ContainsKey( word ) ) {
                    if ( !actualDict.ContainsKey( word ) ) {
                        actualDict.Add( word, dict[ word ] );
                    }
                    else
                        this.errorMessage( "dictionary already contains word [" + word + "]" );
                }
                else
                    this.errorMessage( "word [" + word + "] not found" );
            }
        }
            
        if ( keyword != string.Empty ) {
            if ( dict.ContainsKey( keyword ) && !actualDict.ContainsKey( keyword ) )
                actualDict.Add( keyword, dict[ keyword ] );
            else
                this.errorMessage( "keyword [" + keyword + "] not found" );
        } 
        return actualDict;
    }
}
