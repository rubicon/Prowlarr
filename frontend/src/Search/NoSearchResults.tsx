import React from 'react';
import Alert from 'Components/Alert';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NoSearchResults.css';

interface NoSearchResultsProps {
  totalItems: number;
}

function NoSearchResults(props: NoSearchResultsProps) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <Alert kind={kinds.WARNING} className={styles.message}>
        {translate('AllSearchResultsHiddenByFilter')}
      </Alert>
    );
  }

  return (
    <Alert kind={kinds.INFO} className={styles.message}>
      {translate('NoSearchResultsFound')}
    </Alert>
  );
}

export default NoSearchResults;
