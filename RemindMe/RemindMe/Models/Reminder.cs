using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace RemindMe.Models
{
    public class Reminder : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        private DateTime _date;

        public event PropertyChangedEventHandler PropertyChanged;

        public Reminder(string title, string description, DateTime date)
        {
            _title = title;
            _description = description;
            _date = date;
        }

        [JsonProperty(PropertyName = "Id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty(PropertyName = "Description")]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty(PropertyName = "RemindTime")]
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateDisplay));
            }
        }

        public string DateDisplay
        {
            get
            {
                var formatted = Date.ToString("hh:mm dd/MM");
                if (Date < DateTime.Now)
                    formatted += " Expired";

                return formatted;
            }

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }
}
