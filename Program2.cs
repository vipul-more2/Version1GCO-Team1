using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static readonly int N = 5;
    static readonly int[] grid = new int[25];
    static readonly bool[] used = new bool[26];

    // Prime-indexed grid cells
    static readonly (int r,int c)[] primeCells =
    {
        (0,1),(0,3),(1,0),(1,2),(2,1),(2,3),(3,0),(3,2),(4,1),(4,3)
    };

    static void Main()
    {
        // Fixed center
        grid[2 * 5 + 2] = 13;
        used[13] = true;

        // Top row median constraint:
        // sorted row1 must have median = 14 → one cell in row1 must be 14
        // But we do not know which one; solver will decide.
        
        Solve(0);

        Console.WriteLine("NO SOLUTION FOUND");
    }

    static bool Solve(int idx)
    {
        if (idx == 25)
        {
            if (!CheckPrimeSumEven()) return false;

            PrintGrid();
            Console.WriteLine("\nGrid(5,5) = " + grid[24]);
            Environment.Exit(0);
        }

        int r = idx / 5;
        int c = idx % 5;

        // Skip center which is fixed
        if (r == 2 && c == 2)
            return Solve(idx + 1);

        // C4: If we are filling row 1 (r=0), enforce that 14 must appear somewhere
        if (r == 0 && idx == 4)  // last cell of row 1
        {
            // If 14 not yet used, try it here
            if (!used[14])
            {
                if (CanPlace(r, c, 14))
                {
                    Place(r, c, 14);
                    if (Solve(idx + 1)) return true;
                    Remove(r, c, 14);
                }
            }
            return false;
        }

        for (int v = 1; v <= 25; v++)
        {
            if (used[v]) continue;

            if (!CanPlace(r, c, v)) continue;

            Place(r, c, v);
            if (Solve(idx + 1)) return true;
            Remove(r, c, v);
        }

        return false;
    }

    static void Place(int r, int c, int v)
    {
        grid[r * 5 + c] = v;
        used[v] = true;
    }

    static void Remove(int r, int c, int v)
    {
        grid[r * 5 + c] = 0;
        used[v] = false;
    }

    static bool CanPlace(int r, int c, int v)
    {
        // C1: No orthogonal consecutive
        int[][] ortho = { new[]{1,0}, new[]{-1,0}, new[]{0,1}, new[]{0,-1} };
        foreach (var d in ortho)
        {
            int rr = r + d[0], cc = c + d[1];
            if (Inside(rr,cc))
            {
                int w = grid[rr*5 + cc];
                if (w != 0 && Math.Abs(w - v) == 1) return false;
            }
        }

        // C2: No diagonal diff = 2
        int[][] diag = { new[]{1,1}, new[]{1,-1}, new[]{-1,1}, new[]{-1,-1} };
        foreach (var d in diag)
        {
            int rr = r + d[0], cc = c + d[1];
            if (Inside(rr,cc))
            {
                int w = grid[rr*5 + cc];
                if (w != 0 && Math.Abs(w - v) == 2) return false;
            }
        }

        return true;
    }

    static bool Inside(int r, int c) => r >= 0 && r < 5 && c >= 0 && c < 5;

    static bool CheckPrimeSumEven()
    {
        int sum = 0;
        foreach (var (r, c) in primeCells)
            sum += grid[r * 5 + c];

        return sum % 2 == 0;
    }

    static void PrintGrid()
    {
        Console.WriteLine("Solution Grid:");
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
                Console.Write(grid[r * 5 + c].ToString().PadLeft(3));
            Console.WriteLine();
        }

        Console.WriteLine("\nSubmission Format:");
        Console.WriteLine(string.Join(",", grid));
    }
}
