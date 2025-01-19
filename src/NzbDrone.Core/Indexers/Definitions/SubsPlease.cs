using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Exceptions;
using NzbDrone.Core.Indexers.Settings;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.Definitions
{
    public class SubsPlease : TorrentIndexerBase<NoAuthTorrentBaseSettings>
    {
        public override string Name => "SubsPlease";
        public override string[] IndexerUrls => new[]
        {
            "https://subsplease.org/",
            "https://subsplease.mrunblock.bond/",
            "https://subsplease.nocensor.click/"
        };
        public override string[] LegacyUrls => new[]
        {
            "https://subsplease.nocensor.space/"
        };
        public override string Language => "en-US";
        public override string Description => "SubsPlease - A better HorribleSubs/Erai replacement";
        public override Encoding Encoding => Encoding.UTF8;
        public override IndexerPrivacy Privacy => IndexerPrivacy.Public;
        public override IndexerCapabilities Capabilities => SetCapabilities();

        public SubsPlease(IIndexerHttpClient httpClient, IEventAggregator eventAggregator, IIndexerStatusService indexerStatusService, IConfigService configService, Logger logger)
            : base(httpClient, eventAggregator, indexerStatusService, configService, logger)
        {
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new SubsPleaseRequestGenerator(Settings);
        }

        public override IParseIndexerResponse GetParser()
        {
            return new SubsPleaseParser(Settings);
        }

        private IndexerCapabilities SetCapabilities()
        {
            var caps = new IndexerCapabilities
            {
                TvSearchParams = new List<TvSearchParam>
                {
                    TvSearchParam.Q, TvSearchParam.Season, TvSearchParam.Ep
                },
                MovieSearchParams = new List<MovieSearchParam>
                {
                    MovieSearchParam.Q
                }
            };

            caps.Categories.AddCategoryMapping(1, NewznabStandardCategory.TVAnime);
            caps.Categories.AddCategoryMapping(2, NewznabStandardCategory.MoviesOther);

            return caps;
        }
    }

    public class SubsPleaseRequestGenerator : IIndexerRequestGenerator
    {
        private static readonly Regex ResolutionRegex = new (@"\d{3,4}p", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly NoAuthTorrentBaseSettings _settings;

        public SubsPleaseRequestGenerator(NoAuthTorrentBaseSettings settings)
        {
            _settings = settings;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetSearchRequests(searchCriteria.SanitizedSearchTerm, searchCriteria));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MusicSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(TvSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var searchTerm = searchCriteria.SanitizedSearchTerm.Trim();

            // Only include season > 1 in searchTerm, format as S2 rather than S02
            if (searchCriteria.Season is > 1)
            {
                searchTerm += $" S{searchCriteria.Season}";
            }

            if (int.TryParse(searchCriteria.Episode, out var episode) && episode > 0)
            {
                searchTerm += $" {episode:00}";
            }

            pageableRequests.Add(GetSearchRequests(searchTerm, searchCriteria));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(BookSearchCriteria searchCriteria)
        {
            return new IndexerPageableRequestChain();
        }

        public IndexerPageableRequestChain GetSearchRequests(BasicSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetSearchRequests(searchCriteria.SanitizedSearchTerm, searchCriteria));

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetSearchRequests(string term, SearchCriteriaBase searchCriteria)
        {
            var queryParameters = new NameValueCollection
            {
                { "tz", "UTC" }
            };

            if (searchCriteria.IsRssSearch)
            {
                queryParameters.Set("f", "latest");
            }
            else
            {
                var searchTerm = Regex.Replace(term, "\\[?SubsPlease\\]?\\s*", string.Empty, RegexOptions.IgnoreCase).Trim();

                // If the search terms contain a resolution, remove it from the query sent to the API
                var resolutionMatch = ResolutionRegex.Match(searchTerm);

                if (resolutionMatch.Success)
                {
                    searchTerm = searchTerm.Replace(resolutionMatch.Value, string.Empty).Trim();
                }

                queryParameters.Set("f", "search");
                queryParameters.Set("s", searchTerm);
            }

            var searchUrl = $"{_settings.BaseUrl.TrimEnd('/')}/api/?{queryParameters.GetQueryString()}";

            yield return new IndexerRequest(searchUrl, HttpAccept.Json);
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }

    public class SubsPleaseParser : IParseIndexerResponse
    {
        private static readonly Regex RegexSize = new (@"\&xl=(?<size>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly NoAuthTorrentBaseSettings _settings;

        public SubsPleaseParser(NoAuthTorrentBaseSettings settings)
        {
            _settings = settings;
        }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse indexerResponse)
        {
            var torrentInfos = new List<ReleaseInfo>();

            if (indexerResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new IndexerException(indexerResponse, $"Unexpected response status {indexerResponse.HttpResponse.StatusCode} code from indexer request");
            }

            // When there are no results, the API returns an empty array or empty response instead of an object
            if (string.IsNullOrWhiteSpace(indexerResponse.Content) || indexerResponse.Content == "[]")
            {
                return torrentInfos;
            }

            var jsonResponse = new HttpResponse<Dictionary<string, SubPleaseRelease>>(indexerResponse.HttpResponse);

            foreach (var value in jsonResponse.Resource.Values)
            {
                foreach (var d in value.Downloads)
                {
                    var release = new TorrentInfo
                    {
                        InfoUrl = $"{_settings.BaseUrl}shows/{value.Page}/",
                        PublishDate = value.ReleaseDate.LocalDateTime,
                        Files = 1,
                        Categories = new List<IndexerCategory> { NewznabStandardCategory.TVAnime },
                        Seeders = 1,
                        Peers = 2,
                        MinimumRatio = 1,
                        MinimumSeedTime = 172800, // 48 hours
                        DownloadVolumeFactor = 0,
                        UploadVolumeFactor = 1
                    };

                    if (value.ImageUrl.IsNotNullOrWhiteSpace())
                    {
                        release.PosterUrl = _settings.BaseUrl + value.ImageUrl.TrimStart('/');
                    }

                    if (value.Episode.ToLowerInvariant() == "movie")
                    {
                        release.Categories.Add(NewznabStandardCategory.MoviesOther);
                    }

                    // Ex: [SubsPlease] Shingeki no Kyojin (The Final Season) - 64 (1080p)
                    release.Title = $"[SubsPlease] {value.Show} - {value.Episode} ({d.Resolution}p)";
                    release.MagnetUrl = d.Magnet;
                    release.DownloadUrl = null;
                    release.Guid = d.Magnet;
                    release.Size = GetReleaseSize(d);

                    torrentInfos.Add(release);
                }
            }

            return torrentInfos.ToArray();
        }

        private static long GetReleaseSize(SubPleaseDownloadInfo info)
        {
            if (info.Magnet.IsNotNullOrWhiteSpace())
            {
                var sizeMatch = RegexSize.Match(info.Magnet);

                if (sizeMatch.Success &&
                    long.TryParse(sizeMatch.Groups["size"].Value, out var releaseSize)
                    && releaseSize > 0)
                {
                    return releaseSize;
                }
            }

            // The API doesn't tell us file size, so give an estimate based on resolution
            return info.Resolution switch
            {
                "1080" => 1.3.Gigabytes(),
                "720" => 700.Megabytes(),
                "480" => 350.Megabytes(),
                _ => 1.Gigabytes()
            };
        }

        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }

    public class SubPleaseRelease
    {
        public string Time { get; set; }

        [JsonProperty("release_date")]
        public DateTimeOffset ReleaseDate { get; set; }
        public string Show { get; set; }
        public string Episode { get; set; }
        public SubPleaseDownloadInfo[] Downloads { get; set; }
        public string Xdcc { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
        public string Page { get; set; }
    }

    public class SubPleaseDownloadInfo
    {
        [JsonProperty("res")]
        public string Resolution { get; set; }
        public string Magnet { get; set; }
    }
}
