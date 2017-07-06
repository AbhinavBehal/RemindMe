using RemindMe.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RemindMe.ViewModels
{
    public class ReminderViewModel
    {
        private ObservableCollection<Reminder> _remindersList;
        private Reminder _selectedReminder;

        public ReminderViewModel()
        {
            _remindersList = new ObservableCollection<Reminder>();
        }

        public Reminder SelectedReminder
        {
            get => _selectedReminder;
            set
            {
                _selectedReminder = value;
            }
        }
    }
}
