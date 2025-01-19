import PropTypes from 'prop-types';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Link from 'Components/Link/Link';
import formatDateTime from 'Utilities/Date/formatDateTime';
import translate from 'Utilities/String/translate';
import styles from './HistoryDetails.css';

function HistoryDetails(props) {
  const {
    indexer,
    eventType,
    date,
    data,
    shortDateFormat,
    timeFormat
  } = props;

  if (eventType === 'indexerQuery' || eventType === 'indexerRss') {
    const {
      query,
      queryResults,
      categories,
      limit,
      offset,
      source,
      host,
      url,
      elapsedTime,
      cached
    } = data;

    return (
      <DescriptionList>
        <DescriptionListItem
          descriptionClassName={styles.description}
          title={translate('Query')}
          data={query ? query : '-'}
        />

        {
          indexer ?
            <DescriptionListItem
              title={translate('Indexer')}
              data={indexer.name}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('QueryResults')}
              data={queryResults ? queryResults : '-'}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('Categories')}
              data={categories ? categories : '-'}
            /> :
            null
        }

        {
          limit ?
            <DescriptionListItem
              title={translate('Limit')}
              data={limit}
            /> :
            null
        }

        {
          offset ?
            <DescriptionListItem
              title={translate('Offset')}
              data={offset}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('Source')}
              data={source}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('Host')}
              data={host}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('Url')}
              data={url ? <Link to={url}>{translate('Link')}</Link> : '-'}
            /> :
            null
        }

        {
          elapsedTime ?
            <DescriptionListItem
              title={translate('ElapsedTime')}
              data={`${elapsedTime}ms${cached === '1' ? ' (cached)' : ''}`}
            /> :
            null
        }

        {
          date ?
            <DescriptionListItem
              title={translate('Date')}
              data={formatDateTime(date, shortDateFormat, timeFormat, { includeSeconds: true })}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'releaseGrabbed') {
    const {
      source,
      host,
      grabTitle,
      url,
      publishedDate,
      infoUrl,
      downloadClient,
      downloadClientName,
      elapsedTime,
      grabMethod
    } = data;

    const downloadClientNameInfo = downloadClientName ?? downloadClient;

    return (
      <DescriptionList>
        {
          indexer ?
            <DescriptionListItem
              title={translate('Indexer')}
              data={indexer.name}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('Source')}
              data={source ? source : '-'}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('Host')}
              data={host}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('GrabTitle')}
              data={grabTitle ? grabTitle : '-'}
            /> :
            null
        }

        {
          infoUrl ?
            <DescriptionListItem
              title={translate('InfoUrl')}
              data={<Link to={infoUrl}>{infoUrl}</Link>}
            /> :
            null
        }

        {
          publishedDate ?
            <DescriptionListItem
              title={translate('PublishedDate')}
              data={formatDateTime(publishedDate, shortDateFormat, timeFormat, { includeSeconds: true })}
            /> :
            null
        }

        {
          downloadClientNameInfo ?
            <DescriptionListItem
              title={translate('DownloadClient')}
              data={downloadClientNameInfo}
            /> :
            null
        }

        {
          data ?
            <DescriptionListItem
              title={translate('Url')}
              data={url ? <Link to={url}>{translate('Link')}</Link> : '-'}
            /> :
            null
        }

        {
          elapsedTime ?
            <DescriptionListItem
              title={translate('ElapsedTime')}
              data={`${elapsedTime}ms`}
            /> :
            null
        }

        {
          grabMethod ?
            <DescriptionListItem
              title={translate('Redirected')}
              data={grabMethod.toLowerCase() === 'redirect' ? translate('Yes') : translate('No')}
            /> :
            null
        }

        {
          date ?
            <DescriptionListItem
              title={translate('Date')}
              data={formatDateTime(date, shortDateFormat, timeFormat, { includeSeconds: true })}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  if (eventType === 'indexerAuth') {
    const { elapsedTime } = data;

    return (
      <DescriptionList
        descriptionClassName={styles.description}
        title={translate('Auth')}
      >
        {
          indexer ?
            <DescriptionListItem
              title={translate('Indexer')}
              data={indexer.name}
            /> :
            null
        }

        {
          elapsedTime ?
            <DescriptionListItem
              title={translate('ElapsedTime')}
              data={`${elapsedTime}ms`}
            /> :
            null
        }

        {
          date ?
            <DescriptionListItem
              title={translate('Date')}
              data={formatDateTime(date, shortDateFormat, timeFormat, { includeSeconds: true })}
            /> :
            null
        }
      </DescriptionList>
    );
  }

  return (
    <DescriptionList>
      <DescriptionListItem
        descriptionClassName={styles.description}
        title={translate('Name')}
        data={data.query}
      />

      {
        date ?
          <DescriptionListItem
            title={translate('Date')}
            data={formatDateTime(date, shortDateFormat, timeFormat, { includeSeconds: true })}
          /> :
          null
      }
    </DescriptionList>
  );
}

HistoryDetails.propTypes = {
  indexer: PropTypes.object.isRequired,
  eventType: PropTypes.string.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  timeFormat: PropTypes.string.isRequired
};

export default HistoryDetails;
