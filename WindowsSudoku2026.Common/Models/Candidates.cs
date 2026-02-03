using System.Numerics;

namespace WindowsSudoku2026.Common.Models;

public class Candidates
{
    private int _bitMask;
    public bool this[int candidate]
    {
        get => Contains(candidate);
        set
        {
            if (value)
                Add(candidate);
            else
                Remove(candidate);
        }
    }
    public Candidates()
    {
        BitMask = 0b111111111;
    }
    public Candidates(int bitMask)
    {
        BitMask = bitMask;
    }
    public int BitMask
    {
        get => _bitMask;
        set
        {
            if (_bitMask == value)
                return;

            _bitMask = value;
        }
    }
    public Candidates(HashSet<int> candidates)
    {
        BitMask = 0;
        FromHashSet(candidates);
    }

    public void Clear()
    {
        BitMask = 0b000000000;
    }
    public bool Any()
    {
        return BitMask != 0;
    }
    public void Add(int candidate)
    {
        if (candidate < 1 || candidate > 9)
            throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

        BitMask |= (1 << (candidate - 1));
    }

    public bool Remove(int candidate)
    {
        if (candidate < 1 || candidate > 9)
            throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

        int mask = 1 << (candidate - 1);

        // Check if the bit is already cleared
        if ((BitMask & mask) == 0)
        {
            return false; // Candidate was not present
        }

        BitMask &= ~mask; // Clear the bit for the candidate

        return true; // Candidate was successfully removed
    }
    /// <summary>
    /// Gibt die Anzahl der gesetzten Kandidaten (Bits) zurück.
    /// Nutzt hochperformante CPU-Instruktionen (PopCount).
    /// </summary>
    public int Count => BitOperations.PopCount((uint)BitMask);
    public bool Contains(int candidate)
    {
        if (candidate < 1 || candidate > 9)
            throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

        return (BitMask & (1 << (candidate - 1))) != 0; // Check the bit for the candidate
    }

    // New method to set candidates from a bitmask
    public void FromBitMask(int bitmask)
    {
        BitMask = bitmask;
    }
    public void FromHashSet(HashSet<int> candidates)
    {
        int mask = 0;
        foreach (int candidate in candidates)
        {
            if (candidate < 1 || candidate > 9)
                throw new ArgumentOutOfRangeException(nameof(candidate), "Candidate must be between 1 and 9.");

            mask |= (1 << (candidate - 1)); // Set the bit for the candidate
        }

        BitMask = mask;
    }

    public int UnionWithAsBitMask(Candidates other) => BitMask | other.BitMask;
    public Candidates UnionWith(Candidates other) => new(UnionWithAsBitMask(other));
    public int IntersectWithAsBitMask(Candidates other) => BitMask & other.BitMask;
    public Candidates IntersectWith(Candidates other) => new(IntersectWithAsBitMask(other));

    public static Candidates FromString(string bitString)
    {
        if (string.IsNullOrWhiteSpace(bitString) || bitString.Length != 9)
            return new Candidates(0);

        int mask = 0;
        for (int i = 0; i < 9; i++)
        {
            if (bitString[i] == '1')
            {
                mask |= (1 << (8 - i));
            }
        }
        return new Candidates(mask);
    }
    public override string ToString()
    {
        return Convert.ToString(BitMask, 2).PadLeft(9, '0'); // PadLeft 9 to include all bits
    }
}

