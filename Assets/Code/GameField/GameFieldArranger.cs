using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeaWarsOnline.Core.GameObjects;

namespace SeaWarsOnline.Core
{
    public class GameFieldArranger
    {

        #region Singleton Stuff 

        private GameFieldArranger() { }

        private static GameFieldArranger _instance;

        public static GameFieldArranger Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameFieldArranger();
                }

                return _instance;
            }
        }

        #endregion

        #region Methods

        private System.Random _random = new System.Random();

        public Task<GameFieldArrangerResult> Arrange(GameRules rules)
        {
            return Task.Run(() =>
            {
                var field = new GameField(rules.GameFieldSize);
                var shipsToPlaceExtraLarge = new List<DesignTimeShip>();
                var shipsToPlaceLarge = new List<DesignTimeShip>();
                var shipsToPlaceMedium = new List<DesignTimeShip>();
                var shipsToPlaceSmall = new List<DesignTimeShip>();
                var minesToPlace = new List<DesignTimeMine>();

                for (int i = 1; i <= rules.CountShipsExtraLarge; i++)
                    shipsToPlaceExtraLarge.Add(new DesignTimeShip(field, 4));

                for (int i = 1; i <= rules.CountShipsLarge; i++)
                    shipsToPlaceLarge.Add(new DesignTimeShip(field, 3));

                for (int i = 1; i <= rules.CountShipsMedium; i++)
                    shipsToPlaceMedium.Add(new DesignTimeShip(field, 2));

                for (int i = 1; i <= rules.CountShipsSmall; i++)
                    shipsToPlaceSmall.Add(new DesignTimeShip(field, 1));

                for (int i = 1; i <= rules.CountMines; i++)
                    minesToPlace.Add(new DesignTimeMine(field));

                while(true){
                    var failedTry = false;

                    failedTry = !PlaceShips(field, rules.CountShipsExtraLarge, rules.StraightShips, shipsToPlaceExtraLarge);
                    
                    if (!failedTry)
                        failedTry = !PlaceShips(field, rules.CountShipsLarge, rules.StraightShips, shipsToPlaceLarge);

                    if (!failedTry)
                        failedTry = !PlaceShips(field, rules.CountShipsMedium, rules.StraightShips, shipsToPlaceMedium);

                    if (!failedTry)
                        failedTry = !PlaceShips(field, rules.CountShipsSmall, rules.StraightShips, shipsToPlaceSmall);

                    if (rules.AllowMines){
                        if (!failedTry)
                            failedTry = !PlaceMines(field, rules.CountMines,minesToPlace);
                    }

                    if (!failedTry && (shipsToPlaceExtraLarge.All(x => x.Cells.Count == x.Size) &&
                            shipsToPlaceLarge.All(x => x.Cells.Count == x.Size) &&
                            shipsToPlaceMedium.All(x => x.Cells.Count == x.Size) &&
                            shipsToPlaceSmall.All(x => x.Cells.Count == x.Size))){
                        break;
                    } 

                    field.Clear();

                    shipsToPlaceExtraLarge.ForEach(x =>{ 
                        x.AvailableNewCellLocations.Clear();
                        x.Cells.Clear();
                    });
                    shipsToPlaceLarge.ForEach(x =>{ 
                        x.AvailableNewCellLocations.Clear();
                        x.Cells.Clear();
                    });
                    shipsToPlaceMedium.ForEach(x =>{ 
                        x.AvailableNewCellLocations.Clear();
                        x.Cells.Clear();
                    });
                    shipsToPlaceSmall.ForEach(x =>{ 
                        x.AvailableNewCellLocations.Clear();
                        x.Cells.Clear();
                    });
                }

                var resShips = shipsToPlaceExtraLarge.Union(shipsToPlaceLarge.Union(shipsToPlaceMedium.Union(shipsToPlaceSmall))).ToList();
                var resMines = minesToPlace.ToList();
                
                return Task.FromResult(new GameFieldArrangerResult{
                    ShipsArranged = resShips,
                    MinesArranged = resMines
                });
            });
        }

        private bool PlaceShips(GameField field, int shipsCount, bool isStraight, IEnumerable<DesignTimeShip> shipsToPlace){
            for (int i = 1; i <= shipsCount; i++){
                var rp = GetNextRandomPoint(field, ignoreObstacles: false);

                if (rp == null){
                    //UnityEngine.Debug.Log($"Cannot find location for ship. Starting over.");
                    return false;
                }

                var ship = shipsToPlace.ElementAt(i-1);

                ship.AddCell(rp, isStraight);

                while(ship.IsIncomplete){
                    if (ship.AvailableNewCellLocations.Count == 0){
                        //UnityEngine.Debug.Log($"Cannot complete the ship. Starting over.");
                        return false;
                    }

                    var ind = _random.Next(0, ship.AvailableNewCellLocations.Count - 1);
                    var p = ship.AvailableNewCellLocations[ind];

                    ship.AddCell(p, isStraight);

                    if (ship.AvailableNewCellLocations.Count == 0 && ship.IsIncomplete){
                        //UnityEngine.Debug.Log($"Cannot complete the ship. Starting over.");
                        return false;
                    }
                } 
            }

            return true;
        }

        private bool PlaceMines(GameField field, int minesCount, IEnumerable<DesignTimeMine> minesToPlace){
            for (int i = 1; i <= minesCount; i++){
                var rp = GetNextRandomPoint(field, ignoreObstacles: true);

                if (rp == null || field.CellAtPoint(rp).State == FieldCellBase.CellStates.Ship){
                    //UnityEngine.Debug.Log($"Cannot find location for mine. Starting over.");
                    return false;
                }

                minesToPlace.ElementAt(i-1).AddCell(rp);
            }

            return true;
        }

        private Point GetNextRandomPoint(GameField field, bool ignoreObstacles){
            var p = TryGetNextRandomPoint(field, 0, ignoreObstacles);

            if (p.Item2==null || (!ignoreObstacles && p.Item2.HasObstacleNearPoint(field)))
            {
                while(p.Item1 > 0) {
                    p = TryGetNextRandomPoint(field, p.Item1, ignoreObstacles);

                    if (p.Item2!=null){
                        //UnityEngine.Debug.Log($"Found at index {p.Item2.Y * field.FieldSize + p.Item2.X}");
                        return p.Item2;
                    }
                }                
            } else {
                return p.Item2;
            }

           return null;
        }

        private Tuple<int, Point> TryGetNextRandomPoint(GameField field, int rightWall, bool ignoreObstacles){
            var random = new System.Random();
            var startX = _random.Next(0, field.FieldSize - 1);
            var startY = _random.Next(0, field.FieldSize - 1);
            var n = rightWall == 0 ? startY*field.FieldSize+startX : rightWall - 10;

            //if(rightWall > 0)
                //UnityEngine.Debug.Log($"Stepping back to index {n}");

            if (n<0) 
                n=0;

            var m = rightWall == 0 ? field.FieldSize * field.FieldSize : rightWall;

            for (var i = n; i < m; i++)
            {
                var c = field.CellAtIndex(i);

                if (c != null && c.IsEmpty && (ignoreObstacles || !c.Location.HasObstacleNearPoint(field)))
                    return new Tuple<int, Point>(n, c.Location);
            }

            return new Tuple<int, Point>(n, null);
        }

        #endregion

    }
}
