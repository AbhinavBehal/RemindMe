using LocalNotifications.Plugin;
using LocalNotifications.Plugin.Abstractions;
using RemindMe.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace RemindMe.ViewModels
{
    public class ReminderDetailViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Reminder _reminder;
        private string _titleInput;
        private string _descriptionInput;
        private string _cachedTitle;
        private string _cachedDescription;
        private string _spellcheckLabel;
        private bool _canRevert;
        private bool _isSpellchecking;
        private DateTime _dateInput;
        private DateTime _minimumDate;
        private TimeSpan _timeInput;

        public ICommand SubmitReminderCommand { get; private set; }
        public ICommand DeleteReminderCommand { get; private set; }
        public ICommand SpellcheckCommand { get; private set; }

        public ReminderDetailViewModel(Reminder reminder)
        {
            _reminder = reminder;
            if(reminder != null)
            {
                _titleInput = reminder.Title;
                _descriptionInput = reminder.Description;
                _dateInput = reminder.Date;
                _timeInput = reminder.Date.TimeOfDay;
            }
            else
            {
                _dateInput = DateTime.Today;
                _timeInput = DateTime.Now.TimeOfDay;
            }

            _canRevert = false;
            _spellcheckLabel = "Spellcheck";
            _minimumDate = DateTime.Today;

            SubmitReminderCommand = new Command(async () => await SubmitReminder(), () => !string.IsNullOrWhiteSpace(TitleInput));
            DeleteReminderCommand = new Command(async () => await DeleteReminder());
            SpellcheckCommand = new Command(async () => await Spellcheck());
        }

        public string TitleInput
        {
            get => _titleInput;
            set
            {
                if(_canRevert && _titleInput != value)
                {
                    _canRevert = false;
                    SpellcheckLabel = "Spellcheck";
                }
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
                if (_canRevert && _descriptionInput != value)
                {
                    _canRevert = false;
                    SpellcheckLabel = "Spellcheck";
                }
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

        public DateTime MinimumDate
        {
            get => _minimumDate;
            set
            {
                _minimumDate = value;
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
            }
        }

        public string SpellcheckLabel
        {
            get => _spellcheckLabel;
            set
            {
                _spellcheckLabel = value;
                OnPropertyChanged();
            }
        }

        public bool IsSpellchecking
        {
            get => _isSpellchecking;
            set
            {
                _isSpellchecking = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task SubmitReminder()
        {
            var date = new DateTime(DateInput.Year, DateInput.Month, DateInput.Day, TimeInput.Hours, TimeInput.Minutes, TimeInput.Seconds);

            if(_reminder == null)
                await DatabaseManager.Instance.PostReminder(new Reminder(TitleInput, DescriptionInput, date));
            else
            {
                _reminder.Title = TitleInput;
                _reminder.Description = DescriptionInput;
                _reminder.Date = date;
                await DatabaseManager.Instance.UpdateReminder(_reminder);
            }
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task DeleteReminder()
        {
            bool result = await Application.Current.MainPage.DisplayAlert("Delete Reminder", "Are you sure?", "Delete", "Cancel");
            if (!result)
                return;

            await DatabaseManager.Instance.DeleteReminder(_reminder);
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private async Task Spellcheck()
        {
            if(_canRevert)
            {
                TitleInput = _cachedTitle;
                DescriptionInput = _cachedDescription;
                _canRevert = false;
            }
            else
            {
                IsSpellchecking = true;
                _cachedTitle = TitleInput;
                _cachedDescription = DescriptionInput;
                var changedTitle = await Spellchecker.Check(TitleInput);
                var changedDescription = await Spellchecker.Check(DescriptionInput);

                TitleInput = changedTitle;
                DescriptionInput = changedDescription;

                _canRevert = _cachedTitle != changedTitle || _cachedDescription != changedDescription;
                IsSpellchecking = false;
            }
            
            if (_canRevert)
                SpellcheckLabel = "Revert";
            else
                SpellcheckLabel = "Spellcheck";
        }
    }
}
