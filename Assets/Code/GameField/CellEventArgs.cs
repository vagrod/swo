namespace SeaWarsOnline.Core
{
    public class CellEventArgs
    {

        #region Properties

        public Point CellLocation { get; set; }

        #endregion

    }

    public class CellStateEventArgs
    {

        #region Properties

        public Point CellLocation { get; set; }
        public FieldCellBase.CellStates OldState{get; set;}
        public FieldCellBase.CellStates NewState{get; set;}

        #endregion

    }
}
