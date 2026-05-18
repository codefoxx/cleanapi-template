namespace Company.Template.Composition.Framework;

/// <summary>
///     Marks a type as an explicit feature identifier for composition module registration.
/// </summary>
/// <remarks>
///     Feature markers avoid relying on naming conventions when activating feature modules from the composition root.
/// </remarks>
public interface IFeature;
