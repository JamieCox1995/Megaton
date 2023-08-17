using UnityEngine;

public partial class Typesetter
{
    private class SpaceElement : InlineTextElement
    {
        public override float PreferredWidth { get { return this.Font.GetCharacterFromString(" ").Width * this.FontSize; } }

        public SpaceElement(HersheyFont font, float fontSize, float lineSpacing, TextAnchor anchor) : base(font, fontSize, lineSpacing, anchor) { }
		
		public override string ToString()
		{
			return " ";
		}
    }
}