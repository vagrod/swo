using SeaWarsOnline.Core.GameObjects;

namespace SeaWarsOnline.Core{

    public static class Utils {

        public static bool HasObstacleNearPoint(this Point p, GameField field){
            return HasObstacleNearPoint(p, field, null);
        }

        public static bool HasObstacleNearPoint(this Point p, GameField field, Ship ship){
            var cell = field.CellAtPoint(new Point(p.X, p.Y-1));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined && (ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }

            cell = field.CellAtPoint(new Point(p.X, p.Y+1));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined &&(ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }
        
            cell = field.CellAtPoint(new Point(p.X-1, p.Y));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined &&(ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }

            cell = field.CellAtPoint(new Point(p.X+1, p.Y));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined &&(ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }

            cell = field.CellAtPoint(new Point(p.X-1, p.Y+1));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined &&(ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }

            cell = field.CellAtPoint(new Point(p.X+1, p.Y+1));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined &&(ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }

            cell = field.CellAtPoint(new Point(p.X-1, p.Y-1));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined &&(ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }

            cell = field.CellAtPoint(new Point(p.X+1, p.Y-1));
            
            if (cell != null){
                if (!cell.IsEmpty && !cell.IsMissed && !cell.IsMined &&(ship==null|| !ship.HasCell(cell.Location)))
                    return true;
            }

            return false;
        }
        
    }

}