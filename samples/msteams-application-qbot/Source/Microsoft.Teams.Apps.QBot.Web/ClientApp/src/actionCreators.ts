import { QBotContext } from 'App';
import { useContext, useMemo } from 'react';
import { useDispatch } from 'react-redux';
import { bindActionCreators } from 'redux';
import { QBotApplication } from 'compositionRoot';

export type QBotActionCreators = QBotApplication['actionCreators'];

export function useActionCreator<
  ActionType,
  T extends (...args: any[]) => ActionType
>(selector: (actionCreators: QBotActionCreators) => T): T {
  const { actionCreators } = useContext(QBotContext);
  const actionCreator = selector(actionCreators);
  const dispatch = useDispatch();
  return useMemo(() => bindActionCreators(actionCreator, dispatch), [
    actionCreator,
    dispatch,
  ]);
}
