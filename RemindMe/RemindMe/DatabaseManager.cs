using Microsoft.WindowsAzure.MobileServices;
using RemindMe.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RemindMe
{
    public class DatabaseManager
    {
        public delegate void InsertEventHandler(Reminder reminder);
        public event InsertEventHandler InsertEvent;

        public delegate void RemoveEventHandler(Reminder reminder);
        public event RemoveEventHandler RemoveEvent;

        public delegate void UpdateEventHandler(Reminder reminder);
        public event UpdateEventHandler UpdateEvent;

        private static DatabaseManager _instance;
        private MobileServiceClient _client;
        private IMobileServiceTable<Reminder> _reminderTable;

        private DatabaseManager()
        {
            _client = new MobileServiceClient("https://msa-remindme.azurewebsites.net");
            _reminderTable = _client.GetTable<Reminder>();
        }

        public MobileServiceClient Client => _client;

        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DatabaseManager();

                return _instance;
            }
        }

        public async Task<List<Reminder>> GetReminders()
        {
            return await _reminderTable.ToListAsync();
        }

        public async Task PostReminder(Reminder reminder)
        {
            await _reminderTable.InsertAsync(reminder);
            InsertEvent?.Invoke(reminder);
        }

        public async Task UpdateReminder(Reminder reminder)
        {
            await _reminderTable.UpdateAsync(reminder);
            UpdateEvent?.Invoke(reminder);
        }

        public async Task DeleteReminder(Reminder reminder)
        {
            await _reminderTable.DeleteAsync(reminder);
            RemoveEvent?.Invoke(reminder);
        }
    }
}
