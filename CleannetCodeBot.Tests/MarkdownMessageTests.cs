using System.Text;
using CleannetCodeBot.Features.Welcome;
using CleannetCodeBot.Infrastructure;

namespace CleannetCodeBot.Tests;

public class MarkdownMessageTests
{
    [Fact]
    public void NewMarkdown_ShouldReturnText()
    {
        var message = new MarkdownMessage("Test text");
        Assert.NotNull(message?.Text);
        Assert.NotEmpty(message.Text);
    }

    [Fact]
    public void NewMarkdown_InsertSpecialCharacter_ShouldReturnText()
    {
        var specialCharacters = MarkdownMessage.SpecialCharacters;

        foreach (var specialCharacter in specialCharacters)
        {
            var text = "test";
            var randomIndex = Random.Shared.Next(0, text.Length);
            text = new StringBuilder(text[0..randomIndex])
                .Append(specialCharacter)
                .Append(text[randomIndex..text.Length])
                .ToString();

            var message = new MarkdownMessage(text);
            Assert.Contains("\\", message.Text);
        }
    }
}