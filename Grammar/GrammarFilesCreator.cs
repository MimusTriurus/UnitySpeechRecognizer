using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GrammarNamespace
{
    public class GrammarFilesCreator
    {
        private string _dir;
        public string dir { get { return _dir; } }

        private const string GRAMMAR_FILES_FOLDER_NAME = "grammarFiles";
        const string FILE_EXTENSION = ".gram";

        public GrammarFilesCreator(string dir, string language)
        {
            prepareFilePathForGrammarFiles(dir, language);
        }

        private void prepareFilePathForGrammarFiles(string dir, string language)
        {
            _dir = dir + "/" + language + "/" + GRAMMAR_FILES_FOLDER_NAME;
            if (!Directory.Exists(dir + "/" + language))
            {
                Directory.CreateDirectory(dir + "/" + language);
            }
            if (!Directory.Exists(_dir))
            {
                Directory.CreateDirectory(_dir);
            }

        }

        private void initNewGrammarFile(string grammarName, string[] words)
        {
            string destination = _dir + "/" + grammarName + FILE_EXTENSION;
            System.IO.StreamWriter file = new System.IO.StreamWriter(destination);
            file.WriteLine("#JSGF V1.0;");
            file.WriteLine("grammar commands;\n");
            file.Write("<commands> = ");

            for (int i = 0; i < words.Length; i++)
            {
                file.Write(words[i]);
                if (i != words.Length - 1)
                    file.Write(" | ");
            }
            file.Write(";\n");

            file.WriteLine("public <command> = <commands>+;");

            file.Close();
        }

        public void createGrammarFiles(GrammarFileStruct[] grammarFilesStruct)
        {
            foreach (GrammarFileStruct grammarFile in grammarFilesStruct)
            {
                initNewGrammarFile(grammarFile.name, grammarFile.words);
            }
        }

    }
}
