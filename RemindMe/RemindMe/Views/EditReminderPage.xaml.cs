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
	public partial class EditReminderPage : ContentPage
	{
        private ReminderViewModel _vm;
		public EditReminderPage (ReminderViewModel vm)
		{
			InitializeComponent ();
            BindingContext = vm;
            _vm = vm;
		}
    }
}