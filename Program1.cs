using System;
using System.Collections.Generic;

class Program
{
    static int size = 5;
    static int[,] grid = new int[size, size];
    static HashSet<int> used = new HashSet<int>();
    static int[] numbers = new int[25];
    static (int, int) centerPos = (2, 2); // Row 3, Col 3 (zero-based)
    static int centerValue = 13;

    // Prime positions (zero-based)
    static (int, int)[] primePositions = {
        (0,1),(0,3),(1,0),(1,2),(2,1),(2,3),(3,0),(3,2),(4,1),(4,3)
    };

    static void Main()
    {
        for (int i = 0; i < 25; i++) numbers[i] = i + 1;

        grid[centerPos.Item1, centerPos.Item2] = centerValue;
        used.Add(centerValue);

        if (Backtrack(0))
        {
            Console.WriteLine("Valid grid found:");
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    Console.Write(grid[r, c]);
                    if (!(r == size - 1 && c == size - 1)) Console.Write(",");
                }
            }
        }
        else
        {
            Console.WriteLine("No solution found.");
        }
    }

    static bool Backtrack(int pos)
    {
        if (pos == size * size)
        {
            return IsValid();
        }

        int r = pos / size;
        int c = pos % size;

        if ((r, c) == centerPos) return Backtrack(pos + 1);

        foreach (int num in numbers)
        {
            if (used.Contains(num)) continue;

            grid[r, c] = num;
            used.Add(num);

            if (IsLocalValid(r, c))
            {
                if (Backtrack(pos + 1)) return true;
            }

            used.Remove(num);
            grid[r, c] = 0;
        }

        return false;
    }

    static bool IsLocalValid(int r, int c)
    {
        int val = grid[r, c];

        // Orthogonal check
        foreach (var (dr, dc) in new (int, int)[] { (1,0),(-1,0),(0,1),(0,-1) })
        {
            int nr = r + dr, nc = c + dc;
            if (nr >= 0 && nr < size && nc >= 0 && nc < size && grid[nr, nc] != 0)
            {
                if (Math.Abs(grid[nr, nc] - val) == 1) return false;
            }
        }

        // Diagonal check
        foreach (var (dr, dc) in new (int, int)[] { (1,1),(1,-1),(-1,1),(-1,-1) })
        {
            int nr = r + dr, nc = c + dc;
            if (nr >= 0 && nr < size && nc >= 0 && nc < size && grid[nr, nc] != 0)
            {
                if (Math.Abs(grid[nr, nc] - val) == 2) return false;
            }
        }

        return true;
    }

    static bool IsValid()
    {
        int primeSum = 0;
        foreach (var (r, c) in primePositions)
        {
            primeSum += grid[r, c];
        }
        return primeSum % 2 == 0;
    }
}