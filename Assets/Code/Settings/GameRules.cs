using System.Collections.Generic;
using Photon.Realtime;
using SeaWarsOnline.Core.GameObjects;

namespace SeaWarsOnline.Core
{
    public class GameRules
    {

        #region Enums

        public enum GameKinds
        {
            SinglePlayer,
            Network
        }

        #endregion

        #region Properties

        public GameKinds GameKind { get; set; }

        public int GameFieldSize { get; set; }

        public bool StraightShips { get; set; }
        
        public int DensityProfile { get; set; }

        public bool AllowMines { get; set; }

        public bool AllowSpyGlass { get; set; }
        
        public bool ChangeTurnEachTime { get; set; }

        public short CountShipsSmall{ get; set; }

        public short CountShipsMedium { get; set; }

        public short CountShipsLarge { get; set; }

        public short CountShipsExtraLarge { get; set; }

        public short CountMines { get; set; }

        public List<Ship> PlayerShips { get; set; } = new List<Ship>();

        public List<Mine> PlayerMines { get; set; } = new List<Mine>();

        #endregion

        #region Static Methods

        public static GameRules Default(){
            return new GameRules
            {
                GameKind = GameKinds.SinglePlayer,
                GameFieldSize = 10,
                StraightShips = true,
                ChangeTurnEachTime = false,
                DensityProfile = 1 // Medium
            };
        }

        #endregion

        public void ReadFromRoom(RoomInfo room)
        {
            GameKind = GameKinds.Network;

            var fieldSize = (int)room.CustomProperties["fieldSize"];
            var allowMines = room.CustomProperties["allowMines"].Equals(1);
            var allowSpyGlass = room.CustomProperties["allowSpyGlass"].Equals(1);
            var straightShips = room.CustomProperties["straightShips"].Equals(1);
            var oneByOneTurns = room.CustomProperties["oneByOneTurns"].Equals(1);
            var density = (int)room.CustomProperties["densityProfile"];

            GameFieldSize = fieldSize;
            AllowSpyGlass = allowSpyGlass;
            AllowMines = allowMines;
            ChangeTurnEachTime = oneByOneTurns;
            StraightShips = straightShips;
            DensityProfile = density;

            CountMines = (short)room.CustomProperties["countMines"];
            CountShipsSmall = (short)room.CustomProperties["countSmall"];
            CountShipsMedium = (short)room.CustomProperties["countMedium"];
            CountShipsLarge = (short)room.CustomProperties["countLarge"];
            CountShipsExtraLarge = (short)room.CustomProperties["countExtraLarge"];
        }
    }
}
