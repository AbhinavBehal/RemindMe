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
        private string _cachedTitle;
        private string _cachedDescription;

        private bool _isRefreshing;
        private bool _canRevert;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand AddReminderCommand { get; private set; }
        public ICommand SubmitReminderCommand { get; private set; }

        public ICommand SaveEditCommand { get; private set; }
        public ICommand CancelEditCommand { get; private set; }
        public ICommand DeleteReminderCommand { get; private set; }

        public ICommand PullRemindersCommand { get; private set; }

        public ICommand SpellcheckCommand { get; private set; }
        public ICommand RevertSpellcheckCommand { get; private set; }

        public ReminderViewModel()
        {
            _canRevert = false;

            AddReminderCommand          = new Command(async () => await AddReminder());
            SubmitReminderCommand       = new Command(async () => await SubmitReminder(), () => !string.IsNullOrWhiteSpace(TitleInput));
            SaveEditCommand             = new Command(async () => await SaveEdit());
            CancelEditCommand           = new Command(async () => await CancelEdit());
            DeleteReminderCommand       = new Command(async () => await DeleteReminder());
            PullRemindersCommand        = new Command(async () => await PullReminders());
            SpellcheckCommand           = new Command(async () => await Spellcheck(), () => !string.IsNullOrWhiteSpace(TitleInput) || !string.IsNullOrWhiteSpace(DescriptionInput));
            RevertSpellcheckCommand     = new Command(RevertSpellcheck, () => _canRevert);

            PullRemindersCommand.Execute(null);
        }

        private async Task Spellcheck()
        {
            _cachedTitle = TitleInput;
            _cachedDescription = DescriptionInput;

            var titleChecked = await Spellchecker.Check(TitleInput);
            var descriptionChecked = await Spellchecker.Check(DescriptionInput);

            _canRevert = !string.Equals(TitleInput, titleChecked) || !string.Equals(DescriptionInput, descriptionChecked);
            TitleInput = titleChecked;
            DescriptionInput = descriptionChecked;

            ((Command)RevertSpellcheckCommand).ChangeCanExecute();
        }

        private void RevertSpellcheck()
        {
            TitleInput = _cachedTitle;
            DescriptionInput = _cachedDescription;

            _canRevert = false;
            ((Command)RevertSpellcheckCommand).ChangeCanExecute();
        }

        private async Task PullReminders()
        {
            IsRefreshing = true;
            var result = await DatabaseManager.Instance.GetReminders();
            RemindersList = new ObservableCollection<Reminder>(result);
            IsRefreshing = false;
        }

        private void ClearFields()
        {
            TitleInput = "";
            DescriptionInput = "";
            _cachedTitle = "";
            _cachedDescription = "";
            _canRevert = false;
        }

        private async Task AddReminder()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new AddReminderPage(this));
        }
        
        private async Task SubmitReminder()
        {
            await DatabaseManager.Instance.PostReminder(new Reminder(TitleInput, DescriptionInput));
            await Application.Current.MainPage.Navigation.PopAsync();
            ClearFields();
            PullRemindersCommand.Execute(null);
        }

        private async Task SaveEdit()
        {
            SelectedReminder.Title = TitleInput;
            SelectedReminder.Description = DescriptionInput;

            await DatabaseManager.Instance.UpdateReminder(SelectedReminder);
            SelectedReminder = null;
            ClearFields();
            await Application.Current.MainPage.Navigation.PopAsync();
            PullRemindersCommand.Execute(null);
        }
        
        private async Task CancelEdit()
        {
            SelectedReminder = null;
            ClearFields();
            await Application.Current.MainPage.Navigation.PopAsync();
        }
        
        private async Task DeleteReminder()
        {
            var result = await Application.Current.MainPage.DisplayAlert("Delete reminder", "Are you sure?", "Delete", "Cancel");
            if(result)
            {
                await DatabaseManager.Instance.DeleteReminder(SelectedReminder);
                SelectedReminder = null;
                ClearFields();
                await Application.Current.MainPage.Navigation.PopAsync();
                
                PullRemindersCommand.Execute(null);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    TitleInput = SelectedReminder.Title;
                    DescriptionInput = SelectedReminder.Description;

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
                ((Command)SpellcheckCommand).ChangeCanExecute();
            }
        }

        public string DescriptionInput
        {
            get => _descriptionInput;
            set
            {
                _descriptionInput = value;
                OnPropertyChanged();
                ((Command)SpellcheckCommand).ChangeCanExecute();
            }
        }
    }
}
