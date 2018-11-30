using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core
{
	public class FontIconRegistry
	{
		public void SetDefaultIconFontFamily(string fontFamily);
		public void Clear();
		public void SetGlyphName(string fontFamily, string glyph, string name);
		public void SetGlyphName(string glyph, string name);
		public string GetGlyph(string name);
		public string GetGlyph(string fontFamily, string name);
	}
}
