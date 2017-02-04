using System.IO;

namespace GrammarNamespace
{
    /// <summary>
    /// инициализирует файлы грамматики
    /// </summary>
    public class GrammarFilesCreator
    {
        /// <summary>
        /// директория в которой хранятся файлы грамматики
        /// </summary>
        private string _dir;
        public string dir { get { return _dir; } }
        /// <summary>
        /// имя директории в которой хранятся файлы грамматики
        /// </summary>
        private const string GRAMMAR_FILES_FOLDER_NAME = "grammarFiles";
        /// <summary>
        /// расширение файлов грамматики
        /// </summary>
        const string FILE_EXTENSION = ".gram";
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="dir">базовая директория</param>
        /// <param name="language">язык - определяет целевую директорию</param>
        public GrammarFilesCreator(string dir, string language)
        {
            prepareFilePathForGrammarFiles(dir, language);
        }
        /// <summary>
        /// создаёт директорию для файлов грамматики
        /// </summary>
        /// <param name="dir">базовая директория</param>
        /// <param name="language">язык - определяет целевую директорию</param>
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
        /// <summary>
        /// создаём файл грамматики по шаблону
        /// </summary>
        /// <param name="grammarName">имя файла грамматики</param>
        /// <param name="words">массив слов</param>
        private void initNewGrammarFile(string grammarName, string[] words)
        {
            string destination = _dir + "/" + grammarName + FILE_EXTENSION;
            System.IO.StreamWriter file = new System.IO.StreamWriter(destination);
            file.WriteLine("#JSGF V1.0;");
            file.WriteLine("grammar commands;\n");
            file.Write("<commands> = ");

            for (int i = 0; i < words.Length; i++)
            {
                words[i] = words[i];
                file.Write(words[i]);
                if (i != words.Length - 1)
                    file.Write(" | ");
            }
            file.Write(";\n");

            file.WriteLine("public <command> = <commands>+;");

            file.Close();
        }
        /// <summary>
        /// создаём все файлы грамматики
        /// </summary>
        /// <param name="grammarFilesStruct">массив со структурами описывающими файлы грамматики</param>
        public void createGrammarFiles(GrammarFileStruct[] grammarFilesStruct)
        {
            foreach (GrammarFileStruct grammarFile in grammarFilesStruct)
            {
                initNewGrammarFile(grammarFile.name, grammarFile.words);
            }
        }

    }
}
