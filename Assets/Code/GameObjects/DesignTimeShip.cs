using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaWarsOnline.Core.GameObjects
{
    public class DesignTimeShip : Ship
    {

        public DesignTimeShip(GameField field, int size):base(field, size){ }

        public List<Point> AvailableNewCellLocations{get;} = new List<Point>();

        public bool IsIncomplete => Cells.Count < Size;

        public void AddCell(Point location, bool isStraightShip)
        {
            Cells.Add(Field.CellAtPoint(location));
            Field.CellAtPoint(location).State = FieldCellBase.CellStates.Ship;
            RecalculateAvailableNewCellLocation(location, isStraightShip);
        }

        public void RemoveCell(Point location, bool isStraightShip)
        {
            if (HasCell(location)){
                Cells.Remove(Cells.First(x => x.Location == location));
                Field.CellAtPoint(location).State = FieldCellBase.CellStates.Empty;
                RecalculateAvailableNewCellLocation(Cells.FirstOrDefault()?.Location, isStraightShip);
            }
        }

        public void RecalculateAvailableNewCellLocation(bool isStraightShip){
            if (Cells.Any())
                RecalculateAvailableNewCellLocation(Cells.First().Location, isStraightShip);
        }

        private void RecalculateAvailableNewCellLocation(Point location, bool isStraightShip){
            AvailableNewCellLocations.Clear();

            if (Cells.Count == 0)
                return;

            if (Cells.Count == Size)
                return;

            if (isStraightShip){
                var cellsAround = new List<Tuple<Point, bool>>();

                var p = new Point(location.X, location.Y-1);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                p = new Point(location.X, location.Y+1);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                p = new Point(location.X-1, location.Y);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                p = new Point(location.X+1, location.Y);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                // Diagonals

                p = new Point(location.X-1, location.Y-1);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                p = new Point(location.X+1, location.Y-1);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                p = new Point(location.X-1, location.Y+1);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                p = new Point(location.X+1, location.Y+1);
                cellsAround.Add(new Tuple<Point, bool>(p, HasCell(p)));

                if (Cells.Count == 1){
                    if (!cellsAround[0].Item2 && cellsAround[0].Item1.Y>=0 && !cellsAround[0].Item1.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(cellsAround[0].Item1);
                    
                    if (!cellsAround[1].Item2 && cellsAround[1].Item1.Y<Field.FieldSize && !cellsAround[1].Item1.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(cellsAround[1].Item1);

                    if (!cellsAround[2].Item2 && cellsAround[2].Item1.X>=0 && !cellsAround[2].Item1.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(cellsAround[2].Item1);

                    if (!cellsAround[3].Item2 && cellsAround[3].Item1.X<Field.FieldSize && !cellsAround[3].Item1.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(cellsAround[3].Item1);

                    return;
                }

                var xMin = Cells.Min(x => x.Location.X);
                var yMin = Cells.Min(x => x.Location.Y);
                var xMax = Cells.Max(x => x.Location.X);
                var yMax = Cells.Max(x => x.Location.Y);

                if (xMin == xMax){
                    // Vertical

                    var p2 = new Point(xMax, yMin-1);
                    if (!HasCell(p2) && p2.Y>=0 && !p2.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(p2);
                    
                    p2 = new Point(xMax, yMax+1);
                    if (!HasCell(p2) && p2.Y<Field.FieldSize && !p2.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(p2);
                } else {
                    // Horizontal

                    var p3 = new Point(xMin-1, yMin);
                    if (!HasCell(p3) && p3.X>=0 && !p3.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(p3);
                    
                    p3 = new Point(xMax+1, yMin);
                    if (!HasCell(p3) && p3.X<Field.FieldSize && !p3.HasObstacleNearPoint(Field, this))
                        AvailableNewCellLocations.Add(p3);
                }
            } else {
                var xMin = Cells.Min(x => x.Location.X) - 1;
                var yMin = Cells.Min(x => x.Location.Y) - 1;
                var xMax = Cells.Max(x => x.Location.X) + 1;
                var yMax = Cells.Max(x => x.Location.Y) + 1;

                for(var x=xMin;x<=xMax;x++){
                    for(var y=yMin;y<=yMax;y++){
                        var p4 = new Point(x,y);

                        if (!HasCell(p4) && !p4.HasObstacleNearPoint(Field, this) && (
                                HasCell(new Point(x-1,y)) ||
                                HasCell(new Point(x+1,y)) ||
                                HasCell(new Point(x,y-1)) ||
                                HasCell(new Point(x,y+1)) // Not diagonal
                            ) && x>=0 && y>=0 && x<Field.FieldSize && y<Field.FieldSize)
                            AvailableNewCellLocations.Add(p4);
                    }
                }
            }
        }
    }
}