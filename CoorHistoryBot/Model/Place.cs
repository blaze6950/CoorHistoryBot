using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CoorHistoryBot
{
    class Place
    {
        const int MAX_COMPLAINTS = 5;
        private long _id;
        private int _complaints; // жалобы, когда появляется 5 штук, запись о месте переносится в промежуточную таблицу, для модерации
        private Location _location;
        private List<byte[]> _photos;
        private StringBuilder _caption;
        private User _user;

        public Place(User user)
        {
            _user = user;
            _complaints = 0;
            _location = null;
            _photos = new List<byte[]>();
            _caption = new StringBuilder();
        }

        public Place(Location location, List<byte[]> photos, StringBuilder caption, User user)
        {
            _user = user;
            _complaints = 0;
            _location = location;
            _photos = photos;
            _caption = caption;
        }

        public Place(Location location, StringBuilder caption)
        {
            Location = location;
            Caption = caption;
        }

        public int Complaints
        {
            get
            {
                return _complaints;
            }

            set
            {
                _complaints = value;
                if (_complaints >= MAX_COMPLAINTS)
                {
                    LoadToListOfModeration();
                }
            }
        }

        public Location Location { get => _location; set => _location = value; }
        public List<byte[]> Photos { get => _photos; set => _photos = value; }
        public StringBuilder Caption { get => _caption; set => _caption = value; }
        public User User { get => _user; set => _user = value; }

        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private void LoadToListOfModeration()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return _caption.ToString();
        }
    }
}
