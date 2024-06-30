using System;
namespace ZooManager
{
    public class Zone
    {
        private Occupant _occupant = null;
        public Occupant occupant
        {
            get { return _occupant; }
            set {
                _occupant = value;
                if (_occupant != null) {
                    _occupant.location = location;
                }
            }
        }

        public Point location;

        public string emoji
        {
            get
            {
                if (occupant == null) return "";
                return occupant.emoji;
            }
        }

        public string rtLabel
        {
            get
            {
                if (occupant as Animal == null) return "";
                return ((Animal)occupant).reactionTime.ToString();
            }
        }

        public Zone(int x, int y, Occupant occupant)
        {
            location.x = x;
            location.y = y;

            this.occupant = occupant;
        }
    }
}
