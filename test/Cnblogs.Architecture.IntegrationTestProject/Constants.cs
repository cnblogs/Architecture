﻿namespace Cnblogs.Architecture.IntegrationTestProject;

public static class Constants
{
    public const string AppName = "test-web";
    public const string IntegrationEventIdHeaderName = "X-IntegrationEvent-Id";

    public static class LogTemplates
    {
        public const string HandledIntegratonEvent = "Handled integration event {@event}.";
    }
}