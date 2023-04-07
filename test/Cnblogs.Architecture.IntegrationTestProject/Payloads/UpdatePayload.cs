namespace Cnblogs.Architecture.IntegrationTestProject.Payloads;

public record UpdatePayload(bool NeedExecutionError = false, bool NeedValidationError = false);