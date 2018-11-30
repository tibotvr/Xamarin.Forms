using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using ValidateValueDelegate = Xamarin.Forms.BindableProperty.ValidateValueDelegate;

namespace Xamarin.Forms
{
	public partial class FontImageSource : ImageSource
	{
		public static string Scheme = "glyph";

		public override string ToString()
		{
			return $"Url: {Uri}";
		}

		public static implicit operator FontImageSource(Uri url)
		{
			return (FontImageSource)FromUri(url);
		}

		public static implicit operator Uri(FontImageSource font)
		{
			return font?.Uri;
		}
	}

	public partial class FontImageSource
	{
		private static BindableProperty CreateBindableProperty<T>(
			string name,
			T defaultValue = default(T),
			Func<T, bool> validateValue = null)
		{
			ValidateValueDelegate validate = null;
			if (validateValue != null)
				validate = (bindable, value) => validateValue((T)value);

			return BindableProperty.Create(
				propertyName: name,
				returnType: typeof(T),
				declaringType: typeof(FontImageSource),
				defaultValue: defaultValue,
				validateValue: validate
			);
		}

		[TypeConverter(typeof(UriTypeConverter))]
		public Uri Uri { get => (Uri)GetValue(UriProperty); set => SetValue(UriProperty, value); }
		private static bool ValidateUriProperty(Uri value)
		{
			if (value == null)
				return false;

			if (!value.IsAbsoluteUri)
				return false;

			if (value.Scheme != Scheme)
				return false;

			if (value.Segments.Length != 2)
				return false;

			// glyph:www.vendor.com/fontFamily/name => glyph
			return true;
		}
		public static readonly BindableProperty UriProperty
			= CreateBindableProperty<Uri>(nameof(Uri),
				validateValue: ValidateUriProperty
			);

		public string Name { get => (string)GetValue(NameProperty); set => SetValue(NameProperty, value); }
		public static readonly BindableProperty NameProperty = CreateBindableProperty<string>(nameof(Name));

		public double Size { get => (double)GetValue(SizeProperty); set => SetValue(SizeProperty, value); }
		public static readonly BindableProperty SizeProperty = CreateBindableProperty(nameof(Size), 30d);

		public char Glyph { get => (char)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
		public static readonly BindableProperty GlyphProperty = CreateBindableProperty<char>(nameof(Glyph));

		public Color Color { get => (Color)GetValue(ColorProperty); set => SetValue(ColorProperty, value); }
		public static readonly BindableProperty ColorProperty = CreateBindableProperty<Color>(nameof(Color));

		public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
		public static readonly BindableProperty FontFamilyProperty = CreateBindableProperty<string>(nameof(FontFamily));

		private static BindableProperty[] BindableProperties = new[]
		{
			NameProperty,
			UriProperty,
			FontFamilyProperty,
			GlyphProperty,
			ColorProperty,
			SizeProperty,
		};
		protected override void OnPropertyChanged(string propertyName = null)
		{
			for (var i = 0; i < BindableProperties.Length; i++)
			{
				var bindableProperty = BindableProperties[i];
				if (propertyName == bindableProperty.PropertyName)
					OnSourceChanged();
			}
			
			base.OnPropertyChanged(propertyName);
		}
	}
}
