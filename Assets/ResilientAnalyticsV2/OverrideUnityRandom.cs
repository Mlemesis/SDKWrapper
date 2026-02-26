//use the same namespace as the SDK using the normal Unity engine api
namespace Interview.Mocks
{
// This mimics the structure of the real Unity API
    public static class UnityEngine
    {
        public static class Random
        {
            // Use System.Random which is safe to call from any thread
            private static readonly System.Random _generator = new System.Random();

            public static float value
            {
                get
                {
                    lock (_generator) // Ensure thread safety if multiple tasks run
                    {
                        return (float)_generator.NextDouble();
                    }
                }
            }
        }
    }
}