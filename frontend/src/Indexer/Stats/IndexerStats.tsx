import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import IndexerStatsAppState from 'App/State/IndexerStatsAppState';
import Alert from 'Components/Alert';
import BarChart from 'Components/Chart/BarChart';
import DoughnutChart from 'Components/Chart/DoughnutChart';
import StackedBarChart from 'Components/Chart/StackedBarChart';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageContent from 'Components/Page/PageContent';
import PageContentBody from 'Components/Page/PageContentBody';
import PageToolbar from 'Components/Page/Toolbar/PageToolbar';
import PageToolbarButton from 'Components/Page/Toolbar/PageToolbarButton';
import PageToolbarSection from 'Components/Page/Toolbar/PageToolbarSection';
import { align, icons, kinds } from 'Helpers/Props';
import {
  fetchIndexerStats,
  setIndexerStatsFilter,
} from 'Store/Actions/indexerStatsActions';
import { createCustomFiltersSelector } from 'Store/Selectors/createClientSideCollectionSelector';
import {
  IndexerStatsHost,
  IndexerStatsIndexer,
  IndexerStatsUserAgent,
} from 'typings/IndexerStats';
import abbreviateNumber from 'Utilities/Number/abbreviateNumber';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import IndexerStatsFilterModal from './IndexerStatsFilterModal';
import styles from './IndexerStats.css';

function getAverageResponseTimeData(indexerStats: IndexerStatsIndexer[]) {
  const statistics = [...indexerStats].sort((a, b) =>
    a.averageResponseTime === b.averageResponseTime
      ? b.averageGrabResponseTime - a.averageGrabResponseTime
      : b.averageResponseTime - a.averageResponseTime
  );

  return {
    labels: statistics.map((indexer) => indexer.indexerName),
    datasets: [
      {
        label: translate('AverageQueries'),
        data: statistics.map((indexer) => indexer.averageResponseTime),
      },
      {
        label: translate('AverageGrabs'),
        data: statistics.map((indexer) => indexer.averageGrabResponseTime),
      },
    ],
  };
}

function getFailureRateData(indexerStats: IndexerStatsIndexer[]) {
  const data = [...indexerStats]
    .map((indexer) => ({
      label: indexer.indexerName,
      value:
        (indexer.numberOfFailedQueries +
          indexer.numberOfFailedRssQueries +
          indexer.numberOfFailedAuthQueries +
          indexer.numberOfFailedGrabs) /
        (indexer.numberOfQueries +
          indexer.numberOfRssQueries +
          indexer.numberOfAuthQueries +
          indexer.numberOfGrabs),
    }))
    .filter((s) => s.value > 0);

  data.sort((a, b) => b.value - a.value);

  return data;
}

function getTotalRequestsData(indexerStats: IndexerStatsIndexer[]) {
  const statistics = [...indexerStats]
    .filter(
      (s) =>
        s.numberOfQueries > 0 ||
        s.numberOfRssQueries > 0 ||
        s.numberOfAuthQueries > 0
    )
    .sort(
      (a, b) =>
        b.numberOfQueries +
        b.numberOfRssQueries +
        b.numberOfAuthQueries -
        (a.numberOfQueries + a.numberOfRssQueries + a.numberOfAuthQueries)
    );

  return {
    labels: statistics.map((indexer) => indexer.indexerName),
    datasets: [
      {
        label: translate('SearchQueries'),
        data: statistics.map((indexer) => indexer.numberOfQueries),
      },
      {
        label: translate('RssQueries'),
        data: statistics.map((indexer) => indexer.numberOfRssQueries),
      },
      {
        label: translate('AuthQueries'),
        data: statistics.map((indexer) => indexer.numberOfAuthQueries),
      },
    ],
  };
}

function getNumberGrabsData(indexerStats: IndexerStatsIndexer[]) {
  const data = [...indexerStats]
    .map((indexer) => ({
      label: indexer.indexerName,
      value: indexer.numberOfGrabs - indexer.numberOfFailedGrabs,
    }))
    .filter((s) => s.value > 0);

  data.sort((a, b) => b.value - a.value);

  return data;
}

function getUserAgentGrabsData(indexerStats: IndexerStatsUserAgent[]) {
  const data = indexerStats.map((indexer) => ({
    label: indexer.userAgent ? indexer.userAgent : 'Other',
    value: indexer.numberOfGrabs,
  }));

  data.sort((a, b) => b.value - a.value);

  return data;
}

function getUserAgentQueryData(indexerStats: IndexerStatsUserAgent[]) {
  const data = indexerStats.map((indexer) => ({
    label: indexer.userAgent ? indexer.userAgent : 'Other',
    value: indexer.numberOfQueries,
  }));

  data.sort((a, b) => b.value - a.value);

  return data;
}

function getHostGrabsData(indexerStats: IndexerStatsHost[]) {
  const data = indexerStats.map((indexer) => ({
    label: indexer.host ? indexer.host : 'Other',
    value: indexer.numberOfGrabs,
  }));

  data.sort((a, b) => b.value - a.value);

  return data;
}

function getHostQueryData(indexerStats: IndexerStatsHost[]) {
  const data = indexerStats.map((indexer) => ({
    label: indexer.host ? indexer.host : 'Other',
    value: indexer.numberOfQueries,
  }));

  data.sort((a, b) => b.value - a.value);

  return data;
}

const indexerStatsSelector = () => {
  return createSelector(
    (state: AppState) => state.indexerStats,
    createCustomFiltersSelector('indexerStats'),
    (indexerStats: IndexerStatsAppState, customFilters) => {
      return {
        ...indexerStats,
        customFilters,
      };
    }
  );
};

function IndexerStats() {
  const {
    isFetching,
    isPopulated,
    item,
    error,
    filters,
    customFilters,
    selectedFilterKey,
  } = useSelector(indexerStatsSelector());
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(fetchIndexerStats());
  }, [dispatch]);

  const onRefreshPress = useCallback(() => {
    dispatch(fetchIndexerStats());
  }, [dispatch]);

  const onFilterSelect = useCallback(
    (value: string) => {
      dispatch(setIndexerStatsFilter({ selectedFilterKey: value }));
    },
    [dispatch]
  );

  const isLoaded = !error && isPopulated;
  const indexerCount = item.indexers?.length ?? 0;
  const userAgentCount = item.userAgents?.length ?? 0;
  const queryCount =
    item.indexers?.reduce((total, indexer) => {
      return (
        total +
        indexer.numberOfQueries +
        indexer.numberOfRssQueries +
        indexer.numberOfAuthQueries
      );
    }, 0) ?? 0;
  const grabCount =
    item.indexers?.reduce((total, indexer) => {
      return total + indexer.numberOfGrabs;
    }, 0) ?? 0;

  return (
    <PageContent title={translate('Stats')}>
      <PageToolbar>
        <PageToolbarSection>
          <PageToolbarButton
            label={translate('Refresh')}
            iconName={icons.REFRESH}
            isSpinning={isFetching}
            onPress={onRefreshPress}
          />
        </PageToolbarSection>

        <PageToolbarSection alignContent={align.RIGHT} collapseButtons={false}>
          <FilterMenu
            alignMenu={align.RIGHT}
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={customFilters}
            filterModalConnectorComponent={IndexerStatsFilterModal}
            isDisabled={false}
            onFilterSelect={onFilterSelect}
          />
        </PageToolbarSection>
      </PageToolbar>
      <PageContentBody>
        {isFetching && !isPopulated && <LoadingIndicator />}

        {!isFetching && !!error && (
          <Alert kind={kinds.DANGER}>
            {getErrorMessage(error, 'Failed to load indexer stats from API')}
          </Alert>
        )}

        {isLoaded && (
          <div>
            <div className={styles.quarterWidthChart}>
              <div className={styles.statContainer}>
                <div className={styles.statTitle}>
                  {translate('ActiveIndexers')}
                </div>
                <div className={styles.stat}>{indexerCount}</div>
              </div>
            </div>
            <div className={styles.quarterWidthChart}>
              <div className={styles.statContainer}>
                <div className={styles.statTitle}>
                  {translate('TotalQueries')}
                </div>
                <div className={styles.stat}>
                  {abbreviateNumber(queryCount)}
                </div>
              </div>
            </div>
            <div className={styles.quarterWidthChart}>
              <div className={styles.statContainer}>
                <div className={styles.statTitle}>
                  {translate('TotalGrabs')}
                </div>
                <div className={styles.stat}>{abbreviateNumber(grabCount)}</div>
              </div>
            </div>
            <div className={styles.quarterWidthChart}>
              <div className={styles.statContainer}>
                <div className={styles.statTitle}>
                  {translate('ActiveApps')}
                </div>
                <div className={styles.stat}>{userAgentCount}</div>
              </div>
            </div>
            <div className={styles.fullWidthChart}>
              <div className={styles.chartContainer}>
                <StackedBarChart
                  data={getAverageResponseTimeData(item.indexers)}
                  title={translate('AverageResponseTimesMs')}
                  stepSize={100}
                />
              </div>
            </div>
            <div className={styles.fullWidthChart}>
              <div className={styles.chartContainer}>
                <BarChart
                  data={getFailureRateData(item.indexers)}
                  title={translate('IndexerFailureRate')}
                  stepSize={0.1}
                  kind={kinds.WARNING}
                />
              </div>
            </div>
            <div className={styles.halfWidthChart}>
              <div className={styles.chartContainer}>
                <StackedBarChart
                  data={getTotalRequestsData(item.indexers)}
                  title={translate('TotalIndexerQueries')}
                />
              </div>
            </div>
            <div className={styles.halfWidthChart}>
              <div className={styles.chartContainer}>
                <BarChart
                  data={getNumberGrabsData(item.indexers)}
                  title={translate('TotalIndexerSuccessfulGrabs')}
                />
              </div>
            </div>
            <div className={styles.halfWidthChart}>
              <div className={styles.chartContainer}>
                <BarChart
                  data={getUserAgentQueryData(item.userAgents)}
                  title={translate('TotalUserAgentQueries')}
                  horizontal={true}
                />
              </div>
            </div>
            <div className={styles.halfWidthChart}>
              <div className={styles.chartContainer}>
                <BarChart
                  data={getUserAgentGrabsData(item.userAgents)}
                  title={translate('TotalUserAgentGrabs')}
                  horizontal={true}
                />
              </div>
            </div>
            <div className={styles.halfWidthChart}>
              <div className={styles.chartContainer}>
                <DoughnutChart
                  data={getHostQueryData(item.hosts)}
                  title={translate('TotalHostQueries')}
                  horizontal={true}
                />
              </div>
            </div>
            <div className={styles.halfWidthChart}>
              <div className={styles.chartContainer}>
                <DoughnutChart
                  data={getHostGrabsData(item.hosts)}
                  title={translate('TotalHostGrabs')}
                  horizontal={true}
                />
              </div>
            </div>
          </div>
        )}
      </PageContentBody>
    </PageContent>
  );
}

export default IndexerStats;
