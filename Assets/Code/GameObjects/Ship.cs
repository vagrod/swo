using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaWarsOnline.Core.GameObjects
{
    public class Ship : IDisposable
    {

        #region Events

        public event EventHandler Destroyed;

        #endregion

        #region Constructors

        public Ship(GameField gameField, int size)
        {
            Field = gameField;
            ShipId = Guid.NewGuid();
            Size = size;
        }

        #endregion

        #region Properties

        public Guid ShipId { get; private set; }

        public int Size {get;}

        public GameField Field { get; }

        public List<FieldCellBase> Cells { get; } = new List<FieldCellBase>();

        public bool IsDestroyed => Cells.All(x => x.State == FieldCellBase.CellStates.Damaged);

        #endregion

        #region Public Methods

        public bool HasCell(Point p){
          return Cells.Any(x => x.Location == p);
        }

        public bool ProcessHit(Point hitLocation)
        {
            var damagedCell = Cells.SingleOrDefault(cell => cell.Location == hitLocation);

            if (damagedCell == null || damagedCell.IsDamaged)
                return false;

            //UnityEngine.Debug.Log($"Ship is changing cell state to Damaged");
            damagedCell.State = FieldCellBase.CellStates.Damaged;

            return true;
        }

        public List<Point> GetBufferPoints()
        {
            var res = new List<Point>();

            foreach (var cell in Cells){
                var pTop = new Point(cell.Location.X, cell.Location.Y - 1);
                var pBottom = new Point(cell.Location.X, cell.Location.Y + 1);
                var pLeft = new Point(cell.Location.X - 1, cell.Location.Y);
                var pRight = new Point(cell.Location.X + 1, cell.Location.Y);

                var pDiag1 = new Point(cell.Location.X + 1, cell.Location.Y - 1);
                var pDiag2 = new Point(cell.Location.X + 1, cell.Location.Y + 1);
                var pDiag3 = new Point(cell.Location.X - 1, cell.Location.Y - 1);
                var pDiag4 = new Point(cell.Location.X - 1, cell.Location.Y + 1);

                if (!HasCell(pTop) && pTop.X>=0 && pTop.Y>=0 && pTop.X<Field.FieldSize && pTop.Y<Field.FieldSize)
                    res.Add(pTop);

                if (!HasCell(pBottom) && pBottom.X>=0 && pBottom.Y>=0 && pBottom.X<Field.FieldSize && pBottom.Y<Field.FieldSize)
                    res.Add(pBottom);

                if (!HasCell(pLeft) && pLeft.X>=0 && pLeft.Y>=0 && pLeft.X<Field.FieldSize && pLeft.Y<Field.FieldSize)
                    res.Add(pLeft);

                if (!HasCell(pRight) && pRight.X>=0 && pRight.Y>=0 && pRight.X<Field.FieldSize && pRight.Y<Field.FieldSize)
                    res.Add(pRight);

                if (!HasCell(pDiag1) && pDiag1.X>=0 && pDiag1.Y>=0 && pDiag1.X<Field.FieldSize && pDiag1.Y<Field.FieldSize)
                    res.Add(pDiag1);

                if (!HasCell(pDiag2) && pDiag2.X>=0 && pDiag2.Y>=0 && pDiag2.X<Field.FieldSize && pDiag2.Y<Field.FieldSize)
                    res.Add(pDiag2);

                if (!HasCell(pDiag3) && pDiag3.X>=0 && pDiag3.Y>=0 && pDiag3.X<Field.FieldSize && pDiag3.Y<Field.FieldSize)
                    res.Add(pDiag3);

                if (!HasCell(pDiag4) && pDiag4.X>=0 && pDiag4.Y>=0 && pDiag4.X<Field.FieldSize && pDiag4.Y<Field.FieldSize)
                    res.Add(pDiag4);
            }

            return res;
        }

        #endregion

        #region Private Methods

        private void CellOnStateChanged(object sender, CellStateEventArgs eventArgs)
        {
            if (IsDestroyed)
                Destroyed?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Static Methods

        public static Ship FromContract(GameField field, GameShipContract shipData)
        {
            var fieldShip = new Ship(field, shipData.cells.Count);

            fieldShip.ShipId = Guid.Parse(shipData.shipId);

            foreach (var shipCell in shipData.cells)
            {
                fieldShip.Cells.Add(field.CellAtPoint(new Point(shipCell.x, shipCell.y)));
            }

            return fieldShip;
        }

        #endregion 

        #region Disposing

        private void UnsubscribeEvents()
        {
           // foreach (var cell in Cells)
          //  {
          //      cell.StateChanged -= CellOnStateChanged;
          //  }
        }

        public void Dispose()
        {
            UnsubscribeEvents();
        }

        #endregion

    }
}
