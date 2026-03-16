using System;
using System.Collections.Generic;

namespace BingoSync.Sessions
{
    public class BingoBoard
    {
        private List<Square> _squares;
        public bool IsRevealed { get; set; } = false;
        public bool IsConfirmed { get; set; } = false;
        public bool IsAvailable => (_squares != null);
        public List<Square> AllSquares => _squares;
        public List<Square> SquaresToDisplay => DisplaySquaresSelector(_squares);

        private Func<List<Square>, List<Square>> DisplaySquaresSelector = DefaultDisplaySquaresSelector;

        private static List<Square> DefaultDisplaySquaresSelector(List<Square> allSquares)
        {
            return allSquares;
        }

        public void SetDisplaySquaresSelector(Func<List<Square>, List<Square>> selector)
        {
            DisplaySquaresSelector = selector ?? DefaultDisplaySquaresSelector;
        }

        public void SetDefaultDisplaySquaresSelector()
        {
            DisplaySquaresSelector = DefaultDisplaySquaresSelector;
        }

        public void SetSquares(List<Square> squares)
        {
            _squares = squares;
        }

        public void Clear()
        {
            foreach(Square square in _squares)
            {
                square.MarkedBy.Clear();
                square.MarkedBy.Add(Colors.Blank);
            }
        }

        public Square GetIndex(int index)
        {
            return _squares[index];
        }

        // bingosync.com uses "slot" as a 1-based index, hence the separation internally
        public Square GetSlot(int slot)
        {
            return GetIndex(slot - 1);
        }

    }
}
