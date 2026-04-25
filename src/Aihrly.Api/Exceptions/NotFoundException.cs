namespace Aihrly.Api.Exceptions;

public class NotFoundException(string resource, object id)
    : Exception($"{resource} with id '{id}' was not found.");
