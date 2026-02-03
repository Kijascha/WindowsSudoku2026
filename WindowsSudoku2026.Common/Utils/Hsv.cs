
namespace WindowsSudoku2026.Common.Utils
{
    public struct Hsv
    {
        public double H; // 0..360
        public double S; // 0..1
        public double V; // 0..1

        public Hsv(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }

        public override string ToString() => $"H={H:F1}, S={S:F2}, V={V:F2}";
    }
}
