using LocalNotifications.Plugin;
using LocalNotifications.Plugin.Abstractions;
using RemindMe.Models;
using RemindMe.Views;
using System;
using System.Linq;
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
        private Dictionary<Reminder, LocalNotification> _notifications;
        private Reminder _selectedReminder;
        private bool _isRefreshing;

        public ICommand AddReminderCommand { get; private set; }
        public ICommand PullRemindersCommand { get; private set; }
        public ICommand ClearExpiredCommand { get; private set; }

        public ReminderViewModel()
        {
            AddReminderCommand = new Command(async () => await ShowAddPage());
            PullRemindersCommand = new Command(async () => await PullReminders());
            ClearExpiredCommand = new Command(async () => await ClearExpired());

            _notifications = new Dictionary<Reminder, LocalNotification>();

            PullRemindersCommand.Execute(null);

            DatabaseManager.Instance.InsertEvent += OnReminderAdded;
            DatabaseManager.Instance.RemoveEvent += OnReminderDeleted;
            DatabaseManager.Instance.UpdateEvent += OnReminderUpdated;
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
                    ShowEditPage();
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void ShowEditPage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new EditReminderPage(new ReminderDetailViewModel(SelectedReminder)));
            SelectedReminder = null;
        }

        private async Task ShowAddPage()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AddReminderPage(new ReminderDetailViewModel(SelectedReminder)));
            SelectedReminder = null;
        }

        private async Task ClearExpired()
        {
            IsRefreshing = true;
            // Create a separate list as each DeleteReminder() call updates RemindersList while we're iterating
            // which throws an exception
            var list = RemindersList.ToList();
            foreach (Reminder r in list)
            {
                if (r.Date < DateTime.Now)
                    await DatabaseManager.Instance.DeleteReminder(r);
            }
            IsRefreshing = false;
        }

        private void OnReminderAdded(Reminder reminder)
        {
            RemindersList.Add(reminder);
            if (reminder.Date < DateTime.Now) return;

            var notification = new LocalNotification
            {
                Title = reminder.Title,
                Text = reminder.Description,
                NotifyTime = reminder.Date,
                Id = IdGenerator.NextId
            };
            CrossLocalNotifications.CreateLocalNotifier().Notify(notification);
            _notifications.Add(reminder, notification);
        }

        private void OnReminderDeleted(Reminder reminder)
        { 
            var result = RemindersList.Remove(reminder);
            if (!_notifications.ContainsKey(reminder)) return;

            CrossLocalNotifications.CreateLocalNotifier().Cancel(_notifications[reminder].Id);
            _notifications.Remove(reminder);
        }

        private void OnReminderUpdated(Reminder reminder)
        {
            // Don't need to update RemindersList because of INotifyPropertyChanged on Reminder

            var notification = new LocalNotification
            {
                Title = reminder.Title,
                Text = reminder.Description,
                NotifyTime = reminder.Date,
                Id = IdGenerator.NextId
            };
            var notifier = CrossLocalNotifications.CreateLocalNotifier();


            if(_notifications.ContainsKey(reminder)) // Cancel and update the current notification if its not expired
            {
                notifier.Cancel(_notifications[reminder].Id);
                if (reminder.Date < DateTime.Now)
                {
                    _notifications.Remove(reminder);
                    return;
                }

                _notifications[reminder] = notification;
            }
            else // Add a new notification
            {
                _notifications.Add(reminder, notification);
            }
            notifier.Notify(_notifications[reminder]);
        }

        private async Task PullReminders()
        {
            IsRefreshing = true;
            var result = await DatabaseManager.Instance.GetReminders();

            RemindersList = new ObservableCollection<Reminder>(result);

            var notifier = CrossLocalNotifications.CreateLocalNotifier();
            foreach(LocalNotification n in _notifications.Values)
            {
                notifier.Cancel(n.Id);
            }
            _notifications.Clear();
            IdGenerator.Clear();

            foreach(Reminder r in RemindersList)
            {
                if (r.Date < DateTime.Now)
                    continue;

                var notification = new LocalNotification
                {
                    Title = r.Title,
                    Text = r.Description,
                    NotifyTime = r.Date,
                    Id = IdGenerator.NextId
                };
                _notifications.Add(r, notification);
                notifier.Notify(notification);
            }
            IsRefreshing = false;
        }
    }
}
