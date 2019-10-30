namespace Kingdom.Roslyn.Compilation
{
    /// <summary>
    /// Callback used when Merging assets.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TBits"></typeparam>
    /// <param name="obj"></param>
    /// <param name="bits"></param>
    /// <returns></returns>
    public delegate T MergeAssetsCallback<T, in TBits>(T obj, TBits bits);
}
