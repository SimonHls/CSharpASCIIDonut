namespace Main.Structs;
public struct Point3d {
    public Point3d(double x, double y, double z, double l) {
        X = x;
        Y = y;
        Z = z;
        L = l;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public double L { get; }

    public override string ToString() => $"({X}, {Y}, {Z}, luminance: {L})";
}