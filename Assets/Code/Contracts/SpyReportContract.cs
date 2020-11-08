using System;
using System.Collections.Generic;

namespace Assets.Code.Contracts
{
    [Serializable]
    public class SpyReportContract
    {

        public string sender;
        public List<GameCellContract> cells;

    }
}
