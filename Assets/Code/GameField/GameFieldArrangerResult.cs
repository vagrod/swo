using System.Collections.Generic;
using SeaWarsOnline.Core.GameObjects;

namespace SeaWarsOnline.Core
{
    public class GameFieldArrangerResult
    {

        #region Properties

        public List<DesignTimeShip> ShipsArranged { get; set; } = new List<DesignTimeShip>();

        public List<DesignTimeMine> MinesArranged { get; set; } = new List<DesignTimeMine>();

        #endregion

    }
}
