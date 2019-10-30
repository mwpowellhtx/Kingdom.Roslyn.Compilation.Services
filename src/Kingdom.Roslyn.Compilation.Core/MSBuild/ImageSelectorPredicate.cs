namespace Kingdom.Roslyn.Compilation.MSBuild
{
    /// <summary>
    /// Returns whether the <paramref name="value"/> meets the intended Selection criteria.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public delegate bool ImageSelectorPredicate(object value);
}
