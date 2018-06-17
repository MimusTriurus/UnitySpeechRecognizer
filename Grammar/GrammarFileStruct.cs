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
    /// преобразует структуру в формализованную строку грамматики
    /// </summary>
    /// <returns>формализованная строка</returns>
    public string toString( ) {
        string value;
        value = "#JSGF V1.0;";
        value += "grammar commands;";
        value += "<commands> = ";

        for ( int i = 0; i < words.Length; i++ ) {
            //words[ i ] = words[ i ];
            value += words[ i ];
            if ( i != words.Length - 1 )
                value += " | ";
        }
        value += ";";
        value += "public <command> = <commands>+;";

        return value;
    }
    /// <summary>
    /// Костыль. Необходим для формирования грамматики для 
    /// словарей с различными регистром 
    /// (приведение всех словарей к нижнему регистру не работает)
    /// Меняем слово грамматики на слово из словаря с учетом его регистра
    /// </summary>
    /// <param name="from">слово грамматики</param>
    /// <param name="to">слово словаря</param>
    /// <returns></returns>
    public bool replace( string from, string to ) {
        for ( int i = 0; i < words.Length; i++ ) {
            if ( words[ i ] == from ) {
                words[ i ] = to;
                return true;
            }
        }
        return false;
    }
}


