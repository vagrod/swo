using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeaWarsOnline.Core
{

    public abstract class PlayerBase
    {

        #region Events

        public event EventHandler<HitBackEventArgs> HitBack;

        #endregion

        #region Private Properties

        public GameField GameField { get; protected set; }

        #endregion

        #region Constructor

        protected PlayerBase(GameField gameField)
        {
            GameField = gameField;
        }

        #endregion

        #region Virtual Methods

        public virtual Task<HitResult> Hit(Point cellLocation, bool isByMine)
        {
            return Task.FromResult(new HitResult());
        }

        public virtual Task<bool> ReportHitBackResult(HitResult result)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> ReportMineHit(Point p)
        {
            return Task.FromResult(true);
        }

        public virtual Task<List<CellStateInfo>> SpyAtPoint(Point p){
            return Task.FromResult(new List<CellStateInfo>());
        }

        #endregion

        #region Public Methods

        public void InvokeHitBack(HitBackEventArgs e)
        {
            HitBack?.Invoke(this, e);
        }

        public virtual void Reset(){
            GameField.Clear();
            GameField = null;
        }

        #endregion 

    }
}
