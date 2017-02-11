using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace DictionaryNamespace
{
    /// <summary>
    /// инициализирует актуальный словарь
    /// </summary>
    internal class DictionaryFileCreator
    {
        /// <summary>
        /// директория в которой хранятся словари
        /// </summary>
        private string _dir;
        public string dir { get { return _dir; } }
        /// <summary>
        /// имя директория где хранятся словари
        /// </summary>
        private const string DICT_FILES_FOLDER_NAME = "dictionaries";
        /// <summary>
        /// имя базового словаря
        /// </summary>
        private const string BASE_DICTIONARY_NAME = "baseDictionary";
        /// <summary>
        /// имя актуального словаря
        /// </summary>
        private const string ACTUAL_DICTIONARY_NAME = "actualDictionary";
        /// <summary>
        /// расширение файла словаря
        /// </summary>
        private const string FILE_EXTENSION = ".dict";
        /// <summary>
        /// инициализация словаря прошла успешно
        /// </summary>
        private bool _isOK = true;
        public bool isOK { get { return _isOK; } }
        /// <summary>
        /// контейнер хранит ключ(слово) и значение(транскрипция)
        /// </summary>
        private Dictionary<string, string> _transriptionContainer = new Dictionary<string, string>();
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="sourcePath">директория с базовым словарём</param>
        /// <param name="targetPath">директория где будет создан актуальный словарь</param>
        /// <param name="language">язык - определяет базовую директорию</param>
        public DictionaryFileCreator(string sourcePath, string targetPath, string language)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android: readBaseDictionaryFromAndroidAssets(sourcePath, language); break;
                case RuntimePlatform.WindowsEditor: readBaseDictionaryOnDesktop(sourcePath, language); break;
                case RuntimePlatform.WindowsPlayer: readBaseDictionaryOnDesktop(sourcePath, language); break;
            }
            prepareFilePathDictionary(targetPath, language);
        }
        /// <summary>
        /// читаем словарь из ассетов андроид приложения 
        /// </summary>
        /// <param name="baseDictinaryDestination">директория базового словаря</param>
        /// <param name="language">язык - определяет базовую дирректорию</param>
        private void readBaseDictionaryFromAndroidAssets(string baseDictinaryDestination, string language)
        {
            baseDictinaryDestination += "/" + language + "/dictionaries/" + BASE_DICTIONARY_NAME + FILE_EXTENSION;
            WWW reader = new WWW(baseDictinaryDestination);
            while (!reader.isDone)
            {
                Debug.Log("wait");
            }
            // все слова в нижний регистр
            string dataText = reader.text.ToLower();
            _transriptionContainer = dataText.TrimEnd('\n').Split('\n').ToDictionary(item => item.Split(' ')[0], item => item.ToString());
                        
        }
        /// <summary>
        /// читаем словарь из папки StreamingAssets приложения 
        /// </summary>
        /// <param name="baseDictinaryDestination">директория базового словаря</param>
        /// <param name="language">язык - определяет базовую дирректорию</param>
        public void readBaseDictionaryOnDesktop(string baseDictinaryDestination, string language)
        {
            baseDictinaryDestination += "/" + language + "/dictionaries/" + BASE_DICTIONARY_NAME + FILE_EXTENSION;
            if (File.Exists(baseDictinaryDestination))
            {
                string dataText = File.ReadAllText(baseDictinaryDestination);
                // все слова в нижний регистр
                dataText = dataText.ToLower();
                _transriptionContainer = dataText.TrimEnd('\n').Split('\n').ToDictionary(item => item.Split(' ')[0], item => item.ToString());
            }
            else
            {
                Debug.Log("file not exist:" + baseDictinaryDestination);
            }
        }
        /// <summary>
        /// создаёт директорию для словаря
        /// </summary>
        /// <param name="dir">базовая директория</param>
        /// <param name="language">язык - определяет целевую директорию</param>
        private void prepareFilePathDictionary(string dir, string language)
        {
            _dir = dir + "/" + language + "/" + DICT_FILES_FOLDER_NAME;

            if (!Directory.Exists(dir + "/" + language))
            {
                Directory.CreateDirectory(dir + "/" + language);
            }
            if (!Directory.Exists(_dir))
            {
                Directory.CreateDirectory(_dir);
            }
        }
        /// <summary>
        /// создаёт актуальный словарь и добавляет в него слова
        /// </summary>
        /// <param name="words">лист со словами и их транскрипциями</param>
        public void initDictionary(List<string> words)
        {
            if (words.Count == 0)
            {
                _isOK = false;
                Debug.Log("word count = 0");
                return;
            }

            string destination = _dir + "/" + ACTUAL_DICTIONARY_NAME + FILE_EXTENSION;
            System.IO.StreamWriter file = new System.IO.StreamWriter(destination);

            foreach (string word in words)
            {
                if (_transriptionContainer.ContainsKey(word))
                {
                    file.WriteLine(_transriptionContainer[word]);
                }
                else
                {
                    Debug.Log("not contains:" + word);
                    _isOK = false;
                }
            }

            file.Close();
        }
    }
}
