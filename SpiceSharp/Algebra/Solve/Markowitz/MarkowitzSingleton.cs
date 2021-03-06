﻿using System;

namespace SpiceSharp.Algebra.Solve
{
    /// <summary>
    /// Markowitz-count based strategy for finding a pivot. This strategy will search for 
    /// singletons (rows or columns with only one element), these can be found rather cheaply.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public class MarkowitzSingleton<T> : MarkowitzSearchStrategy<T> where T : IFormattable, IEquatable<T>
    {
        /// <summary>
        /// Find a pivot in a matrix.
        /// </summary>
        /// <param name="markowitz">The Markowitz pivot strategy.</param>
        /// <param name="matrix">The matrix</param>
        /// <param name="eliminationStep">The current elimination step.</param>
        /// <returns>
        /// The pivot element, or null if no pivot was found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// markowitz
        /// or
        /// matrix
        /// </exception>
        /// <exception cref="ArgumentException">Invalid elimination step</exception>
        public override MatrixElement<T> FindPivot(Markowitz<T> markowitz, SparseMatrix<T> matrix, int eliminationStep)
        {
            if (markowitz == null)
                throw new ArgumentNullException(nameof(markowitz));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));
            if (eliminationStep < 1)
                throw new ArgumentException("Invalid elimination step");

            // No singletons left, so don't bother
            if (markowitz.Singletons == 0)
                return null;

            // Find the first valid singleton we can use
            int singletons = 0, index;
            for (var i = matrix.Size + 1; i >= eliminationStep; i--)
            {
                // First check the current pivot, else
                // search from last to first as it tends to push the higher markowitz
                // products downwards.
                index = i > matrix.Size ? eliminationStep : i;

                // Not a singleton, let's skip this one...
                if (markowitz.Product(index) != 0)
                    continue;

                // Keep track of how many singletons we have found
                singletons++;

                /*
                 * NOTE: In the sparse library of Spice 3f5 first the diagonal is checked,
                 * then the column is checked (if no success go to row) and finally the
                 * row is checked for a valid singleton pivot.
                 * The diagonal should actually not be checked, as the checking algorithm is the same
                 * as for checking the column (FindBiggestInColExclude will not find anything in the column
                 * for singletons). The original author did not find this.
                 * Also, the original algorithm has a bug in there that renders the whole code invalid...
                 * if (ChosenPivot != NULL) { break; } will throw away the pivot even if it was found!
                 * (ref. Spice 3f5 Libraries/Sparse/spfactor.c, line 1286)
                 */

                // Find the singleton element
                MatrixElement<T> chosen;
                if (markowitz.ColumnCount(index) == 0)
                {
                    // The last element in the column is the singleton element!
                    chosen = matrix.GetLastInColumn(index);

                    // Check if it is a valid pivot
                    var magnitude = markowitz.Magnitude(chosen.Value);
                    if (magnitude > markowitz.AbsolutePivotThreshold)
                        return chosen;
                }

                // Check if we can still use a row here
                if (markowitz.RowCount(index) == 0)
                {
                    // The last element in the row is the singleton element
                    chosen = matrix.GetLastInRow(index);

                    // When the matrix has an empty row, and an RHS element, it is possible
                    // that the singleton is not a singleton
                    if (chosen == null || chosen.Column < eliminationStep)
                    {
                        // The last element is not valid, singleton failed!
                        singletons--;
                        continue;
                    }

                    // First find the biggest magnitude in the column, not counting the pivot candidate
                    var element = chosen.Above;
                    var largest = 0.0;
                    while (element != null && element.Row >= eliminationStep)
                    {
                        largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                        element = element.Above;
                    }
                    element = chosen.Below;
                    while (element != null)
                    {
                        largest = Math.Max(largest, markowitz.Magnitude(element.Value));
                        element = element.Below;
                    }

                    // Check if the pivot is valid
                    var magnitude = markowitz.Magnitude(chosen.Value);
                    if (magnitude > markowitz.AbsolutePivotThreshold &&
                        magnitude > markowitz.RelativePivotThreshold * largest)
                        return chosen;
                }

                // Don't continue if no more singletons are available
                if (singletons >= markowitz.Singletons)
                    break;
            }

            // All singletons were unacceptable...
            return null;
        }
    }
}
