using System;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ItemAdder : CollectionModifier 
	{
		public ItemAdder(CollectionView cv) : base(cv, "Adder")
		{
		}

		protected override void ModifyCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var item = new CollectionViewGalleryTestItem(DateTime.Now, "Added", "oasis.jpg", observableCollection.Count +1);
			observableCollection.Add(item);
		}
	}
}