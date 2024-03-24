namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;

/// <summary>
/// Sets the behaviour of entity extractor in case more then one rule matches selector state.
/// </summary>
public enum EntityExtractorDiscoverRuleApplyBehavior
{
    /// <summary>
    /// Accepts all discover rules and attempts to find fitting entity by each rule if previous fails, until all rules fail.
    /// </summary>
    TryAllAcceptedRegistrations,
    /// <summary>
    /// Accepts only first fitting discover rule. If discover by it fails - extractor will return null.
    /// </summary>
    OnlyFirstAcceptedRegistration
}