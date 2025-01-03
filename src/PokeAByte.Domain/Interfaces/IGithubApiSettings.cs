﻿namespace PokeAByte.Domain.Interfaces;

public interface IGithubApiSettings
{
    public string GetBaseRequestString();
    public string GetDirectory();
    public string GetAcceptValue();
    public string GetApiVersionValue();
    public string GetTokenValue();
    public string GetFormattedToken();
    public string GetGithubUrl();
    public void CopySettings(IGithubApiSettings settings);
    public void SaveChanges();
}