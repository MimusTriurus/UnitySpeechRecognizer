﻿using UnityEngine;
using GrammarNamespace;

namespace MultiplatformSpeechRecognizer.SpeechRecognizer
{
    public abstract class BaseSpeechRecognizer : MonoBehaviour
    {
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

        public delegate void ReturnStringValue(string value);
        public delegate void ReturnBoolValue(bool value);
        /// <summary>
        /// сигнал с сообщением отладки из библиотеки распознования
        /// </summary>
        public ReturnStringValue logFromRecognizer;
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
        /// <summary>
        /// инициализируем распознаватель голоса
        /// </summary>
        /// <param name="language">язык - определяет дирректорию с акустической моделью, словарями и файлами граматики</param>
        /// <param name="grammars">массив со структурами грамматики(имя грамматики и массив слов)</param>
        public abstract void initialization(string language, GrammarFileStruct[] grammars);

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
    }
}
