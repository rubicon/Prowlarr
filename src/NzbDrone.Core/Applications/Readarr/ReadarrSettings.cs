using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Applications.Readarr
{
    public class ReadarrSettingsValidator : AbstractValidator<ReadarrSettings>
    {
        public ReadarrSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).IsValidUrl();
            RuleFor(c => c.ProwlarrUrl).IsValidUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class ReadarrSettings : IProviderConfig
    {
        private static readonly ReadarrSettingsValidator Validator = new ReadarrSettingsValidator();

        public ReadarrSettings()
        {
            ProwlarrUrl = "http://localhost:9696";
            BaseUrl = "http://localhost:8787";
        }

        [FieldDefinition(0, Label = "Prowlarr Server", HelpText = "Prowlarr server URL as Readarr sees it, including http(s):// and port if needed")]
        public string ProwlarrUrl { get; set; }

        [FieldDefinition(1, Label = "Readarr Server", HelpText = "Readarr server URL, including http(s):// and port if needed")]
        public string BaseUrl { get; set; }

        [FieldDefinition(2, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "The ApiKey generated by Readarr in Settings/General")]
        public string ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}