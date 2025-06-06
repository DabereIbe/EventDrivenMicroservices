﻿using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Settings;

public static class SettingExtension
{
    public static IServiceCollection UseRabbitMqSettings(
        this IServiceCollection services)
    {
        services.AddSingleton(ReadConfig());

        return services;
    }

    private static RabbitMqSettings? ReadConfig()
    {
        var configFile = File
            .ReadAllText(Path.GetDirectoryName(
                             Assembly.GetExecutingAssembly().Location)
                         + "/appsettings.json");

        var jsonSerializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        jsonSerializeOptions.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Deserialize<RabbitMqSettings>(configFile, jsonSerializeOptions);
    }
}