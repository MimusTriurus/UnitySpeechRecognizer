/// <summary>
/// структура описывающая файл грамматики
/// </summary>
[System.Serializable]
public class GrammarFileStruct {
    /// <summary>
    /// имя файла грамматики
    /// </summary>
    public string name;
    /// <summary>
    /// массив слов
    /// </summary>
    public string[ ] words;
    /// <summary>
    /// преобразует структуру в формализованную строку
    /// </summary>
    /// <returns>формализованная строка</returns>
    public string toString( ) {
        string value;
        value = "#JSGF V1.0;";
        value += "grammar commands;";
        value += "<commands> = ";

        for ( int i = 0; i < words.Length; i++ )
        {
            words[ i ] = words[ i ];
            value += words[ i ];
            if ( i != words.Length - 1 )
                value += " | ";
        }
        value += ";";

        value += "public <command> = <commands>+;";

        return value;
    }
}


