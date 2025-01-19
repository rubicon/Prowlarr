import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export function createIndexerSelector(indexerId?: number) {
  return createSelector(
    (state: AppState) => state.indexers.itemMap,
    (state: AppState) => state.indexers.items,
    (itemMap, allIndexers) => {
      return indexerId ? allIndexers[itemMap[indexerId]] : undefined;
    }
  );
}

function useIndexer(indexerId?: number) {
  return useSelector(createIndexerSelector(indexerId));
}

export default useIndexer;
