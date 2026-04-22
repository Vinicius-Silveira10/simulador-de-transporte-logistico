using UnityEngine;

public static class GameConfig
{
    // Centralização de Endpoints para evitar dessincronização entre scripts
    public static string API_BASE_URL = "http://localhost:5041/api";
    
    // Atalhos para Endpoints Comuns
    public static string LOGIN_URL = $"{API_BASE_URL}/Company/login";
    public static string COMPANY_BASE_URL = $"{API_BASE_URL}/Company";
    public static string TRIPS_URL = $"{API_BASE_URL}/Trips";
}
