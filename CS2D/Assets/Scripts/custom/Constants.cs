namespace custom
{
    public static class Constants
    {
        public static string serverIP = "127.0.0.1";

        // Server ports
        public static int server_base_port = 9000;

        // Server ports
        public static int clients_base_port = 10000;

        public static int pps = 100;
        public static int requiredSnapshots = 3;
        public static float sendRate = 1f / pps;
        
        public static float speed = 3;
        public static float rotationSpeed = 1.0f;

    }
}
