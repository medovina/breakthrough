using System;

// A simple pseudo-random number generator.  We use this instead of System.Random
// to ensure that simulation results will be identical in any C# runtime.
class SRandom {
    int n;
    
    public SRandom(int seed) {
        n = seed >= 0 ? seed : (int) DateTime.Now.Ticks;
    }
    
    // Return a pseudo-random number between 0 and (count - 1).
    public int next(int count) {
       n = (n * 1103515245 + 12345) & 0x7fffffff;
       double d = n / (double) 0x80000000;
       return (int) (count * d);
    }
}
