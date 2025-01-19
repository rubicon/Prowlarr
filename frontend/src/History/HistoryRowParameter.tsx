import React from 'react';
import Link from 'Components/Link/Link';
import { HistoryQueryType } from 'typings/History';
import styles from './HistoryRowParameter.css';

interface HistoryRowParameterProps {
  title: string;
  value: string;
  queryType: HistoryQueryType;
}

function HistoryRowParameter(props: HistoryRowParameterProps) {
  const { title, value, queryType } = props;

  const type = title.toLowerCase();

  let link = null;

  if (type === 'imdb') {
    link = <Link to={`https://imdb.com/title/${value}/`}>{value}</Link>;
  } else if (type === 'tmdb') {
    link = (
      <Link
        to={`https://www.themoviedb.org/${
          queryType === 'tvsearch' ? 'tv' : 'movie'
        }/${value}`}
      >
        {value}
      </Link>
    );
  } else if (type === 'tvdb') {
    link = (
      <Link to={`https://www.thetvdb.com/?tab=series&id=${value}`}>
        {value}
      </Link>
    );
  } else if (type === 'tvmaze') {
    link = <Link to={`https://www.tvmaze.com/shows/${value}/_`}>{value}</Link>;
  }

  return (
    <div className={styles.parameter}>
      <div className={styles.info}>
        <span>{title}</span>
      </div>

      <div className={styles.value}>{link ? link : value}</div>
    </div>
  );
}

export default HistoryRowParameter;
