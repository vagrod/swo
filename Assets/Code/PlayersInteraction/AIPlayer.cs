using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SeaWarsOnline.Core.GameObjects;
using UnityEngine;
using Random = System.Random;

namespace SeaWarsOnline.Core.PlayersInteraction
{
    public class AIPlayer : PlayerBase
    {

        #region Fields

        private readonly List<Point> _locationsUsed = new List<Point>();
        private readonly Random _random;
        private readonly List<DesignTimeShip> _shipsDiscovered = new List<DesignTimeShip>();

        private DesignTimeShip _currentShip;

        #endregion

        #region Private Properties

        private GameRules Rules { get; }
        
        private System.Threading.Timer HitBackDelayTimer { get; set; }

        private GameField OpponentField { get; } 

        #endregion

        #region Constructors

        public AIPlayer(GameField gameField, GameRules gameOptions) : base(gameField)
        {
            Rules = gameOptions;
            OpponentField = new GameField(gameOptions.GameFieldSize);

            OpponentField.IsUnderConstruction = true; // do need any events from this field

            _random = new Random(DateTime.Now.Millisecond);
        }

        #endregion

        #region Methods

        public override Task<bool> ReportMineHit(Point p){
            _locationsUsed.Add(p);

            return Task.FromResult(true);
        }

        public override Task<List<CellStateInfo>> SpyAtPoint(Point p)
        {
            var result = GameField.SpyAtPoint(p);

            DoDelayedHitBack();

            return Task.FromResult(result);
        }

        public override Task<bool> ReportHitBackResult(HitResult result)
        {
            //UnityEngine.Debug.Log($"Hit back result reported to AI: {result.ResultKind.ToString()}");

            if (result.ResultKind == HitResult.HitResults.ShipDestroyed || (result.ResultKind == HitResult.HitResults.ShipDamaged && result.ShipDamaged.IsDestroyed))
            {
                //UnityEngine.Debug.Log("AI destroyed the ship");

                var destroyedShip = DiscoverNewShipAtPoint(result.HitLocation);

                if (destroyedShip != null)
                {
                    MaskOutShipBuffer(destroyedShip);
                }
                else
                {
                    // 1x1 situations
                    MaskOutShipBuffer(result.ShipDamaged);
                }

                if (_shipsDiscovered.Contains(destroyedShip))
                {
                    //UnityEngine.Debug.Log($"Dead ship removed");
                    _shipsDiscovered.Remove(destroyedShip);
                }

                if (destroyedShip == _currentShip)
                    _currentShip = null;

                if(!Rules.ChangeTurnEachTime && result.IsSuccessfulHit)
                    DoDelayedHitBack();

                return Task.FromResult(true);
            }

            if (result.ResultKind == HitResult.HitResults.ShipDamaged){
                //UnityEngine.Debug.Log($"AI damaged ship. Discovering at point [{result.HitLocation.X};{result.HitLocation.Y}]");
                DiscoverNewShipAtPoint(result.HitLocation);
            }

            if (result.ResultKind == HitResult.HitResults.Missed){
                OpponentField.CellAtPoint(result.HitLocation).State = FieldCellBase.CellStates.Miss;
            }

            if(!Rules.ChangeTurnEachTime && result.IsSuccessfulHit)
                DoDelayedHitBack();

            return Task.FromResult(true);
        }

        private void MaskOutShipBuffer(Ship ship){
            //if (ship == null)
             //   Debug.Log("MaskOutShipBuffer NULL ship");

            var buffer = ship.GetBufferPoints();

            //UnityEngine.Debug.Log($"Masking out ship buffer of ship size {ship.Cells.Count}");

            foreach(var point in buffer){
                //UnityEngine.Debug.Log($"Masked out [{point.X};{point.Y}]");
                _locationsUsed.Add(point);
                OpponentField.CellAtPoint(point).State = FieldCellBase.CellStates.Miss;
            }
        }

        public override Task<HitResult> Hit(Point cellLocation, bool isByMine)
        {
            var hit = GameField.ProcessHit(cellLocation);
            var hitResult = Task.FromResult(hit);
            
            if (hit.ShipDamaged != null && hit.ShipDamaged.IsDestroyed){
                var buffer = hit.ShipDamaged.GetBufferPoints();

                foreach(var point in buffer){
                    GameField.CellAtPoint(point).State = FieldCellBase.CellStates.Miss;
                }
            }

            if (!isByMine && OpponentField.DestroyedShipsCount < GameField.Ships.Count && GameField.AnyShipsAlive) // If not game over
            {
                if (Rules.ChangeTurnEachTime || (!Rules.ChangeTurnEachTime && !hit.IsSuccessfulHit))
                    DoDelayedHitBack();
            }

            return hitResult;
        }

        private void DoDelayedHitBack(){
            var delay = _random.Next(1000, 2000);

            //UnityEngine.Debug.Log($"Waiting for {delay / 1000f}s before hit back");

            if (HitBackDelayTimer != null)
                HitBackDelayTimer.Change(delay, System.Threading.Timeout.Infinite);
            else 
                HitBackDelayTimer = new System.Threading.Timer(HitBackDelayOnTimer, null, delay, System.Threading.Timeout.Infinite);
        }

        private void HitBackDelayOnTimer(object state){
            var e = new HitBackEventArgs { CellLocation = GetNextHitPoint() };

            //UnityEngine.Debug.Log($"Hit back: chosen point is [{e.CellLocation.X};{e.CellLocation.Y}]");

            InvokeHitBack(e);
        }

        private DesignTimeShip DiscoverNewShipAtPoint(Point location){
            var existingShip = _shipsDiscovered.FirstOrDefault(x => x.GetBufferPoints().Any(l => l == location));

            if(existingShip != null){
                //UnityEngine.Debug.Log($"Ship already discovered. Adding new cell.");

                existingShip.AddCell(location, Rules.StraightShips);
                return existingShip;
            }

            var ship = new DesignTimeShip(OpponentField, 4); // we don't know ship actual size, so assume it's the biggest one
            
            ship.AddCell(location, Rules.StraightShips);

            _shipsDiscovered.Add(ship); 
            OpponentField.AddShip(ship);

            return ship;
        }

        #endregion

        #region AI Logic

        private Point GetNextHitPoint()
        {
            //UnityEngine.Debug.Log($"AI Get Next Hit Point. Current ship? {(_currentShip == null ? "no" : "yes")}. Any discovered ships? {(_shipsDiscovered.Any() ? "yes" : "no")}");

            if (_shipsDiscovered.Count == 0 && _currentShip == null){
               return GetNextValidRandomPoint();
            } 

            if (_currentShip == null && _shipsDiscovered.Any()){
                //UnityEngine.Debug.Log($"AI is choosing new current ship");

                _currentShip = _shipsDiscovered.First();
            }

            if (_currentShip != null){
                // Trying to use available locations
                var available = _currentShip.AvailableNewCellLocations.Where(x => _locationsUsed.All(l => l != x)).ToList();
                var count = available.Count();

                if (count > 0)
                {
                    var index = _random.Next(0, count - 1);
                    var loc = available.ElementAt(index);

                    //PrintUsedLocations();

                    //UnityEngine.Debug.Log($"Using AvailableLocations: [{loc.X};{loc.Y}]");
                    _locationsUsed.Add(loc);

                    return loc;
                }

                var p = GetNextShipPoint(_currentShip);

                if (p == null){
                    /*
                    UnityEngine.Debug.Log($"Found trash ship. Switching to another.");
                    var bp = _currentShip.GetBufferPoints(GameOptions.StraightShips);
                    foreach(var pp in bp){
                        UnityEngine.Debug.Log($"Buffer point [{pp.X};{pp.Y}]: was used? {(_locationsUsed.Any(x => x == pp) ? "yes" : "no")}");
                    }

                    foreach(var pp in _locationsUsed){
                        UnityEngine.Debug.Log($"Used location [{pp.X};{pp.Y}]");
                    }
                    */

                    if (_shipsDiscovered.Contains(_currentShip))
                        _shipsDiscovered.Remove(_currentShip);  

                    if (_shipsDiscovered.Count == 0)
                    {
                        _currentShip = null;
                        //UnityEngine.Debug.Log($"No more ships to go.");
                        return GetNextValidRandomPoint();
                    }

                    _currentShip = _shipsDiscovered.First();
                    p = GetNextShipPoint(_currentShip);

                    while(p == null){
                        if (_shipsDiscovered.Contains(_currentShip))
                            _shipsDiscovered.Remove(_currentShip);

                        if (_shipsDiscovered.Count == 0)
                        {
                            //UnityEngine.Debug.Log($"No more ships to go.");
                            return GetNextValidRandomPoint();
                        }

                        _currentShip = _shipsDiscovered.First();
                        p = GetNextShipPoint(_currentShip);
                    }                        

                    //UnityEngine.Debug.Log($"No more ships to go.");

                    return GetNextValidRandomPoint();
                }

                //PrintUsedLocations();

                //UnityEngine.Debug.Log($"AI is hitting known ship at point [{p.X};{p.Y}]");
                _locationsUsed.Add(p);

                return p;
                
            }

            //PrintUsedLocations();

            return GetNextValidRandomPoint();
        }

        private void PrintUsedLocations()
        {
            foreach (var pp in _locationsUsed)
            {
                UnityEngine.Debug.Log($"Used location [{pp.X};{pp.Y}]");
            }
        }

        private Point GetNextShipPoint(Ship ship){
            var buffer = ship.GetBufferPoints().Where(x => _locationsUsed.All(l => l != x)).ToList();
            var count = buffer.Count();

            if (count == 0)
                return null;

            var index = _random.Next(0, count - 1);
            var p = buffer.ElementAt(index);

            return p;
        }

        private Point GetNextValidRandomPoint(){
            var p = GetNextRandomPoint();

            if (_locationsUsed.Any(l => l==p))
                p = null;

            if (p == null){
                // No luck there. Will choose in loop
                var direction = _random.Next(0, 100) > 50;

                if (direction)
                {
                    for (var x = 0; x <= Rules.GameFieldSize - 1; x++)
                    {
                        for (var y = 0; y <= Rules.GameFieldSize - 1; y++)
                        {
                            var op = new Point(x, y);
                            if (_locationsUsed.Any(o => o == op))
                                continue;

                            //UnityEngine.Debug.Log($"AI returning loop random point [{op.X};{op.Y}] asc");
                            _anchorPoint = op;
                            _locationsUsed.Add(op);

                            return op;
                        }
                    }
                } else {
                    for (var x = Rules.GameFieldSize - 1; x >= 0 ; x--)
                    {
                        for (var y = Rules.GameFieldSize - 1; y >= 0; y--)
                        {
                            var op = new Point(x, y);
                            if (_locationsUsed.Any(o => o == op))
                                continue;

                            //UnityEngine.Debug.Log($"AI returning loop random point [{op.X};{op.Y}] desc");

                            _anchorPoint = op;
                            _locationsUsed.Add(op);

                            return op;
                        }
                    }
                }

                UnityEngine.Debug.Log("FIELD HAS NO UNUSED CELLS");
                return new Point(0,0);
            }

            _locationsUsed.Add(p);

            //UnityEngine.Debug.Log($"AI returning random point [{p.X};{p.Y}]");

            return p;
        }

        private Point _anchorPoint;

        private Point GetNextRandomPoint(){
            // Probe location near last "anchor" point if any

            var r = GetNearFreePoint(_anchorPoint);

            if (_anchorPoint == null)
                _anchorPoint = r;

            return r;
        }

        private Point GetNearFreePoint(Point p){
            var probe = ProbeNearPoint(p);

            if (probe == null)
            {
                Point CheckPoint(Point pp)
                {
                    var chainBreakProbability = _random.Next(0, 100);
                    if (chainBreakProbability > 12 && chainBreakProbability < 37 && IsValidPoint(pp))
                    {
                        //Debug.Log($"Anchor randomly changed to [{pp.X};{pp.Y}]");
                        _anchorPoint = pp;
                    }

                    if (IsValidPoint(pp) && OpponentField.CellAtPoint(pp).State == FieldCellBase.CellStates.Empty)
                    {
                        _anchorPoint = pp;

                        return pp;
                    }

                    probe = ProbeNearPoint(pp);
                    if (probe != null && OpponentField.CellAtPoint(probe).State == FieldCellBase.CellStates.Empty)
                    {
                        _anchorPoint = probe;

                        return probe;
                    }

                    return null;
                };

                var pattern = GetRandomPattern();

                var t = p + pattern[0];
                var pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                t = p + pattern[1];
                pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                t = p + pattern[2];
                pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                t = p + pattern[3];
                pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                t = p + pattern[4];
                pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                t = p + pattern[5];
                pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                t = p + pattern[6];
                pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                t = p + pattern[7];
                pCheck = CheckPoint(t);
                if (pCheck != null)
                    return pCheck;

                var r = GetRandomCoords();

                _anchorPoint = r;

                return r;
            } 
            
            return probe;
        }

        private bool IsValidPoint(Point p)
        {
            if (p.X < 0 || p.Y < 0 || p.X >= Rules.GameFieldSize || p.Y >= Rules.GameFieldSize)
                return false;

            return true;
        }

        private Point ProbeNearPoint(Point p){
            if (p == null)
                return GetRandomCoords();

            if (!IsValidPoint(p))
                return null;

            var pattern = GetRandomPattern();

            Point CheckPoint(Point pp)
            {
                var chainBreakProbability = _random.Next(0, 100);
                if (chainBreakProbability > 32 && chainBreakProbability < 57 && IsValidPoint(pp))
                {
                    //Debug.Log($"Anchor randomly changed to [{pp.X};{pp.Y}]");
                    _anchorPoint = pp;
                }

                var probe = OpponentField.CellAtPoint(pp);
                if (probe != null && probe.State == FieldCellBase.CellStates.Empty)
                    return probe.Location;

                return null;
            }

            var checkPoint = CheckPoint(p + pattern[0]);
            if(checkPoint != null)
                return checkPoint;

            checkPoint = CheckPoint(p + pattern[1]);
            if (checkPoint != null)
                return checkPoint;

            checkPoint = CheckPoint(p + pattern[2]);
            if (checkPoint != null)
                return checkPoint;

            checkPoint = CheckPoint(p + pattern[3]);
            if (checkPoint != null)
                return checkPoint;

            checkPoint = CheckPoint(p + pattern[4]);
            if (checkPoint != null)
                return checkPoint;

            checkPoint = CheckPoint(p + pattern[5]);
            if (checkPoint != null)
                return checkPoint;

            checkPoint = CheckPoint(p + pattern[6]);
            if (checkPoint != null)
                return checkPoint;

            checkPoint = CheckPoint(p + pattern[7]);
            if (checkPoint != null)
                return checkPoint;

            return null;
        }

        private List<Point> GetRandomPattern()
        {
            var r = _random.Next(0, 7);

            switch (r)
            {
                case 0:
                    return new List<Point>(new []
                    {
                        new Point(-1, 0),
                        new Point(-1, -1),
                        new Point(-1, 1),
                        new Point(1, 0),
                        new Point(0, -1),
                        new Point(0, 1),
                        new Point(1, 1),
                        new Point(1, -1),
                    });

                case 1:
                    return new List<Point>(new[]
                    {
                        new Point(1, 0),
                        new Point(1, -1),
                        new Point(-1, 0),
                        new Point(-1, -1),
                        new Point(-1, 1),
                        new Point(0, -1),
                        new Point(1, 1),
                        new Point(0, 1),
                    });

                case 2:
                    return new List<Point>(new[]
                    {
                        new Point(0, 1),
                        new Point(-1, -1),
                        new Point(-1, 0),
                        new Point(-1, 1),
                        new Point(0, -1),
                        new Point(1, 1),
                        new Point(1, 0),
                        new Point(1, -1),
                    });

                case 3:
                    return new List<Point>(new[]
                    {
                        new Point(0, -1),
                        new Point(-1, 0),
                        new Point(1, 1),
                        new Point(-1, 1),
                        new Point(0, 1),
                        new Point(1, 0),
                        new Point(1, -1),
                        new Point(-1, -1),
                    });

                case 4:
                    return new List<Point>(new[]
                    {
                        new Point(0, 1),
                        new Point(-1, 0),
                        new Point(-1, 1),
                        new Point(0, -1),
                        new Point(-1, -1),
                        new Point(1, 1),
                        new Point(1, 0),
                        new Point(1, -1),
                    });

                case 5:
                    return new List<Point>(new[]
                    {
                        new Point(1, -1),
                        new Point(0, -1),
                        new Point(-1, 0),
                        new Point(0, 1),
                        new Point(1, 1),
                        new Point(1, 0),
                        new Point(-1, -1),
                        new Point(-1, 1),
                        
                    });

                case 6:
                    return new List<Point>(new[]
                    {
                        new Point(-1, -1),
                        new Point(-1, 1),
                        new Point(-1, 0),
                        new Point(1, 0),
                        new Point(1, -1),
                        new Point(0, -1),
                        new Point(1, 1),
                        new Point(0, 1),
                    });

                default:
                    return new List<Point>(new[]
                    {
                        new Point(0, 1),
                        new Point(1, 1),
                        new Point(-1, -1),
                        new Point(-1, 1),
                        new Point(-1, 0),
                        new Point(0, -1),
                        new Point(1, -1),
                        new Point(1, 0),
                    });
            }
        }

        private Point GetRandomCoords(){
            var pX = _random.Next(0, Rules.GameFieldSize - 1);
            var pY = _random.Next(0, Rules.GameFieldSize - 1);

            return new Point(pX, pY);
        }

        #endregion
    }
}


