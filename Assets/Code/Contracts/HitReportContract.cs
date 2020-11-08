using System;

namespace Assets.Code.PlayersInteraction
{
    [Serializable]
    public class HitReportContract
    {

        public string sender;
        public string initiator;
        public int resultKind;
        public string damagedShipId;
        public GameCellContract cell;

    }
}
