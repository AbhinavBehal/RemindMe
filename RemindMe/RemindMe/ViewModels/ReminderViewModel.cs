using RemindMe.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace RemindMe.ViewModels
{
    public class ReminderViewModel
    {
        private ObservableCollection<Reminder> _remindersList;
        private Reminder _selectedReminder;

        public ICommand AddReminderCommand { get; private set; }

        public ReminderViewModel()
        {
            _remindersList = new ObservableCollection<Reminder>();
            AddReminderCommand = new Command(AddReminder);
        }

        private async void AddReminder()
        {
            await Task.Run(
                () => _remindersList.Add(new Reminder("ahahe", "wuehwuheuweuhwe weuh wuhe wu heuhwe ", DateTime.Now)));
        }

        public Reminder SelectedReminder
        {
            get => _selectedReminder;
            set
            {
                _selectedReminder = value;
            }
        }

        public ObservableCollection<Reminder> RemindersList
        {
            get => _remindersList;
            set
            {
                _remindersList = value;
            }
        }
    }
}
