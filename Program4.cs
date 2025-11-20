// Run this program - it will calculate the exact count

// Runtime: Expect anywhere from 30 minutes to several hours

// The answer will be printed at the end
 
using System;

using System.Collections.Generic;
 
class CountSolutions

{

    const int GRID_SIZE = 5;

    const int TOTAL_CELLS = 25;

    static long solutionCount = 0;

    static void Main()

    {

        int[,] grid = new int[GRID_SIZE, GRID_SIZE];

        bool[] used = new bool[TOTAL_CELLS + 1];

        Console.WriteLine("Counting solutions for 5x5 grid (numbers 1-25)");

        Console.WriteLine("Constraint: No consecutive numbers orthogonally adjacent");

        Console.WriteLine("Please wait...\n");

        DateTime start = DateTime.Now;

        Solve(grid, used, 0);

        DateTime end = DateTime.Now;

        Console.WriteLine($"\n{'=',60}");

        Console.WriteLine($"FINAL ANSWER: {solutionCount:N0}");

        Console.WriteLine($"{'=',60}");

        Console.WriteLine($"Time: {(end - start).TotalMinutes:F2} minutes");

    }

    static void Solve(int[,] grid, bool[] used, int pos)

    {

        if (pos == TOTAL_CELLS)

        {

            solutionCount++;

            if (solutionCount % 1000000 == 0)

                Console.WriteLine($"Progress: {solutionCount:N0} solutions found...");

            return;

        }

        int row = pos / GRID_SIZE;

        int col = pos % GRID_SIZE;

        for (int num = 1; num <= TOTAL_CELLS; num++)

        {

            if (!used[num] && IsValid(grid, row, col, num))

            {

                grid[row, col] = num;

                used[num] = true;

                Solve(grid, used, pos + 1);

                grid[row, col] = 0;

                used[num] = false;

            }

        }

    }

    static bool IsValid(int[,] grid, int row, int col, int num)

    {

        // Check up

        if (row > 0 && grid[row-1, col] != 0 && Math.Abs(grid[row-1, col] - num) == 1)

            return false;

        // Check left

        if (col > 0 && grid[row, col-1] != 0 && Math.Abs(grid[row, col-1] - num) == 1)

            return false;

        // Check down

        if (row < GRID_SIZE-1 && grid[row+1, col] != 0 && Math.Abs(grid[row+1, col] - num) == 1)

            return false;

        // Check right

        if (col < GRID_SIZE-1 && grid[row, col+1] != 0 && Math.Abs(grid[row, col+1] - num) == 1)

            return false;

        return true;

    }

}
 