using NUnit.Framework;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TabIndexTests : BaseTestFixture
	{
		[Test]
		public void FindNextElement_Forward_Without_Tabindexes()
		{
			var stackLayout = new StackLayout();

			var labels = new Label[5];
			for (int i = 0; i < labels.Length; i++)
			{
				labels[i] = new Label();
				stackLayout.Children.Add(labels[i]);
			}

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int __);

			for (int i = 0; i < labels.Length; i++)
			{
				int _ = labels[i].TabIndex;
				var found = labels[i].FindNextElement(true, tabIndexes, ref _);
				if (labels.Length != i + 1)
					Assert.AreEqual(labels[i + 1], found);
				else
					Assert.AreEqual(stackLayout, found);
			}
		}

		[Test]
		public void GetTabIndexesOnParentPage_ImplicitZero()
		{
			var target = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					new Label { TabIndex = 0 },
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = target };

			var tabIndexes = target.GetTabIndexesOnParentPage(out int _);

			//StackLayout is technically the first element with TabIndex 0.
			Assert.AreEqual(target, tabIndexes[0][0]);
		}

		[Test]
		public void GetTabIndexesOnParentPage_ExplicitZero()
		{
			Label target = new Label { TabIndex = 0 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int _);

			Assert.AreEqual(target, tabIndexes[0][1]);
		}

		[Test]
		public void GetTabIndexesOnParentPage_NegativeTabIndex()
		{
			Label target = new Label { TabIndex = -1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int _);

			Assert.AreEqual(target, tabIndexes[-1][0]);
		}

		[Test]
		public void FindNextElement_Forward_NextTabIndex()
		{
			Label target = new Label { TabIndex = 1 };
			Label nextElement = new Label { TabIndex = 2 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					new Label { TabIndex = 3 },
					nextElement,
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(true, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Forward_DeclarationOrder()
		{
			Label target = new Label { TabIndex = 1 };
			Label nextElement = new Label { TabIndex = 2 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(true, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Forward_TabIndex()
		{
			Label target = new Label { TabIndex = 1 };
			Label nextElement = new Label { TabIndex = 2 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 1 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int tabIndex = target.TabIndex;

			var found = target.FindNextElement(true, tabIndexes, ref tabIndex);

			Assert.AreEqual(2, tabIndex);
		}

		[Test]
		public void FindNextElement_Backward_NextTabIndex()
		{
			Label target = new Label { TabIndex = 2 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					new Label { TabIndex = 3 },
					nextElement,
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(false, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Backward_DeclarationOrder()
		{
			Label target = new Label { TabIndex = 2 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					nextElement,
					new Label { TabIndex = 1 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int _ = target.TabIndex;

			var found = target.FindNextElement(false, tabIndexes, ref _);

			Assert.AreEqual(nextElement, found);
		}

		[Test]
		public void FindNextElement_Backward_TabIndex()
		{
			Label target = new Label { TabIndex = 2 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			int tabIndex = target.TabIndex;

			var found = target.FindNextElement(false, tabIndexes, ref tabIndex);

			Assert.AreEqual(1, tabIndex);
		}

		[Test]
		public void GetFirstTabStop()
		{
			Label target = new Label { TabIndex = 0 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			var first = TabIndexExtensions.GetFirstNonLayoutTabStop(tabIndexes);

			Assert.AreEqual(target, first);
		}

		[Test]
		public void GetFirstTabStop_Negative()
		{
			Label target = new Label { TabIndex = -1 };
			Label nextElement = new Label { TabIndex = 1 };
			var stackLayout = new StackLayout
			{
				Children = {
					new Label { TabIndex = 3 },
					target,
					nextElement,
					new Label { TabIndex = 2 },
				}
			};

			var page = new ContentPage { Content = stackLayout };

			var tabIndexes = stackLayout.GetTabIndexesOnParentPage(out int maxAttempts);

			var first = TabIndexExtensions.GetFirstNonLayoutTabStop(tabIndexes);

			Assert.AreEqual(target, first);
		}
	}
}
