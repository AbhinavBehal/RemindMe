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
        private ObservableCollection<Reminder> _remindersList;
        private Reminder _selectedReminder;

        private string _titleInput;
        private string _descriptionInput;
        private DateTime _dateInput;
        private TimeSpan _timeInput;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand AddReminderCommand { get; private set; }
        public ICommand SubmitReminderCommand { get; private set; }

        public ICommand SaveEditCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand DeleteReminderCommand { get; private set; }

        public ReminderViewModel()
        {
            _remindersList = new ObservableCollection<Reminder>();
            _remindersList.Add(new Reminder("Lorem Ipsum", 
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua", 
                DateTime.Now));

            AddReminderCommand = new Command(AddReminder);
            SubmitReminderCommand = new Command(SubmitReminder, () => !string.IsNullOrWhiteSpace(TitleInput));

            SaveEditCommand = new Command(SaveEdit);
            CancelEditCommand = new Command(CancelEdit);
            DeleteReminderCommand = new Command(DeleteReminder);

            ClearFields();
        }

        private void ClearFields()
        {
            TitleInput = "";
            DescriptionInput = "";
            DateInput = DateTime.Now;
            TimeInput = DateInput.TimeOfDay;
        }

        private async void AddReminder()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AddReminderPage(this));
        }
        
        private async void SubmitReminder()
        {
            await Task.Run(
                () => _remindersList.Add(new Reminder(TitleInput, DescriptionInput, DateInput)));

            await Application.Current.MainPage.Navigation.PopAsync();

            ClearFields();
        }

        private async void SaveEdit()
        {
            SelectedReminder.Title = TitleInput;
            SelectedReminder.Description = DescriptionInput;
            SelectedReminder.Date = DateInput;

            SelectedReminder = null;
            ClearFields();
            await Application.Current.MainPage.Navigation.PopAsync();
        }
        
        private async void CancelEdit()
        {
            SelectedReminder = null;
            ClearFields();
            await Application.Current.MainPage.Navigation.PopAsync();
        }
        
        private async void DeleteReminder()
        {
            var result = await Application.Current.MainPage.DisplayAlert("Delete reminder", "Are you sure?", "Delete", "Cancel");
            if(result)
            {
                await Task.Run(() => _remindersList.Remove(SelectedReminder));
                SelectedReminder = null;
                ClearFields();
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    TitleInput = SelectedReminder.Title;
                    DescriptionInput = SelectedReminder.Description;
                    DateInput = SelectedReminder.Date;
                    TimeInput = SelectedReminder.Date.TimeOfDay;

                    Application.Current.MainPage.Navigation.PushAsync(new EditReminderPage(this));
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

        public string TitleInput
        {
            get => _titleInput;
            set
            {
                _titleInput = value;
                OnPropertyChanged();
                ((Command)SubmitReminderCommand).ChangeCanExecute();
            }
        }

        public string DescriptionInput
        {
            get => _descriptionInput;
            set
            {
                _descriptionInput = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateInput
        {
            get => _dateInput;
            set
            {
                _dateInput = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan TimeInput
        {
            get => _timeInput;
            set
            {
                _timeInput = value;
                OnPropertyChanged();

                DateInput = new DateTime(DateInput.Year, DateInput.Month, DateInput.Day, TimeInput.Hours, TimeInput.Minutes, TimeInput.Seconds);
            }
        }
    }
}
