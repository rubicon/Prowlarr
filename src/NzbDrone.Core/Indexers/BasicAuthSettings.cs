using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers
{
    public class BasicAuthSettingsValidator : AbstractValidator<BasicAuthSettings>
    {
        public BasicAuthSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
        }
    }

    public class BasicAuthSettings : IProviderConfig
    {
        private static readonly BasicAuthSettingsValidator Validator = new BasicAuthSettingsValidator();

        public BasicAuthSettings()
        {
            Username = "";
            Password = "";
        }

        [FieldDefinition(1, Label = "Username", HelpText = "Site Username")]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "Password", Privacy = PrivacyLevel.Password, Type = FieldType.Password, HelpText = "Site Password")]
        public string Password { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
