using System;
using System.Collections.Generic;
using SeaWarsOnline.Core;

[Serializable]
public class GameFieldContract
{

    public List<GameShipContract> ships;
    public List<GameMineContract> mines;

    public static GameFieldContract FromGameField(GameField gameField)
    {
        var res = new GameFieldContract
        {
            ships = new List<GameShipContract>(),
            mines = new List<GameMineContract>()
        };

        foreach (var ship in gameField.Ships)
        {
            var cells = ship.Cells.ConvertAll(x => new GameCellContract
            {
                x = x.Location.X, 
                y = x.Location.Y, 
                type = (int) x.State
            });

            res.ships.Add(new GameShipContract
            {
                shipId = ship.ShipId.ToString(),
                cells = cells
            });
        }

        foreach (var mine in gameField.Mines)
        {
            res.mines.Add(new GameMineContract
            {
                mineId = mine.MineId.ToString(),
                cell = new GameCellContract
                {
                    x = mine.Location.X,
                    y = mine.Location.Y,
                    type = (int)mine.Cell.State
                }
            });
        }

        return res;
    }
}

[Serializable]
public class GameShipContract
{
    public string shipId;
    public List<GameCellContract> cells;
}

[Serializable]
public class GameMineContract
{
    public string mineId;
    public GameCellContract cell;
}

[Serializable]
public class GameCellContract
{

    public int x;
    public int y;
    public int type;

}
