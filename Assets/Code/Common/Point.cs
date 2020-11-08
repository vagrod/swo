namespace SeaWarsOnline.Core{

    public class Point {

        public Point(){

        }

        public Point(int x, int y){
            X = x;
            Y = y;
        }

        public int X {get; set;}
        public int Y {get; set;}

        public static bool operator == (Point a, Point b){
            return a?.X == b?.X && a?.Y == b?.Y;
        }

        public static bool operator != (Point a, Point b){
            return a?.X != b?.X || a?.Y != b?.Y;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            
            return ((Point)obj).X == this.X && ((Point)obj).Y == this.Y;
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }

}