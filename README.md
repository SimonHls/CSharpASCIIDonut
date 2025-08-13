# The Donut (In C#)

The ASCII-Donut is a pretty famous little program. I've originally stumbled across it in a YouTube video, and wanted to recreate it ever since.

> [!IMPORTANT]
>  Especially the math-part of my implementation relies heavily on [this article](https://www.a1k0n.net/2011/07/20/donut-math.html). If you want to recreate the donut, I'd suggest to just read that instead. My implementation works, but i am 100% sure there are way more efficient ways of rendering this donut.

## What is the donut?

The ASCII-Donut is a torus shape, which is rendered entirely in the console window using ASCII-characters. It also plays an animation in the console, spinning around the X and Z-axis. It looks something like this:

<img src="https://github.com/SimonHls/SimonHls/raw/main/donut.png" width="400" />

## How to build the donut

There are basically two parts to this:

1. Figure out the math to generate a donut as a set of 3d points.
2. Figure out how to display this list of points inside the console.

### Generating the points

First, we need a way to store the 3d points. Since a point is just a collection of primitive data types (doubles for x, y, z and l (we'll get to l later)), a struct is perfect for that.

``` csharp
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
}
```

Now we need a way to build a torus. For that, we first build a circle with an offset to the X-axis. We can do this by using a rotation matrix. The circle is the cross-section of our donut. The visualizations are done with [Desmos](https://www.desmos.com/).

<img src="https://github.com/SimonHls/SimonHls/raw/main/3dDonutCircle.png" width="400" />

If we build more of these circles, while rotating each new one a bit more around the Y-axis, we get a donut shape.

<img src="https://github.com/SimonHls/SimonHls/raw/main/3dDonut.png" width="400" />

We also need to be able to rotate the donut around two axis, to get the spinning animation. This is also done using another rotation matrix. Combining all three matrizes gives us a way to build a donut in any rotation. The math is explained in the article mentioned above. My C#-method to get a single point looks like this:

```csharp
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
```

A and B are the angles of rotation for the entire donut (the axis rotations). Since these are constant for each frame (the donut is only ever fully built with the same axis rotations), we can precaluculate the angle functions and pass them in. 

The `circleAngle` and `torusAngle` are the current angles of the dot we want to build. The circle angle is the angle between the dots from the center of each circle, and the torus angle is the angle between the planes of each Basically, for every circle we increment through these angles until we are at $2\Pi$, which is a full rotation. This also means our angles need to be fractions of $2\Pi$. We then increment the torusAngle the same way, as a fraction of $2\Pi$. In my program, i called the number of "steps" for each circle and torus the "resolution". A resolution for the circle of 5 would mean we get 5 points per circle, so the angle would be $2\Pi / 5$.

This sounds more complicated than it is. We pass in the circle ond torus resolutions in the constructor, and then call the point function with this loop:

``` csharp
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
            nextCircleAngle += 2 * Math.PI / _circleResolution; // <- we increment the angle by a full rotation divided by the resolution
        }
        nextTorusAngle += 2 * Math.PI / _torusResolution; // <- same for the torus
    }

    return points;
}
```

This will give us the full list of points for a donut with the axis rotations A and B. 

For our visualization, we also need to know the brightness of each dot in our points list. This luminance value is also stored in each `Point3d`, and calculated in the `GetDonutPoint` method. The luminance is calculated using the dot product of the surface normal for each dot and a randomly chosen light source vector. The math is also explained in the article mentioned above.

### Displaying the donut

Now, we need to display the donut in the console. I will not describe this in detail, but the code of the "Screen" class is in the repo. But here are some of the key things I've learned:

#### We need to project the points from 3d to 2d. 

This projection allows us to get a result which looks "correct" perspective-wise. The math for that is in the article, and pretty simple.

#### We need to do Z-buffering. 

This means, if two points have the same X and Y coordinates on the 2d screen, we need to show the point with the z-value closer to us. Otherwise, we can see through the donut.

I did this by cónverting my list of points to a dictionaly `visiblePoints`, where the key is the X and Y-coordinate on the screen a point wants to go to, and the value is the projected point. When projecting each point, we check if that key already exists in the dictionary, and if it does, we choose the point with the higher Z-value.

#### We need to paste the whole donut into the console at once

I first tried to draw each point individually by placing the cursor at the coordinates. For the next frame, i would clear the console and draw the next donut the same way. That workded, but was very slow. It also flickered every time i cleared the console. 

I read that overwriting the console is much faster, so instead of placing each character individually, i used the string builder class to first build the entire donut, then paste the string into the console. This worked great and is a lot faster.

### Optimization potential

While building, i noticed a lot of potential for optimization. For example:

- I calculate way more points for the donut than i actually need. This could probably be optimized for the screen size, chosing the resolution values dynamically.
- I wonder if there is a way to not calculate points on the backside of the donut at all.
- The normalization of the luminance to ascii values leads to some weird flickering, maybe this can be done smoother.
- Converting the points to pixels probably leads to a lot of overwriting of existing values, because multiple points might map to the same pixel, not only on the Z-axis but also on X and Y. Maybe I could purge these duplicates somehow.

Still, i think this is a cool project and it's certainly nice to look at!
