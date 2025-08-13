using Main.Structs;
using System.Text;

namespace ConsoleProjection;

internal class Screen {
    // Screen properties
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    // Projection properties
    private readonly double _k1; // FOV
    private readonly double _k2; // Distance of object to camera

    private readonly Dictionary<int, char> asciiPixels = new Dictionary<int, char> {
        {0, '`' },
        {1, ',' },
        {2, '-' },
        {3, '~' },
        {4, ':' },
        {5, ';' },
        {6, '=' },
        {7, '!' },
        {8, '*' },
        {9, '#' },
        {10, '$' },
        {11, '@' },
    };


    public Screen(int screenWidth, int screenHeight, double fov, double distance) {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _k1 = fov;
        _k2 = distance;
    }

    public void DrawPoints(List<Point3d> points) {

        var stringLength = _screenHeight * _screenWidth;
        var screenBuffer = new StringBuilder(new string(' ', stringLength));

        var visiblePoints = new Dictionary<(int, int), ProjectedPoint>();

        foreach (Point3d point in points) {
            var projectedPoint = ProjectTo3d(point);
            (int keyX, int keyY) = (projectedPoint.Px, projectedPoint.Py);

            // If a point already exists at this screen coordinate, replace it only if the new point is closer (higher Z)
            if (visiblePoints.TryGetValue((keyX, keyY), out ProjectedPoint foundPoint)) {
                if (foundPoint.Z > projectedPoint.Z)
                    visiblePoints[(keyX, keyY)] = projectedPoint;
            } else {
                visiblePoints.Add((keyX, keyY), projectedPoint);
            }
        }

        // Draw all visible points into the string buffer
        foreach (KeyValuePair<(int, int), ProjectedPoint> pair in visiblePoints) {
            ProjectedPoint point = pair.Value;

            // Convert projected coordinates to screen coordinates (origin top-left)
            int[] screenCoords = NormalizeToScreenCoordinates(point.Px, point.Py);
            int drawX = screenCoords[0];
            int drawY = screenCoords[1];


            if (drawX >= 0 && drawX < _screenWidth && drawY >= 0 && drawY < _screenHeight) {


                double normalizedL = (point.L + 1.0) / 2.0; // Normalize luminance from [-1, 1] to [0, 1]
                int maxIndex = asciiPixels.Count - 1;
                int charIndex = (int)(normalizedL * maxIndex);
                charIndex = Math.Clamp(charIndex, 0, maxIndex); // Ensure index is valid
                char pixelChar = asciiPixels[charIndex];

                // Calculate the index for the string buffer
                int index = drawY * _screenWidth + drawX;

                screenBuffer[index] = pixelChar;
            }
        }

        Console.SetCursorPosition(0, 0);
        Console.Write(screenBuffer.ToString());
    }

    public int[] NormalizeToScreenCoordinates(int x, int y) {
        return [_screenWidth / 2 + x, _screenHeight / 2 - y / 2];
    }

    private ProjectedPoint ProjectTo3d(Point3d point) {
        var projectedX = _k1 * point.X / (_k2 + point.Z);
        if (projectedX > 0)
            projectedX = Math.Floor(projectedX);
        else
            projectedX = Math.Ceiling(projectedX);

        var projectedY = _k1 * point.Y / (_k2 + point.Z);
        if (projectedY > 0)
            projectedY = Math.Floor(projectedY);
        else
            projectedY = Math.Ceiling(projectedY);


        return new ProjectedPoint((int)projectedX, (int)projectedY, point.Z, point.L);
    }
}

