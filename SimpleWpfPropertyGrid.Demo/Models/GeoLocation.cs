using SimpleWpfPropertyGrid;

namespace SimpleWpfPropertyGrid.Demo.Models;

public class GeoLocation
{
    [PropertyGridNumericUpDown]
    [PropertyGridStep(0.0001)]
    [PropertyGridDecimals(4)]
    public double Latitude { get; set; }

    [PropertyGridNumericUpDown]
    [PropertyGridStep(0.0001)]
    [PropertyGridDecimals(4)]
    public double Longitude { get; set; }

    [PropertyGridNumericUpDown]
    [PropertyGridStep(10.0)]
    [PropertyGridDecimals(1)]
    public double AltitudeMeters { get; set; }
}
