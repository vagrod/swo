using System;
using System.Collections;

namespace SeaWarsOnline.Core
{
    public abstract class FieldCellBase
    {

        #region Private Fields

        private CellStates _state = CellStates.Empty;

        #endregion

        #region Events

        public event EventHandler<CellStateEventArgs> StateChanged;
        public event EventHandler<CellStateEventArgs> StateChanging;

        #endregion

         #region Enums

        public enum CellStates
        {
            Empty,
            Mine,
            MineExploded,
            ForeignShipPart,
            Miss,
            Ship,
            Damaged,
            Available,
            ForeignMine
        }

        #endregion

        #region Constructors

        protected FieldCellBase(GameField gameField, Point location)
        {
            Location = location;
            Field = gameField;
        }

        #endregion

        #region Properties

        public Point Location { get; protected set; }

        public GameField Field { get; protected set; }

        public CellStates State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    if (!Field.IsUnderConstruction)
                        UnityMainThreadDispatcher.Instance().Enqueue(
                            OnStateChanging(_state, value)
                        );

                    var oldState = _state;

                    _state = value;

                    if (!Field.IsUnderConstruction)
                        UnityMainThreadDispatcher.Instance().Enqueue(
                            OnStateChanged(oldState, value)
                        );
                }
            }
        }

        public string UsedImageResource { get; set; }

        public bool IsMined => State == CellStates.Mine;
        public bool IsDamaged => State == CellStates.Damaged;
        public bool IsMineExploded => State == CellStates.MineExploded;
        public bool IsForeignShip => State == CellStates.ForeignShipPart;
        public bool IsForeignMine => State == CellStates.ForeignMine;
        public bool IsEmpty => State == CellStates.Empty || State == CellStates.Available;
        public bool IsMissed  => State == CellStates.Miss;
        public bool IsHealthyShip  => State == CellStates.Ship;
        public bool IsDamagedShip  => State == CellStates.Damaged;

        #endregion

        #region Methods

        public IEnumerator OnStateChanged(CellStates oldState, CellStates newState)
        {
            //UnityEngine.Debug.Log("Invoking OnStateChanged in a main thread");
            StateChanged?.Invoke(this, new CellStateEventArgs{
                CellLocation = Location,
                OldState = oldState,
                NewState = newState
            });

             yield return null;
        }

        public IEnumerator OnStateChanging(CellStates oldState, CellStates newState)
        {
            //UnityEngine.Debug.Log("Invoking OnStateChanged in a main thread");
            StateChanging?.Invoke(this, new CellStateEventArgs{
                CellLocation = Location,
                OldState = oldState,
                NewState = newState
            });

             yield return null;
        }
        
        #endregion

    }
}
