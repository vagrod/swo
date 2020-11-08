namespace SeaWarsOnline.Core.GameObjects
{
    public class DesignTimeMine : Mine{

        public DesignTimeMine(GameField field):base(field){ }

        public bool IsIncomplete => Cell == null;

        #region Public Methods

        public void Clear()
        {   
            if (Cell == null)
                return;

            Cell.State = FieldCellBase.CellStates.Empty;

            UnsubscribeCell();

            Location = null;
            Cell = null;
        }

        #endregion

    }

}