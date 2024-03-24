using System;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor.Exceptions;

/// <summary>
/// Exception that is thrown in case of Entity Extractor not configured properly when called.
/// </summary>
public class EntityExtractorNotConfiguredException : Exception
{
    private const string ErrorMessage = "Cannot discover entity - no Entity Extractor configured. "
                                        + "This can be caused by not calling 'DiscoveringRepositoryBase.ConfigureExtractor' before DiscoveringRepositoryBase.Discover call, "
                                        + "or by invalid override of DiscoveringRepositoryBase.ConfigureExtractor on derived repository. "
                                        + "* Please add DiscoveringRepositoryBase.ConfigureExtractor call upon your repository instance creation - this can be done "
                                        + "by DI Container. \r\n"
                                        + "* Please check if you overrided DiscoveringRepositoryBase.ConfigureExtractor - if you did, it's implementation have to set "
                                        + "DiscoveringRepositoryBase.EntityExtractor property. \r\n\r\n";

    /// <summary>Initializes a new instance of the <see cref="EntityExtractorNotConfiguredException" /> class.</summary>
    public EntityExtractorNotConfiguredException() : base(ErrorMessage)
    {
    }
}