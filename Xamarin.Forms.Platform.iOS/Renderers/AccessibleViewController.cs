using Foundation;
using System.Collections.Generic;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public class AccessibleUIViewController : UIViewController, IAccessibilityElementsController
	{
		bool _disposed;

		VisualElement _element;

		public PageContainer Container { get; private set; }
		IAccessibilityElementsController AccessibilityElementsController => this;

		public List<NSObject> GetAccessibilityElements()
		{
			if (Container == null || _element == null)
				return null;

			var children = _element.Descendants();
			IDictionary<int, List<VisualElement>> tabIndexes = null;
			int childrenWithTabStopsLessOne = 0;
			VisualElement firstTabStop = null;
			List<NSObject> views = new List<NSObject>();
			foreach (var child in children)
			{
				if (!(child is VisualElement ve && Platform.GetRenderer(ve).NativeView is ITabStop tabStop))
				{
					continue;
				}

				if (tabIndexes == null)
				{
					tabIndexes = ve.GetTabIndexesOnParentPage(out childrenWithTabStopsLessOne);
					firstTabStop = TabStopExtensions.GetFirstTabStopVisualElement(tabIndexes);
					break;
				}
			}

			if (firstTabStop == null)
				return null;

			VisualElement nextVisualElement = firstTabStop;
			UIView nextControl = null;
			do
			{
				nextControl = (Platform.GetRenderer(nextVisualElement).NativeView as ITabStop)?.TabStop;

				if (views.Contains(nextControl))
					break; // we've looped to the beginning

				if (nextControl != null)
					views.Add(nextControl);

				nextVisualElement = nextVisualElement.GetNextTabStopVisualElement(forwardDirection: true,
																tabIndexes: tabIndexes,
																maxAttempts: childrenWithTabStopsLessOne);
			} while (nextVisualElement != null && nextVisualElement != firstTabStop);

			return views;
		}

		void IAccessibilityElementsController.ResetAccessibilityElements()
		{
			Container.ClearAccessibilityElements();
		}

		public virtual void SetElement(VisualElement element)
		{
			_element = element;

			ResetContainer();
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			AccessibilityElementsController.ResetAccessibilityElements();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				Container?.Dispose();
				Container = null;

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		void ResetContainer()
		{
			Container?.RemoveFromSuperview();

			Container = new PageContainer(this);
			View.AddSubview(Container);
		}
	}
}