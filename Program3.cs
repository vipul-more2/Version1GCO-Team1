using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

class Solver6x6
{
    const int N = 6;
    const int SIZE = N * N;
    const int MAXV = 36;

    // Special rook set
    static readonly int[] RookVals = { 1, 12, 24, 36 };

    // Precomputed neighbors
    static readonly int[][] Orth = new int[SIZE][];
    static Solver6x6()
    {
        for (int i = 0; i < SIZE; i++)
        {
            var list = new List<int>();
            int r = i / N, c = i % N;
            int[] dr = { -1, 1, 0, 0 }, dc = { 0, 0, -1, 1 };
            for (int k = 0; k < 4; k++)
            {
                int rr = r + dr[k], cc = c + dc[k];
                if (rr >= 0 && rr < N && cc >= 0 && cc < N) list.Add(rr * N + cc);
            }
            Orth[i] = list.ToArray();
        }
    }

    int[] assign = new int[SIZE]; // 0 if unassigned, else 1..36
    int assignedCount = 0;
    // remaining values mask: bit (v-1) set means value v available
    int remainingMask = (1 << MAXV) - 1;

    // quick lookup: position of rook values if already placed (-1 if not)
    int[] rookPos = new int[RookVals.Length];

    // precompute map value->index in rook list (-1 if not in rook set)
    static readonly int[] rookIndexOf = new int[MAXV + 1];
    static Solver6x6()
    {
        for (int i = 0; i <= MAXV; i++) rookIndexOf[i] = -1;
        for (int i = 0; i < RookVals.Length; i++) rookIndexOf[RookVals[i]] = i;
    }

    public Solver6x6()
    {
        for (int i = 0; i < SIZE; i++) assign[i] = 0;
        for (int i = 0; i < rookPos.Length; i++) rookPos[i] = -1;

        // fixed seed: Grid(1,1) -> index 0 must be 1
        Place(0, 1);
    }

    // small helper: number of 1-bits
    static int BitCount(int x) => BitOperations.PopCount((uint)x);

    // Place value v at cell idx (no checks here; caller ensures validity)
    void Place(int idx, int v)
    {
        assign[idx] = v;
        assignedCount++;
        remainingMask &= ~(1 << (v - 1));
        int ri = rookIndexOf[v];
        if (ri >= 0) rookPos[ri] = idx;
    }

    // Unplace value at cell idx
    void Unplace(int idx, int v)
    {
        assign[idx] = 0;
        assignedCount--;
        remainingMask |= (1 << (v - 1));
        int ri = rookIndexOf[v];
        if (ri >= 0) rookPos[ri] = -1;
    }

    // Check if placing v at idx would immediately violate orthogonal constraint
    bool ViolatesOrth(int idx, int v)
    {
        foreach (int n in Orth[idx])
        {
            int w = assign[n];
            if (w != 0 && Math.Abs(w - v) == 1) return true;
        }
        return false;
    }

    // Check rook constraint feasibility: rook values placed so far must be in distinct rows & columns.
    // Also, if placing v is a rook value, it must not share row or column with an already placed rook value.
    bool FeasibleRooksIfPlaced(int idx, int v)
    {
        int ri = rookIndexOf[v];
        if (ri == -1) return true; // not a rook value

        int r = idx / N, c = idx % N;
        for (int j = 0; j < rookPos.Length; j++)
        {
            if (j == ri) continue;
            int p = rookPos[j];
            if (p == -1) continue;
            int rr = p / N, cc = p % N;
            if (rr == r || cc == c) return false;
        }
        return true;
    }

    // Domain mask for cell idx (available values filtered by immediate constraints)
    int DomainMask(int idx)
    {
        if (assign[idx] != 0) return 0;
        int mask = remainingMask;

        // forbid values that conflict with orthogonal neighbors
        foreach (int n in Orth[idx])
        {
            int w = assign[n];
            if (w != 0)
            {
                if (w > 1) mask &= ~(1 << (w - 2)); // forbid w-1
                if (w < MAXV) mask &= ~(1 << (w));   // forbid w+1
            }
        }

        // forbid rook placements that would conflict with existing rook placements
        // for each rook value rVal already placed, forbid values that would share its row/col if they are rook values
        for (int ri = 0; ri < RookVals.Length; ri++)
        {
            int placedPos = rookPos[ri];
            if (placedPos == -1) continue;
            int rr = placedPos / N, cc = placedPos % N;
            // For each other rook value not yet placed, if that value remains available, and assigning it here would share row/col -> forbid that value here.
            for (int rj = 0; rj < RookVals.Length; rj++)
            {
                if (rj == ri) continue;
                if (rookPos[rj] != -1) continue; // already placed, handled
                int candidateVal = RookVals[rj];
                // If this idx shares row or column with the already placed rook candidate, then candidateVal cannot be assigned here
                int rIdx = idx / N, cIdx = idx % N;
                if (rIdx == rr || cIdx == cc)
                {
                    mask &= ~(1 << (candidateVal - 1));
                }
            }
        }

        // also if this cell is in same row or column as some already-placed rook value, forbid it from taking any other rook value
        foreach (int unplacedRi in Enumerable.Range(0, RookVals.Length))
        {
            int p = rookPos[unplacedRi];
            if (p != -1) continue;
            // nothing to do here
        }

        // finally, enforce that if mask contains a rook value v, placing v here must be feasible (distinct rows/cols)
        int m = mask;
        int res = 0;
        while (m != 0)
        {
            int lsb = m & -m;
            int bit = 31 - BitOperations.LeadingZeroCount(lsb); // 0-based bit => value = bit+1
            int val = bit + 1;
            if (rookIndexOf[val] >= 0)
            {
                if (!FeasibleRooksIfPlaced(idx, val))
                {
                    // skip this val (don't include in res)
                }
                else
                {
                    res |= (1 << bit);
                }
            }
            else
            {
                res |= (1 << bit);
            }
            m &= m - 1;
        }
        return res;
    }

    // pick next unassigned cell by MRV
    int ChooseCellMRV(out int domainMask)
    {
        int bestIdx = -1;
        int bestCount = int.MaxValue;
        int bestMask = 0;
        for (int i = 0; i < SIZE; i++)
        {
            if (assign[i] != 0) continue;
            int dm = DomainMask(i);
            int cnt = BitCount(dm);
            if (cnt == 0)
            {
                domainMask = 0;
                return i; // forced fail
            }
            if (cnt < bestCount)
            {
                bestCount = cnt;
                bestIdx = i;
                bestMask = dm;
                if (bestCount == 1) break;
            }
        }
        domainMask = bestMask;
        return bestIdx;
    }

    // recursive backtracking
    bool Search()
    {
        if (assignedCount == SIZE)
        {
            // found complete assignment
            PrintSolution();
            return true;
        }

        int domain;
        int idx = ChooseCellMRV(out domain);
        if (idx == -1) return false; // no unassigned (should not happen)
        if (domain == 0) return false; // dead end

        // iterate candidate values (try smaller first)
        var cand = new List<int>();
        int m = domain;
        while (m != 0)
        {
            int lsb = m & -m;
            int bit = 31 - BitOperations.LeadingZeroCount(lsb);
            cand.Add(bit + 1);
            m &= m - 1;
        }

        // optional heuristic: try rook values earlier to prune
        cand.Sort((a, b) =>
        {
            int ia = (rookIndexOf[a] >= 0) ? 0 : 1;
            int ib = (rookIndexOf[b] >= 0) ? 0 : 1;
            if (ia != ib) return ia - ib;
            return a - b;
        });

        foreach (int v in cand)
        {
            if (ViolatesOrth(idx, v)) continue; // double-check
            if (!FeasibleRooksIfPlaced(idx, v)) continue;

            Place(idx, v);

            if (Search()) return true;

            Unplace(idx, v);
        }

        return false;
    }

    void PrintSolution()
    {
        // row-major comma separated
        string[] parts = new string[SIZE];
        for (int i = 0; i < SIZE; i++) parts[i] = assign[i].ToString();
        Console.WriteLine(string.Join(",", parts));
        Console.WriteLine();
        for (int r = 0; r < N; r++)
        {
            for (int c = 0; c < N; c++)
            {
                Console.Write(assign[r * N + c].ToString().PadLeft(4));
            }
            Console.WriteLine();
        }
    }

    static void Main()
    {
        var solver = new Solver6x6();
        Console.WriteLine("Solver starting... Fixed seed Grid(1,1)=1");
        bool found = solver.Search();
        if (!found) Console.WriteLine("No solution found.");
        #Mainmethod
    }
}
