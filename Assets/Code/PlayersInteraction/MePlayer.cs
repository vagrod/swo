using System.Threading.Tasks;

namespace SeaWarsOnline.Core.PlayersInteraction
{
    public class MePlayer : PlayerBase
    {

        #region Constructor

        public MePlayer(GameField gameField) : base(gameField) { }

        #endregion

        #region PlayerBase Overrides

        public override Task<HitResult> Hit(Point cellLocation, bool isByMine)
        {
            return Task.FromResult(GameField.ProcessHit(cellLocation));
        }

        #endregion

    }
}
