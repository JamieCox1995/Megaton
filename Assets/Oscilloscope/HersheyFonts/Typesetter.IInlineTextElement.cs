public partial class Typesetter
{
    private interface IInlineTextElement : ITextElement
    {
        HersheyFont Font { get; }
        float FontSize { get; }
        float LineSpacing { get; }
    }
}