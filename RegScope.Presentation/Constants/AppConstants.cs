namespace RegScope.Presentation.Constants;

/// <summary>
/// Provides application-wide presentation constants.
/// </summary>
public sealed class AppConstants
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppConstants"/> class.
    /// </summary>
    public AppConstants()
    {
        Images = new();
        Common = new();
    }

    public ImagePaths Images { get; }

    public CommonConstants Common { get; }
}
