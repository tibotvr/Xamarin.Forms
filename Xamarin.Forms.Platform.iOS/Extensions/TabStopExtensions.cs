using System;
using System.Collections.Generic;
using Xamarin.Forms.Internals;

#if __MOBILE__
using NativeView = UIKit.UIView;

namespace Xamarin.Forms.Platform.iOS
#else
using NativeView = AppKit.NSView;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	internal static class TabStopExtensions
	{
		public static NativeView GetNextTabStop(this VisualElement ve, bool forwardDirection, IDictionary<int, List<VisualElement>> tabIndexes, int maxAttempts)
		{
			return FindNextTabStop(ve, forwardDirection, tabIndexes, maxAttempts).Item2;
		}

		public static VisualElement GetNextTabStopVisualElement(this VisualElement ve, bool forwardDirection, IDictionary<int, List<VisualElement>> tabIndexes, int maxAttempts)
		{
			return FindNextTabStop(ve, forwardDirection, tabIndexes, maxAttempts).Item1;
		}

		public static VisualElement GetFirstTabStopVisualElement(IDictionary<int, List<VisualElement>> tabIndexes)
		{
			if (tabIndexes == null)
				return null;

			return TabIndexExtensions.GetFirstElementByTabIndex(tabIndexes);
		}

		static Tuple<VisualElement, NativeView> FindNextTabStop(VisualElement ve, bool forwardDirection, IDictionary<int, List<VisualElement>> tabIndexes, int maxAttempts)
		{
			if (maxAttempts <= 0 || tabIndexes == null)
				return null;

			VisualElement element = ve as VisualElement;
			if (tabIndexes == null)
				return null;

			int tabIndex = ve.TabIndex;
			int attempt = 0;
			NativeView control = null;

			do
			{
				element = element.FindNextElement(forwardDirection, tabIndexes, ref tabIndex);

				var renderer = Platform.GetRenderer(element);
				control = (renderer as ITabStop)?.TabStop;

			} while (!(control?.CanBecomeFocused == true || ++attempt >= maxAttempts));

			return new Tuple<VisualElement, NativeView>(element, control);
		}
	}
}
