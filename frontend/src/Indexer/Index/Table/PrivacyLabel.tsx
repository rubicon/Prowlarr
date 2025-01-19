import React from 'react';
import Label from 'Components/Label';
import { IndexerPrivacy } from 'Indexer/Indexer';
import firstCharToUpper from 'Utilities/String/firstCharToUpper';
import translate from 'Utilities/String/translate';
import styles from './PrivacyLabel.css';

interface PrivacyLabelProps {
  privacy: IndexerPrivacy;
}

function PrivacyLabel({ privacy }: PrivacyLabelProps) {
  return (
    <Label className={styles[`${privacy}Label`]}>
      {translate(firstCharToUpper(privacy))}
    </Label>
  );
}

export default PrivacyLabel;
