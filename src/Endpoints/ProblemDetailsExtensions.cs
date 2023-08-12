﻿using Flunt.Notifications;
using System.Runtime.CompilerServices;

namespace IWantApp.Endpoints;

public static class ProblemDetailsExtensions
{
    //Utilizo o this para indicar que é uma extensão
    public static Dictionary<string, string[]> ConvertToProblemDetails(this IReadOnlyCollection<Notification> notifications)
    {
        return notifications.GroupBy(g => g.Key).ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());

    }
}
