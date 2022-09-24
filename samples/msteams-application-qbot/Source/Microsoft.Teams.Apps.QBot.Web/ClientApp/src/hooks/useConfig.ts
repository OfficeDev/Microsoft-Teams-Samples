import { QBotContext } from 'App';
import { Configuration } from 'compositionRoot';
import { useContext } from 'react';

export function useConfig(): Configuration {
  const { configuration } = useContext(QBotContext);
  return configuration;
}
