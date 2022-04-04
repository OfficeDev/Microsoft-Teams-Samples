/* eslint-disable @typescript-eslint/no-explicit-any */
import { ThemePrepared, useFluentContext } from '@fluentui/react-northstar';

export function useColorScheme<TKey extends string>(
  schemeName: string,
): Record<TKey, any> {
  return useTheme().siteVariables.colorScheme[schemeName];
}

export function useTheme(): ThemePrepared<any> {
  const context = useFluentContext();
  return context.theme;
}

export function useDefaultColorScheme(): Record<any, any> {
  return useColorScheme<any>('default');
}
