import ModelBase from 'App/ModelBase';

export type HistoryQueryType =
  | 'search'
  | 'tvsearch'
  | 'movie'
  | 'book'
  | 'music';

export interface HistoryData {
  source: string;
  host: string;
  limit: number;
  offset: number;
  elapsedTime: number;
  query: string;
  queryType: HistoryQueryType;
}

interface History extends ModelBase {
  indexerId: number;
  date: string;
  successful: boolean;
  eventType: string;
  data: HistoryData;
}

export default History;
