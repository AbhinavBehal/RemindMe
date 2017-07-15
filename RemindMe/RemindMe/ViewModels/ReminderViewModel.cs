using RemindMe.Models;
using RemindMe.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace RemindMe.ViewModels
{
    public class ReminderViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Reminder> _remindersList;
        private Reminder _selectedReminder;
        private bool _isRefreshing;

        public ICommand AddReminderCommand { get; private set; }
        public ICommand PullRemindersCommand { get; private set; }

        public ReminderViewModel()
        {
            AddReminderCommand = new Command(async () => await AddReminder());
            PullRemindersCommand = new Command(async () => await PullReminders());

            DatabaseManager.Instance.InsertEvent += async () => await PullReminders();
            DatabaseManager.Instance.RemoveEvent += async () => await PullReminders();

            PullRemindersCommand.Execute(null);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public Reminder SelectedReminder
        {
            get => _selectedReminder;
            set
            {
                _selectedReminder = value;
                OnPropertyChanged();

                if(SelectedReminder != null)
                {
                    OpenEditPage();
                }
            }
        }


        public ObservableCollection<Reminder> RemindersList
        {
            get => _remindersList;
            set
            {
                _remindersList = value;
                OnPropertyChanged();
            }
        }

        public async void OpenEditPage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new EditReminderPage(new ReminderDetailViewModel(SelectedReminder)));
            SelectedReminder = null;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task AddReminder()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AddReminderPage(new ReminderDetailViewModel(SelectedReminder)));
            SelectedReminder = null;
        }

        private async Task PullReminders()
        {
            IsRefreshing = true;
            var result = await DatabaseManager.Instance.GetReminders();
            RemindersList = new ObservableCollection<Reminder>(result);
            IsRefreshing = false;
        }
    }
}
