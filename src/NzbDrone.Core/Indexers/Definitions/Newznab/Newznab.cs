using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : UsenetIndexerBase<NewznabSettings>
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public override string Name => "Newznab";
        public override string[] IndexerUrls => GetBaseUrlFromSettings();
        public override string Description => "Newznab is an API search specification for Usenet";
        public override bool FollowRedirect => true;
        public override bool SupportsRedirect => true;
        public override bool SupportsPagination => true;
        public override IndexerPrivacy Privacy => IndexerPrivacy.Private;
        public override IndexerCapabilities Capabilities { get => GetCapabilitiesFromSettings(); protected set => base.Capabilities = value; }
        public override int PageSize => GetProviderPageSize();

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new NewznabRequestGenerator(_capabilitiesProvider)
            {
                Definition = Definition,
                PageSize = PageSize,
                Settings = Settings
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new NewznabRssParser(Settings, Definition, _capabilitiesProvider);
        }

        public string[] GetBaseUrlFromSettings()
        {
            if (Definition == null || Settings?.Capabilities == null)
            {
                return new[] { "" };
            }

            return new[] { Settings.BaseUrl };
        }

        protected override NewznabSettings GetDefaultBaseUrl(NewznabSettings settings)
        {
            return settings;
        }

        public IndexerCapabilities GetCapabilitiesFromSettings()
        {
            var caps = new IndexerCapabilities();

            if (Definition == null || Settings?.Capabilities?.Categories == null)
            {
                return caps;
            }

            foreach (var category in Settings.Capabilities.Categories)
            {
                caps.Categories.AddCategoryMapping(category.Name, category);
            }

            caps.SupportsRawSearch = Settings?.Capabilities?.SupportsRawSearch ?? false;
            caps.SearchParams = Settings?.Capabilities?.SearchParams ?? new List<SearchParam> { SearchParam.Q };
            caps.TvSearchParams = Settings?.Capabilities?.TvSearchParams ?? new List<TvSearchParam>();
            caps.MovieSearchParams = Settings?.Capabilities?.MovieSearchParams ?? new List<MovieSearchParam>();
            caps.MusicSearchParams = Settings?.Capabilities?.MusicSearchParams ?? new List<MusicSearchParam>();
            caps.BookSearchParams = Settings?.Capabilities?.BookSearchParams ?? new List<BookSearchParam>();

            return caps;
        }

        public override IndexerCapabilities GetCapabilities()
        {
            // Newznab uses different Caps per site, so we need to cache them to db on first indexer add to prevent issues with loading UI and pulling caps every time.
            return _capabilitiesProvider.GetCapabilities(Settings, Definition);
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return GetDefinition("abNZB", GetSettings("https://abnzb.com"), categories: new[] { 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("altHUB", GetSettings("https://api.althub.co.za"), categories: new[] { 2000, 3000, 4000, 5000, 7000 });
                yield return GetDefinition("AnimeTosho (Usenet)", GetSettings("https://feed.animetosho.org"), categories: new[] { 2020, 5070 });
                yield return GetDefinition("DOGnzb", GetSettings("https://api.dognzb.cr"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("DrunkenSlug", GetSettings("https://drunkenslug.com"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000 });
                yield return GetDefinition("GingaDADDY", GetSettings("https://www.gingadaddy.com"));
                yield return GetDefinition("Miatrix", GetSettings("https://www.miatrix.com"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("Newz69", GetSettings("https://newz69.keagaming.com"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("NinjaCentral", GetSettings("https://ninjacentral.co.za"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("Nzb.su", GetSettings("https://api.nzb.su"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("NZBCat", GetSettings("https://nzb.cat"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000 });
                yield return GetDefinition("NZBFinder", GetSettings("https://nzbfinder.ws"), categories: new[] { 2000, 3000, 5000, 6000, 7000 });
                yield return GetDefinition("NZBgeek", GetSettings("https://api.nzbgeek.info"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("NzbNoob", GetSettings("https://www.nzbnoob.com"), categories: new[] { 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("NZBNDX", GetSettings("https://www.nzbndx.com"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("NzbPlanet", GetSettings("https://api.nzbplanet.net"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 });
                yield return GetDefinition("NZBStars", GetSettings("https://nzbstars.com"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000 });
                yield return GetDefinition("Tabula Rasa", GetSettings("https://www.tabula-rasa.pw", apiPath: @"/api/v1/api"), categories: new[] { 1000, 2000, 3000, 4000, 5000, 6000, 7000 });
                yield return GetDefinition("Generic Newznab", GetSettings(""));
            }
        }

        public Newznab(INewznabCapabilitiesProvider capabilitiesProvider, IIndexerHttpClient httpClient, IEventAggregator eventAggregator, IIndexerStatusService indexerStatusService, IConfigService configService, IValidateNzbs nzbValidationService, Logger logger)
            : base(httpClient, eventAggregator, indexerStatusService, configService, nzbValidationService, logger)
        {
            _capabilitiesProvider = capabilitiesProvider;
        }

        private IndexerDefinition GetDefinition(string name, NewznabSettings settings, IEnumerable<int> categories = null)
        {
            var caps = new IndexerCapabilities();

            if (categories != null)
            {
                foreach (var categoryId in categories)
                {
                    var mappedCat = NewznabStandardCategory.AllCats.FirstOrDefault(x => x.Id == categoryId);

                    if (mappedCat != null)
                    {
                        caps.Categories.AddCategoryMapping(mappedCat.Id, mappedCat);
                    }
                }
            }

            return new IndexerDefinition
            {
                Enable = true,
                Name = name,
                Implementation = GetType().Name,
                Settings = settings,
                Protocol = DownloadProtocol.Usenet,
                Privacy = IndexerPrivacy.Private,
                SupportsRss = SupportsRss,
                SupportsSearch = SupportsSearch,
                SupportsRedirect = SupportsRedirect,
                SupportsPagination = SupportsPagination,
                Capabilities = caps
            };
        }

        private NewznabSettings GetSettings(string url, string apiPath = null)
        {
            var settings = new NewznabSettings { BaseUrl = url };

            if (apiPath.IsNotNullOrWhiteSpace())
            {
                settings.ApiPath = apiPath;
            }

            return settings;
        }

        protected override async Task Test(List<ValidationFailure> failures)
        {
            await base.Test(failures);
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestCapabilities());
        }

        protected static List<int> CategoryIds(IndexerCapabilitiesCategories categories)
        {
            var l = categories.GetTorznabCategoryTree().Select(c => c.Id).ToList();

            return l;
        }

        protected virtual ValidationFailure TestCapabilities()
        {
            try
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings, Definition);

                if (capabilities.SearchParams != null && capabilities.SearchParams.Contains(SearchParam.Q))
                {
                    return null;
                }

                if (capabilities.MovieSearchParams != null &&
                    new[] { MovieSearchParam.Q, MovieSearchParam.ImdbId, MovieSearchParam.TmdbId, MovieSearchParam.TraktId }.Any(v => capabilities.MovieSearchParams.Contains(v)))
                {
                    return null;
                }

                if (capabilities.TvSearchParams != null &&
                    new[] { TvSearchParam.Q, TvSearchParam.TvdbId, TvSearchParam.ImdbId, TvSearchParam.TmdbId, TvSearchParam.RId }.Any(v => capabilities.TvSearchParams.Contains(v)) &&
                    new[] { TvSearchParam.Season, TvSearchParam.Ep }.All(v => capabilities.TvSearchParams.Contains(v)))
                {
                    return null;
                }

                if (capabilities.MusicSearchParams != null &&
                    new[] { MusicSearchParam.Q, MusicSearchParam.Artist, MusicSearchParam.Album }.Any(v => capabilities.MusicSearchParams.Contains(v)))
                {
                    return null;
                }

                if (capabilities.BookSearchParams != null &&
                    new[] { BookSearchParam.Q, BookSearchParam.Author, BookSearchParam.Title }.Any(v => capabilities.BookSearchParams.Contains(v)))
                {
                    return null;
                }

                return new ValidationFailure(string.Empty, "This indexer does not support searching for tv, music, or movies :(. Tell your indexer staff to enable this or force add the indexer by disabling search, adding the indexer and then enabling it again.");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer: " + ex.Message);

                return new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log above the ValidationFailure for more details");
            }
        }

        private int GetProviderPageSize()
        {
            try
            {
                return _capabilitiesProvider.GetCapabilities(Settings, Definition).LimitsDefault.GetValueOrDefault(100);
            }
            catch
            {
                return 100;
            }
        }
    }
}
