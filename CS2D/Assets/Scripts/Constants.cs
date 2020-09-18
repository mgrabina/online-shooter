using System.Collections;
using System.Collections.Generic;
using System.Net;
using Tests;
using UnityEngine;

public static class Constants
{
    public static string serverIP = "127.0.0.1";

    // Server ports
    public static int server_registrationChannelPort = 9000;
    public static int server_visualizationChannelPort = 9001;
    public static int server_clientInputChannelPort = 9002;
    public static int server_serverACKChannelPort = 9003;

    // Server ports
    public static int client_registrationChannelPort = 10000;
    public static int client_visualizationChannelPort = 10001;
    public static int client_clientInputChannelPort = 10002;
    public static int client_serverACKChannelPort = 10003;

    public static int pps = 10;
    public static int requiredSnapshots = 3;
    public static float sendRate = 1f / pps;
    
    public enum MessageType
    {
        // Connection
        CONNECTION_REQUEST,
        CONNECTION_ACCEPT,
        CONNECTION_REJECT,
        
        // Commands send
    }
}
