using ImageMagick;

namespace Phloem.Infrastructure.Settings.Images;

/// <summary>
/// "Is this photo too blurry?" using the standard variance-of-Laplacian test: sharp photos have strong edges,
/// so the Laplacian (an edge filter) varies a lot; blurry photos give a low variance.
/// </summary>
/// <remarks>
/// The photo is first shrunk to <see cref="AnalysisMaxSide"/> and turned grey, so the score is quick to compute
/// and comparable between a 12 MP phone photo and a small one.
/// </remarks>
internal static class BlurDetector
{
    /// <summary>Longest side used for the test.</summary>
    private const uint AnalysisMaxSide = 800;

    /// <summary>The sharpness score of an image (higher = sharper).</summary>
    /// <param name="image">The decoded, upright photo. Not changed (a copy is analysed).</param>
    /// <returns>Variance of the Laplacian on a 0–255 grey scale.</returns>
    public static double Sharpness(IMagickImage<byte> image)
    {
        using var grey = image.Clone();
        grey.Resize(new MagickGeometry(AnalysisMaxSide, AnalysisMaxSide));   // keeps the shape, only shrinks
        grey.Grayscale();

        var width = (int)grey.Width;
        var height = (int)grey.Height;
        if (width < 3 || height < 3) return 0;

        using var pixels = grey.GetPixels();
        var values = pixels.ToByteArray("R");
        if (values is null) return 0;

        // Laplacian: 4 × centre − up − down − left − right, over every inner pixel
        double sum = 0, sumOfSquares = 0;
        long count = 0;
        for (var y = 1; y < height - 1; y++)
        {
            var row = y * width;
            for (var x = 1; x < width - 1; x++)
            {
                var i = row + x;
                double laplacian = 4 * values[i] - values[i - 1] - values[i + 1] - values[i - width] - values[i + width];
                sum += laplacian;
                sumOfSquares += laplacian * laplacian;
                count++;
            }
        }

        var mean = sum / count;
        return sumOfSquares / count - mean * mean;
    }
}