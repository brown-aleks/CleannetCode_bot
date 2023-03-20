namespace CleannetCode_bot.Infrastructure;

public class MarkdownMessage
{
    private readonly string _text;
    public const string SpecialCharacters = @"_*[]()~`>#+-={}.!";

    public MarkdownMessage(string text)
    {
        _text = EscapeSpecialCharacters(text);
    }

    public string Text => _text;

    private string EscapeSpecialCharacters(string input)
    {
        // https://core.telegram.org/bots/api#markdownv2-style
        var specialCharacters = new[]
        {
            "_", "*", "[", "]", "(", ")", "~", "`", ">",
            "#", "+", "-", "=", "|", "{", "}", ".", "!"
        };
        
        foreach (var specialCharacter in specialCharacters)
        {
            input = input.Replace(specialCharacter, "\\" + specialCharacter);
        }
        
        return input;
    }
}