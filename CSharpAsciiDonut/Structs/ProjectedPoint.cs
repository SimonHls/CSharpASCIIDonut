namespace Main.Structs;

public struct ProjectedPoint {
    public ProjectedPoint(int px, int py, double z, double l) {
        Px = px;
        Py = py;
        Z = z;
        L = l;
    }

    public int Px { get; }
    public int Py { get; }
    public double Z { get; }
    public double L { get; }


    public override string ToString() => $"(Projected: {Px}, {Py}; Z-Value: {Z}  Luminance: {L})";
}