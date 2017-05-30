using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloGame.Utility
{
    public class Enums
    {
        public enum Alignment
        {
            Left,
            TopLeft,
            BottomLeft,
            Right,
            TopRight,
            BottomRight,
            Top,
            Bottom,
            Center
        }

        public enum DirectionBinary
        {
            Left,
            Right
        }

        public enum DirectionClock
        {
            Clockwise,
            CounterClockwise
        }

        public enum DirectionCardinal
        {
            West,
            North,
            East,
            South
        }

        public enum DirectionCardinalG
        {
            Left,
            Up,
            Right,
            Down
        }

        public enum DirectionInterCardinal
        {
            West,
            NorthWest,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest
        }
    }
}
