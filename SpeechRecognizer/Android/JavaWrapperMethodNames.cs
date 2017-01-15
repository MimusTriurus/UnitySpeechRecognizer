﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaWrapperMethodNamesNamespace
{
    /// <summary>
    /// имена методов импортируемых из android библиотеки
    /// </summary>
    public static class JavaWrapperMethodNames
    {
        /// <summary>
        /// устанавливает имя объекта принимающего колбэки из библиотеки 
        /// </summary>
        public const string SET_RECIEVER_OBJECT_NAME = "setRecieverObjectName";
        /// <summary>
        /// устанавливает имя метода-приёмника лога из библиотеки 
        /// </summary>
        public const string SET_LOG_RECIEVER_METHOD_NAME = "setLogReceiverMethodName";
        /// <summary>
        /// устанавливает имя метода-приёмника результатов распознавания 
        /// </summary>
        public const string SET_RECOGNITION_RESULT_RECIEVER_METHOD = "setRecognitionResultRecieverMethod";
        /// <summary>
        /// устанавливает имя метода-приёмника промежуточных результатов 
        /// </summary>
        public const string SET_RECOGNITION_PARTIAL_RESULT_RECEIVER_METHOD = "setRecognitionPartialResultRecieverMethod";
        /// <summary>
        /// устанавливает имя метода-приёмника рузультатов инициализации распознавателя 
        /// </summary>
        public const string SET_INITIALIZATION_COMPLETE_METHOD = "setInitializationCompleteMethod";
        /// <summary>
        /// метод начала инициализации распознаваетля 
        /// </summary>
        public const string RUN_RECOGNIZER_SETUP = "runRecognizerSetup";
        /// <summary>
        /// устанавливает временной интервал ожидания получения звука с микрофона 
        /// </summary>
        public const string SET_TIMEOUT_INTERVAL = "setTimeoutInterval";
        /// <summary>
        /// устанавливает актуальный файл грамматики (устарел?) 
        /// </summary>
        public const string SET_BASE_GRAMMAR_FILE = "setBaseGrammarFile";
        /// <summary>
        /// переключает файл грамматики 
        /// </summary>
        public const string SWITCH_SEARCH = "switchSearch";
        /// <summary>
        /// добавляет перед инициализацией доступные для распознавания файлы грамматики 
        /// </summary>
        public const string ADD_GRAMMAR_FILE = "addGrammarFile";
        /// <summary>
        /// инициирует начало распознавания с базовым файлом грамматики 
        /// </summary>
        public const string START_LISTENING = "startListening";
        /// <summary>
        /// прекращает распознавание 
        /// </summary>
        public const string STOP_LISTENING = "stopListening";
    }
}
