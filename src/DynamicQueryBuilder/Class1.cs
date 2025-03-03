namespace DynamicQueryBuilder;

/// <summary>
/// Class Summary Test
/// </summary>
public class Class1
{
    /// <summary>
    /// Method to Sum all items from params.
    /// </summary>
    /// <param name="values">Items to be summ.</param>
    /// <returns>The total value of summ.</returns>
    public int SumItems(params int[] values) => values.Sum();
}