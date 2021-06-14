using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers
{
    public class BasicAuthSettingsValidator : AbstractValidator<BasicAuthSettings>
    {
        public BasicAuthSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.BaseUrl).NotEmpty();
        }
    }

    public class BasicAuthSettings : IIndexerSettings
    {
        private static readonly BasicAuthSettingsValidator Validator = new BasicAuthSettingsValidator();

        public BasicAuthSettings()
        {
            Username = "";
            Password = "";
            BaseUrl = "";
        }

        [FieldDefinition(1, Label = "BaseUrl", Type = FieldType.Select, SelectOptionsProviderAction = "getUrls", HelpText = "Select which baseurl Prowlarr will use for requests to the site")]
        public string BaseUrl { get; set; }

        [FieldDefinition(2, Label = "Username", HelpText = "Site Username")]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Password", Privacy = PrivacyLevel.Password, Type = FieldType.Password, HelpText = "Site Password")]
        public string Password { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
