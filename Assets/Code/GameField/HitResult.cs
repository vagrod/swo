using SeaWarsOnline.Core.GameObjects;

namespace SeaWarsOnline.Core
{
    public class HitResult
    {

        #region Enums

        public enum HitResults
        {
            Missed,
            ShipDamaged,
            ShipDestroyed,
            MineActivated
        }

        #endregion

        #region Properties

        public FieldCellBase FieldCell { get; set; }

        public Ship ShipDamaged { get; set; }

        public HitResults ResultKind { get; set; }

        public Point HitLocation { get; set; }

        public bool IsSuccessfulHit => ResultKind == HitResults.ShipDamaged || ResultKind == HitResults.ShipDestroyed;

        public bool IsNotProcessed { get; set; }

        #endregion

    }
}
