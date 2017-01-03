using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace DictionaryNamespace
{
    public class DictionaryFileCreator
    {
        private string _dir;
        public string dir { get { return _dir; } }

        private const string DICT_FILES_FOLDER_NAME = "dictionaries";
        const string BASE_DICTIONARY_NAME = "baseDictionary";
        const string ACTUAL_DICTIONARY_NAME = "actualDictionary";
        const string FILE_EXTENSION = ".dict";

        private bool _isOK = true;
        public bool isOK { get { return _isOK; } }

        private Dictionary<string, string> _transriptionContainer = new Dictionary<string, string>();

        public DictionaryFileCreator(string sourcePath, string targetPath, string language)
        {
            readBaseDictionary(sourcePath, language);
            prepareFilePathForGrammarFiles(targetPath, language);
        }

        private void readBaseDictionary(string baseDictinaryDestination, string language)
        {
            baseDictinaryDestination += "/" + language + "/dictionaries/" + BASE_DICTIONARY_NAME + FILE_EXTENSION;
            WWW reader = new WWW(baseDictinaryDestination);
            while (!reader.isDone)
            {

            }
            string dataText = reader.text;

            _transriptionContainer = dataText.TrimEnd('\n').Split('\n').ToDictionary(item => item.Split(' ')[0], item => item.ToString());
                        
        }

        private void prepareFilePathForGrammarFiles(string dir, string language)
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

        public void initDictionary(List<string> words)
        {
            if (words.Count == 0)
            {
                _isOK = false;
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
                    _isOK = false;
                }
            }

            file.Close();
        }
    }
}
