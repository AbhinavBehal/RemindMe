using System;
using System.Collections.Generic;
using System.Text;

namespace RemindMe
{
    public class IdGenerator
    {
        private static int _currentId = 1;

        public static int NextId
        {
            get => _currentId++;
        }

        public static void Clear()
        {
            _currentId = 1;
        }
    }
}
