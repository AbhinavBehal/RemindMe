﻿using RemindMe.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RemindMe
{
	public partial class MainPage : ContentPage
	{
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new ReminderViewModel();
        }
    }
}
