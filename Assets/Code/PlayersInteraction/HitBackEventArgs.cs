using System;

namespace SeaWarsOnline.Core
{
    public class HitBackEventArgs : EventArgs
    {

        #region Properties

        public Point CellLocation { get; set; }

        #endregion

    }
}
