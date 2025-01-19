import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons, kinds } from 'Helpers/Props';
import styles from './HistoryEventTypeCell.css';

function getIconName(eventType) {
  switch (eventType) {
    case 'indexerQuery':
      return icons.SEARCH;
    case 'releaseGrabbed':
      return icons.DOWNLOAD;
    case 'indexerAuth':
      return icons.LOCK;
    case 'indexerRss':
      return icons.RSS;
    default:
      return icons.UNKNOWN;
  }
}

function getIconKind(successful, redirect) {
  if (redirect) {
    return kinds.INFO;
  } else if (!successful) {
    return kinds.DANGER;
  }

  return kinds.DEFAULT;
}

function getTooltip(eventType, data, indexer, redirect) {
  switch (eventType) {
    case 'indexerQuery':
      return `Query "${data.query}" sent to ${indexer.name}`;
    case 'releaseGrabbed':
      return redirect ? `Release grabbed via redirect from ${indexer.name}` : `Release grabbed from ${indexer.name}`;
    case 'indexerAuth':
      return `Auth attempted for ${indexer.name}`;
    case 'indexerRss':
      return `RSS query for ${indexer.name}`;
    default:
      return 'Unknown event';
  }
}

function HistoryEventTypeCell({ eventType, successful, data, indexer }) {
  const { grabMethod } = data;
  const redirect = grabMethod && grabMethod.toLowerCase() === 'redirect';

  const iconName = getIconName(eventType);
  const iconKind = getIconKind(successful, redirect);
  const tooltip = getTooltip(eventType, data, indexer, redirect);

  return (
    <TableRowCell
      className={styles.cell}
      title={tooltip}
    >
      <Icon
        name={iconName}
        kind={iconKind}
      />
    </TableRowCell>
  );
}

HistoryEventTypeCell.propTypes = {
  eventType: PropTypes.string.isRequired,
  successful: PropTypes.bool.isRequired,
  data: PropTypes.object,
  indexer: PropTypes.object
};

HistoryEventTypeCell.defaultProps = {
  data: {}
};

export default HistoryEventTypeCell;
