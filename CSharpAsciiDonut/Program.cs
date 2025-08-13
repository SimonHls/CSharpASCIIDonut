using ConsoleProjection;
using System.Globalization;

CultureInfo usCulture = new CultureInfo("en-US");
Thread.CurrentThread.CurrentCulture = usCulture;
Thread.CurrentThread.CurrentUICulture = usCulture;

int framerate = 24; // frames per second

Console.WriteLine("Adjust console size, then hit any key to continue");
Console.Read();
Console.Clear();

int screenHeight = Console.WindowHeight;
int screenWidth = Console.WindowWidth;

int frameDelayMs = 1000 / framerate;

var screen = new Screen(screenWidth, screenHeight, 70, 5);

var donut = new Donut(1, 150, 2, 150);

for (double i = 0; i < 20; i += 0.1) {
    var donutPoints = donut.BuildDonut(i, i * 0.2);
    screen.DrawPoints(donutPoints);
    Thread.Sleep(frameDelayMs);
}

Console.ReadLine();
Console.ReadLine();