﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Widget;
using Android.Support.V7.Graphics.Drawable;
using Android.Support.V7.Widget;
using Android.Views;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;
using R = Android.Resource;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public class ShellToolbarTracker : Java.Lang.Object, AView.IOnClickListener, IShellToolbarTracker, IFlyoutBehaviorObserver
	{
		#region IFlyoutBehaviorObserver

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			if (_flyoutBehavior == behavior)
				return;
			_flyoutBehavior = behavior;

			if (Page != null)
				UpdateLeftBarButtonItem();
		}

		#endregion IFlyoutBehaviorObserver

		bool _canNavigateBack;
		bool _disposed;
		DrawerLayout _drawerLayout;
		ActionBarDrawerToggle _drawerToggle;
		FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;
		Page _page;
		SearchHandler _searchHandler;
		IShellSearchView _searchView;
		ContainerView _titleViewContainer;
		IShellContext _shellContext;
		//assume the default
		Color _tintColor = Color.Default;
		Toolbar _toolbar;

		public ShellToolbarTracker(IShellContext shellContext, Toolbar toolbar, DrawerLayout drawerLayout)
		{
			_shellContext = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
			_toolbar = toolbar ?? throw new ArgumentNullException(nameof(toolbar));
			_drawerLayout = drawerLayout ?? throw new ArgumentNullException(nameof(drawerLayout));

			((IShellController)_shellContext.Shell).AddFlyoutBehaviorObserver(this);
		}

		public bool CanNavigateBack
		{
			get { return _canNavigateBack; }
			set
			{
				if (_canNavigateBack == value)
					return;
				_canNavigateBack = value;
				UpdateLeftBarButtonItem();
			}
		}

		public Page Page
		{
			get { return _page; }
			set
			{
				if (_page == value)
					return;
				var oldPage = _page;
				_page = value;
				OnPageChanged(oldPage, _page);
			}
		}

		public Color TintColor
		{
			get { return _tintColor; }
			set
			{
				_tintColor = value;
				if (Page != null)
				{
					UpdateToolbarItems();
					UpdateLeftBarButtonItem();
				}
			}
		}

		protected SearchHandler SearchHandler
		{
			get => _searchHandler;
			set
			{
				if (value == _searchHandler)
					return;

				var oldValue = _searchHandler;
				_searchHandler = value;
				OnSearchHandlerChanged(oldValue, _searchHandler);
			}
		}

		void AView.IOnClickListener.OnClick(AView v)
		{
			var backButtonHandler = Shell.GetBackButtonBehavior(Page);
			if (backButtonHandler?.Command != null)
				backButtonHandler.Command.Execute(backButtonHandler.CommandParameter);
			else if (CanNavigateBack)
				OnNavigateBack();
			else
				_shellContext.Shell.FlyoutIsPresented = !_shellContext.Shell.FlyoutIsPresented;

			v.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					UpdateTitleView(_shellContext.AndroidContext, _toolbar, null);

					_drawerToggle.Dispose();
					if (_searchView != null)
					{
						_searchView.View.RemoveFromParent();
						_searchView.View.ViewAttachedToWindow -= OnSearchViewAttachedToWindow;
						_searchView.SearchConfirmed -= OnSearchConfirmed;
						_searchView.Dispose();
					}

					((IShellController)_shellContext.Shell).RemoveFlyoutBehaviorObserver(this);
				}

				SearchHandler = null;
				_shellContext = null;
				_drawerToggle = null;
				_searchView = null;
				Page = null;
				_toolbar = null;
				_drawerLayout = null;
				_disposed = true;
			}
		}

		protected virtual IShellSearchView GetSearchView(Context context)
		{
			return new ShellSearchView(context, _shellContext);
		}

		protected virtual void OnNavigateBack()
		{
			Page.Navigation.PopAsync();
		}

		protected virtual void OnPageChanged(Page oldPage, Page newPage)
		{
			if (oldPage != null)
			{
				oldPage.PropertyChanged -= OnPagePropertyChanged;
				((INotifyCollectionChanged)oldPage.ToolbarItems).CollectionChanged -= OnPageToolbarItemsChanged;
			}

			if (newPage != null)
			{
				newPage.PropertyChanged += OnPagePropertyChanged;
				((INotifyCollectionChanged)newPage.ToolbarItems).CollectionChanged += OnPageToolbarItemsChanged;

				UpdatePageTitle(_toolbar, newPage);
				UpdateLeftBarButtonItem();
				UpdateToolbarItems();
				UpdateNavBarVisible(_toolbar, newPage);
				UpdateTitleView();
			}
		}

		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdatePageTitle(_toolbar, Page);
			else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
				UpdateToolbarItems();
			else if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
				UpdateNavBarVisible(_toolbar, Page);
			else if (e.PropertyName == Shell.BackButtonBehaviorProperty.PropertyName)
				UpdateLeftBarButtonItem();
			else if (e.PropertyName == Shell.TitleViewProperty.PropertyName)
				UpdateTitleView();
		}

		protected virtual void OnPageToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateToolbarItems();
		}

		protected virtual void OnSearchConfirmed(object sender, EventArgs e)
		{
			_toolbar.CollapseActionView();
		}

		protected virtual void OnSearchHandlerChanged(SearchHandler oldValue, SearchHandler newValue)
		{
			if (oldValue != null)
			{
				oldValue.PropertyChanged -= OnSearchHandlerPropertyChanged;
			}

			if (newValue != null)
			{
				newValue.PropertyChanged += OnSearchHandlerPropertyChanged;
			}
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.SearchBoxVisibilityProperty.PropertyName ||
				e.PropertyName == SearchHandler.IsSearchEnabledProperty.PropertyName)
			{
				UpdateToolbarItems();
			}
		}

		protected virtual async void UpdateLeftBarButtonItem(Context context, Toolbar toolbar, DrawerLayout drawerLayout, Page page)
		{
			var backButtonHandler = Shell.GetBackButtonBehavior(page);
			toolbar.SetNavigationOnClickListener(this);

			if (backButtonHandler != null)
			{
				using (var icon = await context.GetFormsDrawable(backButtonHandler.IconOverride))
				using (var mutatedIcon = icon.GetConstantState().NewDrawable().Mutate())
				{
					mutatedIcon.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					toolbar.NavigationIcon = mutatedIcon;
				}
			}
			else
			{
				var activity = (FormsAppCompatActivity)context;
				if (_drawerToggle == null)
				{
					_drawerToggle = new ActionBarDrawerToggle((Activity)context, drawerLayout, toolbar,
						R.String.Ok, R.String.Ok)
					{
						ToolbarNavigationClickListener = this,
					};
					_drawerToggle.DrawerSlideAnimationEnabled = false;
					drawerLayout.AddDrawerListener(_drawerToggle);
				}

				if (CanNavigateBack)
				{
					_drawerToggle.DrawerIndicatorEnabled = false;
					using (var icon = new DrawerArrowDrawable(activity.SupportActionBar.ThemedContext))
					{
						icon.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
						icon.Progress = 1;
						toolbar.NavigationIcon = icon;
					}
				}
				else if (_flyoutBehavior == FlyoutBehavior.Flyout)
				{
					toolbar.NavigationIcon = null;
					using (var drawable = _drawerToggle.DrawerArrowDrawable)
						drawable.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					_drawerToggle.DrawerIndicatorEnabled = true;
				}
				else
				{
					_drawerToggle.DrawerIndicatorEnabled = false;
				}
				_drawerToggle.SyncState();
			}
		}

		protected virtual void UpdateMenuItemIcon(Context context, IMenuItem menuItem, ToolbarItem toolBarItem)
		{
			FileImageSource icon = toolBarItem.Icon;
			if (!string.IsNullOrEmpty(icon))
			{
				using (var baseDrawable = context.GetFormsDrawable(icon))
				using (var constant = baseDrawable.GetConstantState())
				using (var newDrawable = constant.NewDrawable())
				using (var iconDrawable = newDrawable.Mutate())
				{
					iconDrawable.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					menuItem.SetIcon(iconDrawable);
				}
			}
		}

		protected virtual void UpdateNavBarVisible(Toolbar toolbar, Page page)
		{
			var navBarVisible = Shell.GetNavBarIsVisible(page);
			toolbar.Visibility = navBarVisible ? ViewStates.Visible : ViewStates.Gone;
		}

		protected virtual void UpdatePageTitle(Toolbar toolbar, Page page)
		{
			_toolbar.Title = page.Title;
		}

		protected virtual void UpdateTitleView(Context context, Toolbar toolbar, View titleView)
		{
			if (titleView == null)
			{
				if (_titleViewContainer != null)
				{
					_titleViewContainer.RemoveFromParent();
					_titleViewContainer.Dispose();
					_titleViewContainer = null;
				}
			}
			else
			{
				// FIXME
				titleView.Parent = _shellContext.Shell;
				_titleViewContainer = new ContainerView(context, titleView);
				_titleViewContainer.MatchHeight = _titleViewContainer.MatchWidth = true;
				_titleViewContainer.LayoutParameters = new Toolbar.LayoutParams(LP.MatchParent, LP.MatchParent)
				{
					LeftMargin = (int)context.ToPixels(titleView.Margin.Left),
					TopMargin = (int)context.ToPixels(titleView.Margin.Top),
					RightMargin = (int)context.ToPixels(titleView.Margin.Right),
					BottomMargin = (int)context.ToPixels(titleView.Margin.Bottom)
				};

				_toolbar.AddView(_titleViewContainer);
			}
		}

		protected virtual void UpdateToolbarItems(Toolbar toolbar, Page page)
		{
			var menu = toolbar.Menu;
			menu.Clear();

			foreach (var item in page.ToolbarItems)
			{
				using (var title = new Java.Lang.String(item.Text))
				{
					var menuitem = menu.Add(title);
					UpdateMenuItemIcon(_shellContext.AndroidContext, menuitem, item);
					menuitem.SetEnabled(item.IsEnabled);
					menuitem.SetShowAsAction(ShowAsAction.Always);
					menuitem.SetOnMenuItemClickListener(new GenericMenuClickListener(item.Activate));
					menuitem.Dispose();
				}
			}

			SearchHandler = Shell.GetSearchHandler(page);
			if (SearchHandler != null && SearchHandler.SearchBoxVisibility != SearchBoxVisiblity.Hidden)
			{
				var context = _shellContext.AndroidContext;
				if (_searchView == null)
				{
					_searchView = GetSearchView(context);
					_searchView.SearchHandler = SearchHandler;

					_searchView.LoadView();
					_searchView.View.ViewAttachedToWindow += OnSearchViewAttachedToWindow;

					var lp = new LP(LP.MatchParent, LP.MatchParent);
					_searchView.View.LayoutParameters = lp;
					lp.Dispose();
					_searchView.SearchConfirmed += OnSearchConfirmed;
				}

				if (SearchHandler.SearchBoxVisibility == SearchBoxVisiblity.Collapsable)
				{
					var placeholder = new Java.Lang.String(SearchHandler.Placeholder);
					var item = menu.Add(placeholder);
					placeholder.Dispose();

					item.SetEnabled(SearchHandler.IsSearchEnabled);
					item.SetIcon(Resource.Drawable.abc_ic_search_api_material);
					using (var icon = item.Icon)
						icon.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					item.SetShowAsAction(ShowAsAction.IfRoom | ShowAsAction.CollapseActionView);

					if (_searchView.View.Parent != null)
						_searchView.View.RemoveFromParent();

					_searchView.ShowKeyboardOnAttached = true;
					item.SetActionView(_searchView.View);
					item.Dispose();
				}
				else if (SearchHandler.SearchBoxVisibility == SearchBoxVisiblity.Expanded)
				{
					_searchView.ShowKeyboardOnAttached = false;
					if (_searchView.View.Parent != _toolbar)
						_toolbar.AddView(_searchView.View);
				}
			}
			else
			{
				if (_searchView != null)
				{
					_searchView.View.RemoveFromParent();
					_searchView.View.ViewAttachedToWindow -= OnSearchViewAttachedToWindow;
					_searchView.SearchConfirmed -= OnSearchConfirmed;
					_searchView.Dispose();
					_searchView = null;
				}
			}

			menu.Dispose();
		}

		void OnSearchViewAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
		{
			// We only need to do this tint hack when using collapsed search handlers
			if (SearchHandler.SearchBoxVisibility != SearchBoxVisiblity.Collapsable)
				return;

			for (int i = 0; i < _toolbar.ChildCount; i++)
			{
				var child = _toolbar.GetChildAt(i);
				if (child is AppCompatImageButton button)
				{
					// we want the newly added button which will need layout
					if (child.IsLayoutRequested)
					{
						button.SetColorFilter(TintColor.ToAndroid(Color.White), PorterDuff.Mode.SrcAtop);
					}

					button.Dispose();
				}
			}
		}

		void UpdateLeftBarButtonItem()
		{
			UpdateLeftBarButtonItem(_shellContext.AndroidContext, _toolbar, _drawerLayout, Page);
		}

		void UpdateTitleView()
		{
			UpdateTitleView(_shellContext.AndroidContext, _toolbar, Shell.GetTitleView(Page));
		}

		void UpdateToolbarItems()
		{
			UpdateToolbarItems(_toolbar, Page);
		}
	}
}