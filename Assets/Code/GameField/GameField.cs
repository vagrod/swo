using System;
using System.Collections.Generic;
using System.Linq;
using SeaWarsOnline.Core.GameObjects;
using UnityEngine;

namespace SeaWarsOnline.Core
{
    public class GameField 
	{

        #region Events

        public event EventHandler<CellStateEventArgs> CellStateChanged;
        public event EventHandler<CellStateEventArgs> CellStateChanging;

        #endregion

        #region Constructors

        private GameField()
	    {
	        FieldId = Guid.NewGuid();

            //UnityEngine.Debug.Log($"Creating GameField {FieldId}");
        }

	    public GameField(int fieldSize) : this()
	    {
            CreateBlankGameField(fieldSize);
	    }

        #endregion

        #region Properties

        public Guid FieldId { get; }

	    public int FieldSize { get; private set; }

        public List<Ship> Ships { get; } = new List<Ship>();

	    public List<Mine> Mines { get; } = new List<Mine>();

        public bool IsUnderConstruction {get; set;}

        public bool AnyShipsAlive => Ships.Any(s => !s.IsDestroyed);

        public int DestroyedShipsCount => Ships.Count(s => s.IsDestroyed);

        private Dictionary<int, FieldCellBase> Cells { get; } = new Dictionary<int, FieldCellBase>();

        #endregion

        #region Private Methods

        private void CreateBlankGameField(int fieldDimension)
	    {
	        FieldSize = fieldDimension;
            
            for (int y = 0; y < fieldDimension; y++)
            {
                for(int x = 0; x < fieldDimension; x++)
                {
                    var c = new FieldCell(this, new Point(x, y));

                    c.StateChanged += OnCellStateChanged;
                    c.StateChanging += OnCellStateChanging;

                    Cells.Add(y*fieldDimension+x, c);
                }
            }
        }

        private void OnCellStateChanged(object sender, CellStateEventArgs e)
        {
            //UnityEngine.Debug.Log($"GameField {FieldId} cell state is changed from {e.OldState} to {e.NewState}. Invoking event ({(CellStateChanged == null ? "NO SUBSCRIBERS" : "")})");

            var cell = sender as FieldCellBase;

            CellStateChanged?.Invoke(this, new CellStateEventArgs{
                CellLocation = cell.Location,
                OldState = e.OldState,
                NewState = e.NewState
            });
        }

         private void OnCellStateChanging(object sender, CellStateEventArgs e)
        {
            //UnityEngine.Debug.Log($"GameField {FieldId} cell state is changing from {e.OldState} to {e.NewState}. Invoking event ({(CellStateChanging == null ? "NO SUBSCRIBERS" : "")})");

            var cell = sender as FieldCellBase;

            CellStateChanging?.Invoke(this, new CellStateEventArgs{
                CellLocation = cell.Location,
                OldState = e.OldState,
                NewState = e.NewState
            });
        }

        #endregion

        #region Public Methods

        public FieldCellBase CellAtPoint(Point p){
            if (p.X < 0 || p.Y < 0 || p.X >= FieldSize || p.Y >= FieldSize)
                return null;

            if (Cells.TryGetValue(p.Y * FieldSize + p.X, out var cell))
                return cell;
            
            return null;
        }

        public FieldCellBase CellAtIndex(int index){
            if (index > Cells.Count - 1 || index < 0)
                return null;

            return Cells[index];
        }

        public Ship ShipAtPoint(Point p){
            return Ships.FirstOrDefault(x => x.HasCell(p));
        }

        public void Iterate(Action<FieldCellBase> visitorAtion){
            foreach(var c in Cells){
                visitorAtion(c.Value);
            }
        }

        public void AddShip(Ship ship)
        {
            foreach(var shipCell in ship.Cells)
            {
                var fieldCell = CellAtPoint(shipCell.Location);

                fieldCell.State = FieldCellBase.CellStates.Ship;
            }

            Ships.Add(ship);
        }

	    public void AddMine(Mine mine)
	    {
	        var fieldCell = CellAtPoint(mine.Location);

            fieldCell.State = FieldCellBase.CellStates.Mine;

            Mines.Add(mine);
	    }

        public HitResult ProcessHit(Point location)
        {
            //UnityEngine.Debug.Log($"GameField {FieldId} is processing hit: [{location.X};{location.Y}]");

            var cell = CellAtPoint(location);
            var missedResult = new HitResult
            {
                ResultKind = HitResult.HitResults.Missed,
                FieldCell = cell,
                HitLocation = location,
                ShipDamaged = null
            };

            if (cell == null)
            {
                Debug.Log($"CANNOT FIND CELL AT [{location.X};{location.Y}]");

                return missedResult;
            }

            HitResult result = null;

            // Look for ships hit
            foreach (var ship in Ships)
            {
                if (ship.ProcessHit(location))
                {
                    if (ship.IsDestroyed)
                        result = new HitResult
                        {
                            ResultKind = HitResult.HitResults.ShipDestroyed,
                            FieldCell = cell,
                            HitLocation = location,
                            ShipDamaged = ship
                        };
                    else
                        result = new HitResult
                        {
                            ResultKind = HitResult.HitResults.ShipDamaged,
                            FieldCell = cell,
                            HitLocation = location,
                            ShipDamaged = ship
                        };
                }
            }

            // No ships damaged. Let's look for mines and other stuff.
            if (result == null)
            {
                // 'Miss' case
                if (cell.IsEmpty)
                {
                    //UnityEngine.Debug.Log($"GameField {FieldId} is changing cell state to Miss");
                    cell.State = FieldCellBase.CellStates.Miss;

                    result = new HitResult
                    {
                        ResultKind = HitResult.HitResults.Missed,
                        FieldCell = cell,
                        HitLocation = location,
                        ShipDamaged = null
                    };
                }

                // 'Mine' case
                if (cell.IsMined)
                {
                    //UnityEngine.Debug.Log($"GameField {FieldId} is changing cell state to MineExploded");
                    cell.State = FieldCellBase.CellStates.MineExploded;

                    result = new HitResult
                    {
                        ResultKind = HitResult.HitResults.MineActivated,
                        FieldCell = cell,
                        HitLocation = location,
                        ShipDamaged = null
                    };
                }

                // No other cases yet
            }

            if (cell.State == FieldCellBase.CellStates.Damaged)
                return new HitResult
                {
                    ResultKind = HitResult.HitResults.ShipDamaged,
                    FieldCell = cell,
                    HitLocation = location,
                    ShipDamaged = ShipAtPoint(location)
                };

            //UnityEngine.Debug.Log($"Result for {FieldId} is: {result.ResultKind}");

            //if (result == null)
           // {
                //Debug.Log("NULL hit result");
                //Debug.Log($"Location asked: [{location.X};{location.Y}]");
           // }

            return result ?? missedResult;
        }

        public List<CellStateInfo> SpyAtPoint(Point p)
        {
            var result = new List<CellStateInfo>();

            var n = new Point(p.X, p.Y);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X, p.Y - 1);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X, p.Y + 1);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X - 1, p.Y);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X + 1, p.Y);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X - 1, p.Y - 1);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X + 1, p.Y - 1);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X - 1, p.Y + 1);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            n = new Point(p.X + 1, p.Y + 1);
            if (n.X >= 0 && n.Y >= 0 && n.X < FieldSize && n.Y < FieldSize)
            {
                result.Add(new CellStateInfo
                {
                    Location = n,
                    State = CellAtPoint(n).State
                });
            }

            return result;
        }

        #endregion

        #region Static Methods

        public static GameField FromContract(int fieldSize, GameFieldContract contract)
        {
            var res = new GameField(fieldSize);

            res.IsUnderConstruction = true;

            foreach (var ship in contract.ships)
            {
                res.AddShip(Ship.FromContract(res, ship));
            }

            foreach (var mine in contract.mines)
            {

                res.AddMine(Mine.FromContract(res, mine));
            }

            res.IsUnderConstruction = false;

            return res;
        }

        #endregion 

        #region Disposing

        public void Clear(){
            Ships.Clear();
            Mines.Clear();

            foreach(var cell in Cells)
            {
                cell.Value.State = FieldCellBase.CellStates.Empty;
            }
        }

        #endregion

    }
}
