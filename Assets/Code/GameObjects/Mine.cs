using System;

namespace SeaWarsOnline.Core.GameObjects
{
    public class Mine
    {

        #region Events

        public event EventHandler MineBlown;

        #endregion 

        #region Constructors

        public Mine(GameField gameField)
        {
            Field = gameField;
            MineId = Guid.NewGuid();
        }

        #endregion

        #region Properties

        public Guid MineId { get; private set; }

        public Point Location { get; protected set; }

        public FieldCellBase Cell { get; protected set; }

        public GameField Field { get; }

        public bool IsBlown => Cell?.State == FieldCellBase.CellStates.MineExploded;

        #endregion

        #region Public Methods

        public void AddCell(Point location)
        {
            Location = location;

            var cell = Field.CellAtPoint(location);

            Cell = cell;

            cell.State = FieldCellBase.CellStates.Mine;
            
            SubscribeCell();
        }

        #endregion 

        #region Protected Methods

        protected void SubscribeCell(){
            if (Cell == null)
                return;

            Cell.StateChanged += CellOnStateChanged;
        }

        protected void UnsubscribeCell(){
            if (Cell == null)
                return;

            Cell.StateChanged -= CellOnStateChanged;
        }

        #endregion

        #region Private Methods

        private void CellOnStateChanged(object sender, CellStateEventArgs e)
        {
            if (e.NewState == FieldCellBase.CellStates.MineExploded){
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    if (IsBlown)
                        MineBlown?.Invoke(this, new EventArgs());
                    });
            }
        }

        #endregion

        #region Static Methods

        public static Mine FromContract(GameField field, GameMineContract mineData)
        {
            var fieldMine = new Mine(field);

            fieldMine.MineId = Guid.Parse(mineData.mineId);

            fieldMine.AddCell(new Point(mineData.cell.x, mineData.cell.y));

            return fieldMine;
        }

        #endregion 

    }
}
