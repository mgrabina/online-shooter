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
        
        public static float speed = 0.8f;
        public static float mouseSensibility = 0.1f;
        public static float rotationSpeed = 1.0f;

        public static float health_increment_percentage = 0.0025f;
        public static float health_decrement_percentage = 0.25f;
        public static float min_health_alive = 0.1f;
    }
}
