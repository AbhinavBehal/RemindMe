using RemindMe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RemindMe.Views
{
	public partial class AddReminderPage : ContentPage
	{
		public AddReminderPage (ReminderDetailViewModel detailViewModel)
		{
			InitializeComponent ();
            BindingContext = detailViewModel;
		}
	}
}