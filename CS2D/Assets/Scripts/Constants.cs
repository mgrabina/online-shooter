using System.Collections;
using System.Collections.Generic;
using System.Net;
using Tests;
using UnityEngine;

public static class Constants
{
    public static string serverIP = "127.0.0.1";
    public static int registrationChannelPort = 9000;
    public static int visualizationChannelPort = 9001;
    public static int clientInputChannelPort = 9002;
    public static int serverACKChannelPort = 9003;
    public static int pps = 10;
    public static int requiredSnapshots = 3;
    public static float sendRate = 1f / pps;
}
