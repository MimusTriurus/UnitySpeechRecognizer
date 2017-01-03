using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaWrapperMethodNamesNamespace
{
    public static class JavaWrapperMethodNames
    {
        // устанавливает имя объекта принимающего колбэки из библиотеки
        public const string SET_RECIEVER_OBJECT_NAME = "setRecieverObjectName";
        // устанавливает имя метода-приёмника лога из библиотеки
        public const string SET_LOG_RECIEVER_METHOD_NAME = "setLogReceiverMethodName";
        // устанавливает имя метода-приёмника результатов распознавания
        public const string SET_RECOGNITION_RESULT_RECIEVER_METHOD = "setRecognitionResultRecieverMethod";
        // устанавливает имя метода-приёмника промежуточных результатов
        public const string SET_RECOGNITION_PARTIAL_RESULT_RECEIVER_METHOD = "setRecognitionPartialResultRecieverMethod";
        // устанавливает имя метода-приёмника рузультатов инициализации распознавателя
        public const string SET_INITIALIZATION_COMPLETE_METHOD = "setInitializationCompleteMethod";
        // метод начала инициализации распознаваетля
        public const string RUN_RECOGNIZER_SETUP = "runRecognizerSetup";
        // устанавливает временной интервал ожидания получения звука с микрофона
        public const string SET_TIMEOUT_INTERVAL = "setTimeoutInterval";
        // устанавливает актуальный файл грамматики (устарел?)
        public const string SET_BASE_GRAMMAR_FILE = "setBaseGrammarFile";
        // переключает файл грамматики
        public const string SWITCH_SEARCH = "switchSearch";
        // добавляет перед инициализацией доступные для распознавания файлы грамматики
        public const string ADD_GRAMMAR_FILE = "addGrammarFile";
        // инициирует начало распознавания с базовым файлом грамматики
        public const string START_LISTENING = "startListening";
        // прекращает распознавание
        public const string STOP_LISTENING = "stopListening";
    }
}
