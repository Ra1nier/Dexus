// <copyright file="Extensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace GuardianServices;

/// <summary>
/// Provides Extensions to the List class.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Prints the contents of a List in the following format: "item1, item2, item3, ...".
    /// </summary>
    /// <typeparam name="T">Type used for the list.</typeparam>
    /// <param name="list">List to print.</param>
    /// <returns>A string representing the list.</returns>
    public static string Print<T>(this List<T> list)
    {
        string printedString = string.Empty;
        if (list.Count > 0)
        {
            foreach (var component in list)
            {
                printedString += component.ToString() + ", ";
            }
        }

        return printedString.TrimEnd(',', ' ');
    }

    /// <summary>
    /// Prints the contents of a List in the following CSV format: "item1,item2,item3, ...".
    /// </summary>
    /// <typeparam name="T">Type used for the list.</typeparam>
    /// <param name="list">List to print.</param>
    /// <returns>A string in CSV format representing the list.</returns>
    public static string PrintCSV<T>(this List<T> list)
    {
        string printedString = string.Empty;
        if (list.Count > 0)
        {
            foreach (var component in list)
            {
                printedString += component.ToString() + ",";
            }
        }

        return printedString.TrimEnd(',', ' ');
    }
}
