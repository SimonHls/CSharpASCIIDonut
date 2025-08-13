using Main.Structs;

internal class Donut {

    private readonly double _r1;
    private readonly double _r2;
    private readonly int _circleResolution;
    private readonly int _torusResolution;

    public Donut(double circleRadius, int circleResolution, double torusRadius, int torusResolution) {
        _r1 = circleRadius;
        _r2 = torusRadius;
        _circleResolution = circleResolution;
        _torusResolution = torusResolution;
    }

    public List<Point3d> BuildDonut(double A, double B) {
        // Pre-calculate constants for this iteration
        var cosA = Math.Cos(A);
        var sinA = Math.Sin(A);
        var cosB = Math.Cos(B);
        var sinB = Math.Sin(B);

        var points = new List<Point3d>();

        var nextCircleAngle = 0.0;
        var nextTorusAngle = 0.0;

        for (int i = 0; i < _torusResolution; i++) {
            for (int j = 0; j < _circleResolution; j++) {
                points.Add(GetDonutPoint(nextCircleAngle, nextTorusAngle, cosA, sinA, cosB, sinB));
                nextCircleAngle += 2 * Math.PI / _circleResolution;
            }
            nextTorusAngle += 2 * Math.PI / _torusResolution;
        }

        return points;

    }

    private Point3d GetDonutPoint(double circleAngle, double torusAngle, double cosA, double sinA, double cosB, double sinB) {
        // common expression
        var r2r1angle = _r2 + _r1 * Math.Cos(circleAngle);

        var x = Math.Round(r2r1angle * (cosB * Math.Cos(torusAngle) + sinA * sinB * Math.Sin(torusAngle)) - _r1 * cosA * sinB * Math.Sin(circleAngle), 2);
        var y = Math.Round(r2r1angle * (Math.Cos(torusAngle) * sinB - cosB * sinA * Math.Sin(torusAngle)) + _r1 * cosA * cosB * Math.Sin(circleAngle), 2);
        var z = Math.Round(cosA * r2r1angle * Math.Sin(torusAngle) + _r1 * sinA * Math.Sin(circleAngle), 2);

        var luminance = Math.Round(Math.Cos(torusAngle) * Math.Cos(circleAngle) * sinB
            - cosA * Math.Cos(circleAngle) * Math.Sin(torusAngle)
            - sinA * Math.Sin(circleAngle)
            + cosB * (cosA * Math.Sin(circleAngle)
            - Math.Cos(circleAngle) * sinA * Math.Sin(torusAngle)), 2);

        return new Point3d(x, y, z, luminance);
    }
}