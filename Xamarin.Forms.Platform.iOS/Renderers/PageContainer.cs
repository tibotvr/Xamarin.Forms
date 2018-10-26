using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class PageContainer : UIView, IUIAccessibilityContainer
	{
		readonly AccessibleUIViewController _parent;
		List<NSObject> _accessibilityElements = new List<NSObject>();

		public PageContainer(AccessibleUIViewController parent)
		{
			_parent = parent;
		}

		public PageContainer()
		{
			IsAccessibilityElement = false;
		}

		public List<NSObject> AccessibilityElements
		{
			get
			{
				if (_accessibilityElements == null)
					_accessibilityElements = _parent.GetAccessibilityElements()
						?? NSArray.ArrayFromHandle<NSObject>(AccessibilityContainer.GetAccessibilityElements().Handle).ToList();

				return _accessibilityElements;
			}
		}

		IUIAccessibilityContainer AccessibilityContainer => this;

		public void ClearAccessibilityElements()
		{
			_accessibilityElements = null;
		}

		[Export("accessibilityElementCount")]
		nint AccessibilityElementCount()
		{
			return AccessibilityElements.Count;
		}

		[Export("accessibilityElementAtIndex:")]
		NSObject GetAccessibilityElementAt(nint index)
		{
			return AccessibilityElements[(int)index];
		}

		[Export("indexOfAccessibilityElement:")]
		int GetIndexOfAccessibilityElement(NSObject element)
		{
			return AccessibilityElements.IndexOf(element);
		}
	}
}